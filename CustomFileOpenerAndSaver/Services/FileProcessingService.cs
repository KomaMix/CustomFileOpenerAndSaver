using CustomFileOpenerAndSaver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFileOpenerAndSaver.Services
{
    public class FileProcessingService
    {
        private readonly string defaultDirectory = FileSystem.Current.AppDataDirectory;
        private readonly string defaultExtension = ".tdbkp";

        // Метод для сохранения файла
        public async Task<TransferFile> SaveFile(TransferFile file)
        {
            try
            {
                // Устанавливаем директорию, если не указана, используем AppDataDirectory
                string directory = string.IsNullOrWhiteSpace(file.Path) ? defaultDirectory : file.Path;

                // Если имя файла не указано, генерируем его
                string fileName = string.IsNullOrWhiteSpace(file.Name) ? $"file_{Guid.NewGuid()}" : file.Name;

                // Устанавливаем расширение файла, если не указано
                string extension = string.IsNullOrWhiteSpace(file.Extension) ? defaultExtension : file.Extension;

                // Формируем полный путь к файлу
                string filePath = Path.Combine(directory, $"{fileName}{extension}");

                // Преобразуем содержимое файла из Base64 в двоичные данные
                byte[] fileBytes = Convert.FromBase64String(file.Content);

                // Сохраняем файл
                await File.WriteAllBytesAsync(filePath, fileBytes);

                // Возвращаем обновленный TransferFile с указанным путем
                file.Path = filePath;
                file.Error = null; // Нет ошибки
            }
            catch (Exception ex)
            {
                // В случае ошибки записываем сообщение об ошибке в объект TransferFile
                file.Error = new Error { Code = "SaveError", Message = ex.Message };
            }

            return file;
        }

        // Метод для получения всех файлов с указанным расширением из директории
        public async Task<string[]> GetFiles(TransferFile file)
        {
            try
            {
                // Устанавливаем директорию, если не указана, используем AppDataDirectory
                string directory = string.IsNullOrWhiteSpace(file.Path) ? defaultDirectory : file.Path;

                // Устанавливаем расширение, если не указано
                string extension = string.IsNullOrWhiteSpace(file.Extension) ? defaultExtension : file.Extension;

                // Получаем список файлов с указанным расширением
                string[] files = Directory.GetFiles(directory, $"*{extension}");

                return files;
            }
            catch (Exception ex)
            {
                // В случае ошибки возвращаем пустой массив и логируем ошибку
                file.Error = new Error { Code = "GetFilesError", Message = ex.Message };
                return Array.Empty<string>();
            }
        }

        // Метод для открытия файла и возвращения TransferFile с содержимым
        public async Task<TransferFile> OpenFile(TransferFile file)
        {
            try
            {
                // Устанавливаем полный путь файла
                string filePath = string.IsNullOrWhiteSpace(file.Path) ? throw new ArgumentException("File path is required") : file.Path;

                // Читаем содержимое файла как двоичные данные
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

                // Преобразуем двоичные данные в Base64 для хранения в TransferFile
                file.Content = Convert.ToBase64String(fileBytes);

                // Получаем имя файла и расширение
                file.Name = Path.GetFileNameWithoutExtension(filePath);
                file.Extension = Path.GetExtension(filePath);

                file.Error = null; // Нет ошибки
            }
            catch (Exception ex)
            {
                // В случае ошибки записываем сообщение об ошибке в объект TransferFile
                file.Error = new Error { Code = "OpenError", Message = ex.Message };
            }

            return file;
        }
    }
}
