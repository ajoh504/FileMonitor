﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FileMonitor.Models
{
    class MonitoredFiles
    {
        private List<string>? allFiles;
        private List<string>? filesChangedSinceBackup;
        public event EventHandler<FilesChangedEventArgs> FilesChangedEventHandler;

        /// <summary>
        /// Public property to expose the private field
        /// Call OnPropertyChanged() whenever the files have changed.
        /// </summary>
        public List<string> Files
        {
            get { return allFiles; }
            set
            {
                if (value != allFiles)
                {
                    List<string> oldFiles = allFiles;
                    OnFilesChanged(new FilesChangedEventArgs(oldFiles, value));
                    allFiles = value;
                }
                else return;
            }
        }

        protected virtual void OnFilesChanged(FilesChangedEventArgs e)
        {
            FilesChangedEventHandler?.Invoke(this, e);
        }

        /// <summary>
        /// Display all currently stored file paths
        /// </summary>
        /// <param name="mw"> MainWindow object. Used to update the XAML TextBlocks </param>
        public void ShowAll(MainWindow mw)
        {
            List<string> files = JsonFile.GetDeserializedList();
            string result = "";
            for (int i = 0; i < files.Count; i++)
            {
                result += files[i] + "\n";
            }
            mw.FilesDisplayed.Text = result;
        }

        /// <summary>
        /// Display file paths only if the files have changed since the last backup
        /// </summary>
        public void ShowChangedSinceBackup(MainWindow mw)
        {
            mw.RecentlyChangedFiles.Text = JsonFile.GetDeserializedList().ToString();
        }
    }
}
