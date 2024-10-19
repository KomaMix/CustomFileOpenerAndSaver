using Android.App;
using Android.Content;
using Android.Provider;
using CustomFileOpenerAndSaver.Interfaces;
using CustomFileOpenerAndSaver.Models;


namespace CustomFileOpenerAndSaver.Platforms.Android.Services
{
    internal class FileOpenerService : IFileOpenerService
    {
        public static TaskCompletionSource<TransferFile> openFileTcs;


        public Task<TransferFile> OpenFile(TransferFile transferFile)
        {
            // Инициализация TaskCompletionSource
            openFileTcs = new TaskCompletionSource<TransferFile>();

            var intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("*/*"); // Выбор всех типов файлов

            MainActivity.Instance.StartActivityForResult(intent, 1001);

            return openFileTcs.Task;
        }

        public static void OnOpenFileActivityResult(Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok && data?.Data != null)
            {
                var uri = data.Data;
                try
                {
                    var contentResolver = MainActivity.Instance.ContentResolver;
                    using (var inputStream = contentResolver.OpenInputStream(uri))
                    {
                        if (inputStream != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                inputStream.CopyTo(memoryStream);
                                var fileContent = memoryStream.ToArray();

                                // Чтение файла и получение информации
                                var fileName = GetFileName(uri);
                                var fileExtension = Path.GetExtension(fileName).TrimStart('.');

                                if (fileName.EndsWith(".tdbkp"))
                                {
                                    fileName = fileName.Substring(0, fileName.Length - ".tdbkp".Length);
                                }

                                
                                var base64Content = Convert.ToBase64String(fileContent);

                                // Заполнение модели TransferFile
                                var transferFile = new TransferFile
                                {
                                    Name = fileName,
                                    Extension = fileExtension,
                                    Content = base64Content,
                                    Path = uri.ToString(), // Путь в формате URI
                                };

                                openFileTcs?.SetResult(transferFile);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // В случае ошибки заполняем Error
                    var errorTransferFile = new TransferFile
                    {
                        Error = new Error
                        {
                            Code = "FILE_OPEN_ERROR",
                            Message = ex.Message
                        }
                    };

                    openFileTcs?.SetResult(errorTransferFile);
                }
            }
            else
            {
                // Если пользователь отменил выбор файла
                var cancelledTransferFile = new TransferFile
                {
                    Error = new Error
                    {
                        Code = "CANCELLED",
                        Message = "File selection was cancelled by the user."
                    }
                };

                openFileTcs?.SetResult(cancelledTransferFile);
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
