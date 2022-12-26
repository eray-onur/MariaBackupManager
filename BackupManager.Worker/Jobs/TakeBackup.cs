using BackupManager.Worker.Services;
using Quartz;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BackupManager.Worker.Jobs
{
    public class TakeBackup : IJob
    {
        private readonly IConfiguration _config;
        private readonly IFileLoggingService _fileLoggingService;
        public TakeBackup(IConfiguration config, IFileLoggingService fileLoggingService)
        {
            _config = config;
            _fileLoggingService = fileLoggingService;
        }
        private string DetermineBackupDirectory(BackupFrequency backupFrequency)
        {
            var backupSettings = _config.GetSection("BackupSettings");
            switch (backupFrequency)
            {
                case BackupFrequency.Hourly:
                    return backupSettings.GetValue<string>("HourlyBackupDirectoryName");
                case BackupFrequency.Daily:
                    return backupSettings.GetValue<string>("DailyBackupDirectoryName");
                case BackupFrequency.Weekly:
                    return backupSettings.GetValue<string>("WeeklyBackupDirectoryName");
                default:
                    return string.Empty;
            }
        }
        public Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            BackupFrequency frequency = (BackupFrequency) dataMap.GetInt("frequency");

            ProcessStartInfo ProcessInfo;
            Process? Process;

            _fileLoggingService.AppendToFile("[Backup]", $"Hourly backup process has started.");

            // Retrieving Config data.
            var backupSettings = _config.GetSection("BackupSettings");
            string username = backupSettings.GetValue<string>("BackupUsername");
            string password = backupSettings.GetValue<string>("BackupPassword");
            string backupDirRoot = backupSettings.GetValue<string>("BackupDirectoryRoot");
            string hourlyBackupDir = backupSettings.GetValue<string>("HourlyBackupDirectoryName");

            string Command = $"--backup --target-dir={backupDirRoot}{DetermineBackupDirectory(frequency)} " +
                $"--user={username} --password={password}";

            _fileLoggingService.AppendToFile("[Backup]", $"Command to execute: {Command}");

            ProcessInfo = new ProcessStartInfo() { FileName = "mariabackup", Arguments = Command};
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
            catch(Exception ex)
            {
                _fileLoggingService.AppendToFile("[BackupJobError]", ex.Message);
                return Task.FromResult(0);
            }

            

            return Task.FromResult(1);
        }
    }
}
