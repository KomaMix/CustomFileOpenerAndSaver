using AndroidX.DocumentFile.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFileOpenerAndSaver.Platforms.Android.Services
{
    internal class AllFilesService
    {
        public List<string> GetFilesInSelectedDirectory()
        {
            var treeUriString = Preferences.Get("SelectedDirectoryUri", string.Empty);
            var filesList = new List<string>();

            if (!string.IsNullOrEmpty(treeUriString))
            {
                global::Android.Net.Uri treeUri = global::Android.Net.Uri.Parse(treeUriString);
                DocumentFile directory = DocumentFile.FromTreeUri(MainActivity.Instance, treeUri);

                if (directory.IsDirectory)
                {
                    foreach (var file in directory.ListFiles())
                    {
                        filesList.Add(file.Name);
                    }
                }
            }

            return filesList;
        }
    }
}
