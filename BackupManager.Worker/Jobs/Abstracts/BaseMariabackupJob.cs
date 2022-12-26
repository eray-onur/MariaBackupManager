using BackupManager.Worker.Services;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupManager.Worker.Jobs.Abstracts
{
    public class BaseMariabackupJob : IJob
    {
        public BackupType BackupType { get; set; }
        private readonly IConfiguration _config;
        private readonly IFileLoggingService _fileLoggingService;
        public BaseMariabackupJob(IConfiguration config, IFileLoggingService fileLoggingService)
        {
            _config = config;
            _fileLoggingService = fileLoggingService;
        }

        private string DetermineBackupDirectory()
        {
            var backupSettings = _config.GetSection("BackupSettings");
            switch (BackupType)
            {
                case BackupType.Hourly:
                    return backupSettings.GetValue<string>("HourlyBackupDirectoryName");
                case BackupType.Daily:
                    return backupSettings.GetValue<string>("DailyBackupDirectoryName");
                case BackupType.Weekly:
                    return backupSettings.GetValue<string>("WeeklyBackupDirectoryName");
                default:
                    return string.Empty;
            }
        }
        public Task Execute(IJobExecutionContext context)
        {
            ProcessStartInfo ProcessInfo;
            Process? Process;

            _fileLoggingService.AppendToFile("[Backup]", $"Hourly backup process has started.");

            // Retrieving Config data.
            var backupSettings = _config.GetSection("BackupSettings");
            string username = backupSettings.GetValue<string>("BackupUsername");
            string password = backupSettings.GetValue<string>("BackupPassword");
            string backupDirRoot = backupSettings.GetValue<string>("BackupDirectoryRoot");
            string backupDirName = DetermineBackupDirectory();

            string Command = $"--backup --target-dir={backupDirRoot}{backupDirName} " +
                $"--user={username} --password={password}";

            _fileLoggingService.AppendToFile("[Backup]", $"Command to execute: {Command}");

            ProcessInfo = new ProcessStartInfo() { FileName = "mariabackup", Arguments = Command };
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;
            ProcessInfo.RedirectStandardOutput = true;

            try
            {
                Process = Process.Start(ProcessInfo);

                if (Process == null) return Task.FromResult(0);
                Process.WaitForExit();
                Process.Close();

            }
            catch (Exception ex)
            {
                _fileLoggingService.AppendToFile("[BackupJobError]", ex.Message);
                return Task.FromResult(0);
            }



            return Task.FromResult(1);
        }
    }
}
