using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Provider;
using Android.Content;

namespace CustomFileOpenerAndSaver.Platforms.Android
{
    public static class CheckPermissionManager
    {
        public static bool CheckExternalStoragePermission()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                var result = global::Android.OS.Environment.IsExternalStorageManager;
                if (!result)
                {
                    var manage = Settings.ActionManageAppAllFilesAccessPermission;
                    Intent intent = new Intent(manage);
                    global::Android.Net.Uri uri = global::Android.Net.Uri.Parse("package:" + AppInfo.Current.PackageName);
                    intent.SetData(uri);
                    Platform.CurrentActivity.StartActivity(intent);
                }
                return result;
            }

            return true;
        }
    }
}
