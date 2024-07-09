using Microsoft.Win32;

namespace FileMonitor.Dialogs
{
    internal static class FolderDialogWindow
    {
        /// <summary>
        /// Open a Folder Dialog for the user to select a folder.
        /// </summary>
        /// <returns> The full path to the folder. </returns>
        public static string GetPath()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.ShowDialog();
            return dialog.FolderName;
        }
    }
}
