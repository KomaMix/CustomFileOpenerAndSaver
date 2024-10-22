using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Content;
using AndroidX.DocumentFile.Provider;


namespace CustomFileOpenerAndSaver.Platforms.Android
{

    [Activity(Theme = "@style/Maui.SplashTheme", LaunchMode = LaunchMode.Multiple, Exported = true)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataMimeType = "application/octet-stream", DataScheme = "file", DataHost = "*", DataPathPattern = ".*\\..*\\\\.tdbkp")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataMimeType = "application/octet-stream", DataScheme = "content", DataHost = "*", DataPathPattern = ".*\\.tdbkp")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataMimeType = "application/octet-stream", DataScheme = "content", DataHost = "*", DataPathPattern = ".*\\..*\\.tdbkp")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataMimeType = "application/octet-stream", DataScheme = "content", DataHost = "*", DataPathPattern = ".*\\..*\\..*\\.tdbkp")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataMimeType = "application/octet-stream", DataScheme = "content", DataHost = "*", DataPathPattern = ".*\\..*\\..*\\..*\\.tdbkp")]

    // Класс для обработки файла с расширением .tdbkp, переданного из других приложений
    public class TdbkpFileActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Обработка файла через Intent
            HandleIntent(Intent);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }

        private void HandleIntent(Intent intent)
        {
            if (intent?.Action == Intent.ActionView)
            {
                var fileUri = intent.Data;

                if (fileUri != null)
                {
                    DocumentFile documentFile = DocumentFile.FromSingleUri(this, fileUri);
                    if (documentFile != null && documentFile.Name != null)
                    {
                        Log.Debug("TdbkpFileActivity", $"Имя файла: {documentFile.Name}");
                    }



                    // Логируем основные параметры URI
                    Log.Debug("TdbkpFileActivity", $"Получен URI: {fileUri}");
                    Log.Debug("TdbkpFileActivity", $"Путь URI: {fileUri.Path}");
                    Log.Debug("TdbkpFileActivity", $"Схема URI: {fileUri.Scheme}");
                    Log.Debug("TdbkpFileActivity", $"Авторитет URI (Authority): {fileUri.Authority}");
                    Log.Debug("TdbkpFileActivity", $"DataString: {intent.DataString}");

                    

                    // Получаем MIME-тип файла
                    string mimeType = ContentResolver.GetType(fileUri);
                    Log.Debug("TdbkpFileActivity", $"MIME-тип: {mimeType}");

                    // Получаем разрешения на доступ к URI
                    var flags = intent.Flags;
                    Log.Debug("TdbkpFileActivity", $"Флаги намерения: {flags}");

                    // Проверка прав доступа на чтение и запись
                    bool hasReadPermission = (flags & ActivityFlags.GrantReadUriPermission) != 0;
                    bool hasWritePermission = (flags & ActivityFlags.GrantWriteUriPermission) != 0;
                    Log.Debug("TdbkpFileActivity", $"Есть права на чтение URI: {hasReadPermission}");
                    Log.Debug("TdbkpFileActivity", $"Есть права на запись URI: {hasWritePermission}");

                    // Открываем файл через ContentResolver
                    try
                    {
                        var contentResolver = ContentResolver;
                        using (var inputStream = contentResolver.OpenInputStream(fileUri))
                        {
                            if (inputStream != null)
                            {
                                Log.Debug("TdbkpFileActivity", $"Файл успешно открыт через ContentResolver");

                                // Выводим данные файла, если нужно
                                using (var reader = new StreamReader(inputStream))
                                {
                                    string fileContent = reader.ReadToEnd();
                                    Log.Debug("TdbkpFileActivity", $"Содержимое файла: {fileContent.Substring(0, Math.Min(100, fileContent.Length))}..."); // Ограничим вывод первыми 100 символами
                                }
                            }
                            else
                            {
                                Log.Debug("TdbkpFileActivity", "Не удалось открыть файл через ContentResolver.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("TdbkpFileActivity", $"Ошибка при работе с файлом: {ex.Message}");
                    }
                }
                else
                {
                    Log.Debug("TdbkpFileActivity", "Intent не содержит URI.");
                }
            }

            // Закрываем активность после обработки файла
            Finish();
        }
    }
}
