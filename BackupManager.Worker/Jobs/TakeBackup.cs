using BackupManager.Worker.Jobs.Abstracts;
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
            string hourlyBackupDir = backupSettings.GetValue<string>("HourlyBackupDirectoryName");
            //
            //string Command = $"mariabackup --backup --target-dir={backupDirRoot}{hourlyBackupDir} " +
            //    $"--user={username} --password={password}";
            string Command = $"--backup --target-dir={backupDirRoot}{hourlyBackupDir} " +
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
