using System.Windows.Forms;

namespace FileMonitor.Dialogs
{
    internal static class FolderDialogWindow
    {
        /// <summary>
        /// Open a FolderBrowserDialog for the user to select a folder.
        /// </summary>
        /// <returns> The full path to the folder. </returns>
        public static string GetPath()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            return dialog.SelectedPath;
        }
    }
}
