using Android.App;
using Android.Content;
using CustomFileOpenerAndSaver.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFileOpenerAndSaver.Platforms.Android.Services
{
    internal class FileOpenerService : IFileOpenerService
    {
        public static TaskCompletionSource<bool> openFileTcs;


        public Task OpenFile()
        {
            openFileTcs = new TaskCompletionSource<bool>();

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
                    // Получение контента файла
                    var contentResolver = MainActivity.Instance.ContentResolver;
                    using (var inputStream = contentResolver.OpenInputStream(uri))
                    {
                        if (inputStream != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                inputStream.CopyTo(memoryStream);
                                var fileContent = memoryStream.ToArray();

                                // Теперь fileContent содержит содержимое файла
                                // Вы можете сохранить его или использовать как требуется
                                // Например, вывести содержимое файла как строку
                                string fileText = Encoding.UTF8.GetString(fileContent);
                                Console.WriteLine(fileText);
                            }
                        }
                    }

                    openFileTcs?.SetResult(true);
                }
                catch (Exception ex)
                {
                    openFileTcs.SetException(ex); // Ошибка при сохранении
                }
            }
            else
            {
                openFileTcs.SetResult(false); // Сохранение отменено
            }
        }
    }
}
