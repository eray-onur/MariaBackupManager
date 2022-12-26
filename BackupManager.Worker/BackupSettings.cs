using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupManager.Worker
{
    public class BackupSettings
    {
        public string BackupUsername { get; set; }
        public string BackupPassword { get; set; }
        public string BackupDirectoryRoot { get; set; }
        public string HourlyBackupDirectoryName { get; set; }
        public string DailyBackupDirectoryName { get; set; }
        public string WeeklyBackupDirectoryName { get; set; }
    }
}
