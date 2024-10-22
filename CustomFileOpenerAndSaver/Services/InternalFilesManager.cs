using CustomFileOpenerAndSaver.Interfaces;
using CustomFileOpenerAndSaver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFileOpenerAndSaver.Services
{
    internal class InternalFilesManager : IInternalFilesManager
    {
        private readonly string _storagePath;

        public InternalFilesManager()
        {
            // Тут идет сохранение во внешнюю память. Но она внешняя не в том смысле, что без проблем можем получать доступ
            // Внешняя в том смысле, что она доступна пользователю
            // При этом файлы, созданными разными приложениями будут знать, какое приложение их создало

            // Тут можно также указать на внутреннюю папку приложения
#if ANDROID
            // Можно так же указать в внутренней памяти приложения. Все будет работать
            //var internalRoot = Android.App.Application.Context.FilesDir?.AbsolutePath;

            // Получаем путь к Documents (внешняя память)
            var externalRoot = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments)?.AbsolutePath;


            // Создаем путь к папке Ridan внутри папки Downloads
            var _externalStoragePath = Path.Combine(externalRoot, "Ridan");

            // Проверяем, существует ли папка Ridan, если нет — создаем
            if (!Directory.Exists(_externalStoragePath))
            {
                Directory.CreateDirectory(_externalStoragePath);
            }

            _storagePath = _externalStoragePath;
#endif
        }

        // Создание нового файла
        public async Task<TransferFile> CreateFileAsync(TransferFile file)
        {
            if (FileExists(file.Name, file.Extension))
            {
                file.Error = new Error
                {
                    Code = "FileAlreadyExists",
                    Message = "Файл уже существует"
                };
                return file;
            }

            if (string.IsNullOrEmpty(file.Content))
            {
                file.Error = new Error
                {
                    Code = "NoContent",
                    Message = "Пустой контент"
                };
                return file;
            }

            var fullPath = GetFullPath(file.Name, file.Extension);

            try
            {
                await WriteFileAsync(fullPath, file.Content);

                file.Path = fullPath;

                return file;
            }
            catch (Exception ex)
            {
                file.Error = new Error
                {
                    Code = ex.GetType().Name,
                    Message = ex.Message,
                };

                return file;
            }
        }

        //
        public async Task<TransferFile> GetFileContentAsync(TransferFile file)
        {
            if (!FileExists(file.Name, file.Extension))
            {
                file.Error = new Error
                {
                    Code = "FileNotFound",
                    Message = "Файл не найден"
                };

                return file;
            }

            var fullPath = GetFullPath(file.Name, file.Extension);

            try
            {
                var contentBytes = await ReadFileAsync(fullPath);

                file.Content = Convert.ToBase64String(contentBytes);
                file.Path = fullPath;

                return file;
            }
            catch (Exception ex)
            {
                file.Error = new Error
                {
                    Code = ex.GetType().Name,
                    Message = ex.Message
                };

                return file;
            }
        }

        public List<TransferFile> GetAllFileNames()
        {
//#if ANDROID
//            // Проверка доступа ко всей внешней памяти
//            // Проверяет наличие разрешения MANAGE_EXTERNAL_STORAGE
//            Platforms.Android.CheckPermissionManager.CheckExternalStoragePermission();
//#endif

            var files = Directory.GetFiles(_storagePath);
            var transferFiles = new List<TransferFile>();

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var fileExtension = Path.GetExtension(file);

                transferFiles.Add(new TransferFile
                {
                    Name = fileName,
                    Extension = fileExtension
                });
            }

            return transferFiles;
        }

        public async Task<TransferFile> OverwriteFileAsync(TransferFile file)
        {
            if (!FileExists(file.Name, file.Extension))
            {
                file.Error = new Error
                {
                    Code = "FileNotFound",
                    Message = "Файл для перезаписи не найден"
                };

                return file;
            }

            var fullPath = GetFullPath(file.Name, file.Extension);

            try
            {
                await WriteFileAsync(fullPath, file.Content);

                file.Path = fullPath;

                return file;
            }
            catch (Exception ex)
            {
                file.Error = new Error
                {
                    Code = ex.GetType().Name,
                    Message = ex.Message
                };
                return file;
            }
        }

        public TransferFile DeleteFile(TransferFile file)
        {
            if (!FileExists(file.Name, file.Extension))
            {
                file.Error = new Error
                {
                    Code = "FileNotFound",
                    Message = "Файл для удаления не найден"
                };
                return file;
            }

            var fullPath = GetFullPath(file.Name, file.Extension);

            try
            {
                File.Delete(fullPath);
                return file;
            }
            catch (Exception ex)
            {
                file.Error = new Error
                {
                    Code = ex.GetType().Name,
                    Message = ex.Message
                };
                return file;
            }
        }

        public bool FileExists(string fileName, string extension)
        {
            var fullPath = GetFullPath(fileName, extension);
            return File.Exists(fullPath);
        }

        private async Task WriteFileAsync(string fullPath, string contentBase64)
        {
            var contentBytes = Convert.FromBase64String(contentBase64);
            await File.WriteAllBytesAsync(fullPath, contentBytes);
        }

        public string GetFullPath(string fileName, string extension)
        {
            var fileExtension = extension.StartsWith(".") ? extension : "." + extension;
            return Path.Combine(_storagePath, fileName + fileExtension);
        }

        private async Task<byte[]> ReadFileAsync(string fullPath)
        {
            return await File.ReadAllBytesAsync(fullPath);
        }
    }

}
