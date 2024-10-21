using Android.App;
using Android.Content;
using Android.Provider;
using CustomFileOpenerAndSaver.Interfaces;
using CustomFileOpenerAndSaver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFileOpenerAndSaver.Platforms.Android.Services
{
    internal class FileSaverService : IFileSaverService
    {
        private static TaskCompletionSource<TransferFile> saveFileTcs;
        private static TransferFile fileToSave;

        public Task<TransferFile> SaveFileAsync(TransferFile transferFile)
        {
            fileToSave = transferFile;
            saveFileTcs = new TaskCompletionSource<TransferFile>();

            var intent = new Intent(Intent.ActionCreateDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("application/octet-stream");
            intent.PutExtra(Intent.ExtraTitle, transferFile.Name + transferFile.Extension);

            MainActivity.Instance.StartActivityForResult(intent, 1000);

            return saveFileTcs.Task;
        }

        public static void OnSaveFileActivityResult(Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok && data?.Data != null)
            {
                var uri = data.Data;
                try
                {
                    using (var outputStream = MainActivity.Instance.ContentResolver.OpenOutputStream(uri))
                    {
                        if (outputStream != null)
                        {
                            var fileBytesToSave = Convert.FromBase64String(fileToSave.Content);
                            outputStream.Write(fileBytesToSave, 0, fileBytesToSave.Length);
                            outputStream.Flush();

                            // Получение имени файла и расширения
                            var fileName = GetFileName(uri);
                            var fileExtension = Path.GetExtension(fileName).TrimStart('.');

                            if (fileName.EndsWith(".tdbkp"))
                            {
                                fileName = fileName.Substring(0, fileName.Length - ".tdbkp".Length);
                            }

                            // Заполнение модели TransferFile
                            fileToSave.Path = uri.ToString(); // Сохраняем URI файла
                            fileToSave.Name = fileName;
                            fileToSave.Extension = fileExtension;
                            fileToSave.Content = Convert.ToBase64String(fileBytesToSave); // Контент остается тот же

                            saveFileTcs.SetResult(fileToSave); // Успешное сохранение
                        }
                        else
                        {
                            fileToSave.Error = new Error { Code = "StreamError", Message = "Ошибка открытия потока" };
                            saveFileTcs.SetResult(fileToSave); // Ошибка открытия потока
                        }
                    }
                }
                catch (Exception ex)
                {
                    fileToSave.Error = new Error { Code = "SaveError", Message = ex.Message };
                    saveFileTcs.SetException(ex); // Ошибка при сохранении
                }
            }
            else
            {
                fileToSave.Error = new Error { Code = "Canceled", Message = "Сохранение отменено" };
                saveFileTcs.SetResult(fileToSave); // Сохранение отменено
            }
        }

        private static string GetFileName(global::Android.Net.Uri uri)
        {
            string fileName = null;
            var cursor = MainActivity.Instance.ContentResolver.Query(uri, null, null, null, null);
            if (cursor != null && cursor.MoveToFirst())
            {
                int nameIndex = cursor.GetColumnIndex(OpenableColumns.DisplayName);
                if (nameIndex != -1)
                {
                    fileName = cursor.GetString(nameIndex);
                }
                cursor.Close();
            }
            return fileName;
        }
    }

}
