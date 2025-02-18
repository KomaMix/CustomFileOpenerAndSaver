﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using CustomFileOpenerAndSaver.Platforms.Android;
using CustomFileOpenerAndSaver.Platforms.Android.Services;

namespace CustomFileOpenerAndSaver
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Instance = this;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1000)
            {
                FileSaverService.OnSaveFileActivityResult(resultCode, data);
            } else if (requestCode == 1001)
            {
                FileOpenerService.OnOpenFileActivityResult(resultCode, data);
            }

            if (requestCode == 1002 && resultCode == Result.Ok && data != null)
            {
                Android.Net.Uri treeUri = data.Data;

                // Сохранение разрешения для долгосрочного доступа
                ContentResolver.TakePersistableUriPermission(treeUri, ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);

                // Сохранение URI в Preferences для последующего использования
                Preferences.Set("SelectedDirectoryUri", treeUri.ToString());

                // Оповещение страницы об изменениях
                MessagingCenter.Send(this, "DirectorySelected", treeUri.ToString());
            }
        }

        public void RequestDirectoryAccess()
        {
            var intent = new Intent(Intent.ActionOpenDocumentTree);
            intent.AddFlags(ActivityFlags.GrantPersistableUriPermission | ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
            StartActivityForResult(intent, 1002);
        }
    }
}
