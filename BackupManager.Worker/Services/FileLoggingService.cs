using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupManager.Worker.Services
{
    public class FileLoggingService : IFileLoggingService
    {
        private string LogFilePath => Path.Join(AppContext.BaseDirectory, "log");
        public FileLoggingService()
        {
            if(!File.Exists(LogFilePath))
            {
                File.Create(LogFilePath);
            }
        }

        public async Task AppendToFile(string tag, string text)
        {
            string files = string.Join("\n", tag," - ", text, " - ", DateTime.Now.ToString(), "\n");
            await File.AppendAllTextAsync(Path.Join(AppContext.BaseDirectory, "log"), files);
        }
    }
}
