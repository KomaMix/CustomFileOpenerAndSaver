using CustomFileOpenerAndSaver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFileOpenerAndSaver.Interfaces
{
    // Переимновать Storage
    internal interface IInternalStorageManager
    {
        public Task<TransferFile> CreateFileAsync(TransferFile file);
        public Task<TransferFile> GetFileContentAsync(TransferFile file);
        public Task<TransferFile> OverwriteFileAsync(TransferFile file);
        public List<TransferFile> GetAllFileNames();
        public TransferFile DeleteFile(TransferFile file);
        public bool FileExists(string fileName, string extension);
        
    }
}
