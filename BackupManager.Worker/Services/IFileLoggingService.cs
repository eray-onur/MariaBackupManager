using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupManager.Worker.Services
{
    public interface IFileLoggingService
    {
        Task AppendToFile(string tag, string text);
    }
}
