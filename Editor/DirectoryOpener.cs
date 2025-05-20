using UnityEngine;
using System.Diagnostics;
using System.IO;

namespace PretiaEditor
{
    internal class DirectoryOpener
    {
        public static void OpenInFileExplorer(string path)
        {
            // Open the directory
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                path = path.Replace("/", "\\");
                path = Path.Combine(Directory.GetCurrentDirectory(), path);

                // If the folder doesn't exist yet then open the parent folder
                if (!Directory.Exists(path))
                    path = Path.GetDirectoryName(path);
                
                Process.Start("explorer.exe", path);
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                // Replace backslashes with forward slashes for compatibility
                path = path.Replace("\\", "/");
                Process.Start("open", path);
            }
        }
    }
}