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
    internal class FileSaverService : IFileSaverService
    {
        private static TaskCompletionSource<bool> saveFileTcs;

        private static byte[] fileBytesToSave;

        public Task SaveFileAsync(string fileName, byte[] data)
        {
            fileBytesToSave = data;
            saveFileTcs = new TaskCompletionSource<bool>();

            var intent = new Intent(Intent.ActionCreateDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("application/octet-stream");
            intent.PutExtra(Intent.ExtraTitle, fileName);

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
                            outputStream.Write(fileBytesToSave, 0, fileBytesToSave.Length);
                            outputStream.Flush();
                            saveFileTcs.SetResult(true); // Успешное сохранение
                        }
                        else
                        {
                            saveFileTcs.SetResult(false); // Ошибка открытия потока
                        }
                    }
                }
                catch (Exception ex)
                {
                    saveFileTcs.SetException(ex); // Ошибка при сохранении
                }
            }
            else
            {
                saveFileTcs.SetResult(false); // Сохранение отменено
            }
        }

    }

}
