using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FileMonitor.FileBackups
{
    /// <summary>
    /// Defines a class for copying monitored files to a backup location.
    /// </summary>
    public class Backup
    {
        private string backupFolder;

        /// <summary>
        /// Defines the <see cref="Backup"/> class constructor. A backup folder location is required.
        /// </summary>
        /// <param name="backupFolder"> User-specified location to place the copied files. </param>
        public Backup(string backupFolder)
        {
            this.backupFolder = backupFolder;
        }

        /// <summary>
        /// Copy all files monitored by the program to the designated backup location.
        /// </summary>
        /// <param name="files"> A collection of file paths to copy. </param>
        public void CopyAll(IEnumerable<string> files)
        {
            string backupFolderWithDateTime = $"{backupFolder}\\Backup{DateTime.Now.ToString("yyyy-MM-dd-HHmmss")}\\";
            foreach (string sourceFile in files)
            {
                string destination = GetFullDestinationPath(sourceFile, backupFolderWithDateTime);
                CreateDirectory(destination);
                File.Copy(sourceFile, destination, true);
            }
        }

        ///// <summary>
        ///// Copy files that have been changed or updated since the last backup.
        ///// </summary>
        ///// <param name="files"> A collection of file paths to copy. </param>
        //public void CopyUpdated(IEnumerable<string> files)
        //{
        //    string backupLocation = $"{backupFolder}\\Backup\\";
        //    foreach (string sourceFile in files)
        //    {
        //        string destination = GetFullDestinationPath(sourceFile, backupLocation);
        //        CreateDirectory(destination);
        //        destination = IncrementFileName(destination);
        //        File.Copy(sourceFile, destination, true);
        //    }
        //}

        // This method replaces the original path root with the backup destination root.
        private string GetFullDestinationPath(string? sourceFile, string backupFolderWithDateTime)
        {
            // Remove the root from the source path and replace it with the backup folder path.
            string oldRoot = Path.GetPathRoot(sourceFile);
            return sourceFile.Replace(oldRoot, backupFolderWithDateTime);
        }

        // Creates the directory path if it does not exist.
        private void CreateDirectory(string path)
        {
            string directory = Path.GetDirectoryName(path);
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            if (!directoryInfo.Exists) directoryInfo.Create();
        }

        // If the file already exists, append numbered text to the file name. Example: "someFile(Copy 3).txt"
        private string IncrementFileName(string path)
        {
            int count = 2;
            while(true)
            {
                if (File.Exists(path)) // The method returns the path when this line evaluates to false
                {
                    string extension = Path.GetExtension(path);
                    string fileNoExtension = Path.ChangeExtension(path, null);
                    if (FileIsNumbered(fileNoExtension, out string matchToStrip))
                    {
                        path = ReplaceStringMatch(fileNoExtension, matchToStrip, extension, count);
                    }
                    else path = $"{fileNoExtension}(Copy {count}){extension}";
                    count++;
                }
                else return path;
            }
        }

        // Search for a pattern match on the file name. If a match is found, return true. 
        // Use a ref out parameter to return the regex match.
        private bool FileIsNumbered(string fileName, out string matchToStrip)
        {
            string pattern = @"\(Copy\s(\d+)\)"; // Example match: "(Copy 12)"
            Regex regex = new Regex(pattern);
            Match match = regex.Match(fileName);
            if (match.Success)
            {
                matchToStrip = match.Value;
                return true;
            }
            else
            {
                matchToStrip = "";
                return false;
            }
        }

        // Pass in a file name with no extension. Remove the string match, then replace it with the updated count 
        // and the provided file extension.
        private string ReplaceStringMatch(string fileNoExtension, string matchToStrip, string extension, int count)
        {
            int charsToRemove = fileNoExtension.Length - matchToStrip.Length;
            fileNoExtension = fileNoExtension.Remove(charsToRemove);
            return $"{fileNoExtension}(Copy {count}){extension}";
        }
    }
}
