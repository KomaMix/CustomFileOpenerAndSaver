using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Content;


namespace CustomFileOpenerAndSaver.Platforms.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", LaunchMode = LaunchMode.Multiple, Exported = true)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataMimeType = "application/octet-stream", DataScheme = "file", DataHost = "*", DataPathPattern = ".*\\.tdbkp")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataMimeType = "application/octet-stream", DataScheme = "content", DataHost = "*", DataPathPattern = ".*\\.tdbkp")]
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
                    // Получаем путь к файлу
                    string filePath = fileUri.Path;
                    Log.Debug("TdbkpFileActivity", $"Открыт файл: {filePath}");
                    // Можно получить данные файла
                }
            }

            // Закрываем активность после обработки файла
            Finish();
        }
    }
}
