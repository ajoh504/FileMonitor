﻿using System.Windows;
using FileMonitor.Models;
using FileMonitor.Database;
using System;
using System.IO;
using System.Collections.Generic;

namespace FileMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MonitoredFiles monitoredFiles;
        public static readonly string programDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\FileMonitor";
        public static readonly string databasePath = $"{programDir}\\FMDB.sqlite";

        public MainWindow()
        {
            if (!File.Exists(databasePath))
            {
                Directory.CreateDirectory(programDir);
                DatabaseBuilder database = new DatabaseBuilder();
                database.Build();
            }

            InitializeComponent();
            monitoredFiles = new MonitoredFiles();
            DataContext = monitoredFiles;
        }

        private void AddNewFile_Click(object sender, RoutedEventArgs e)
        {
            string newFile = FileDialogWindow.GetPath();

            if(newFile != "")
            {
                monitoredFiles.AddFile(newFile);
            }
        }

        private void AddNewFolder_Click(object sender, RoutedEventArgs e)
        {
            string newFolder = FolderDialogWindow.GetPath();
        }

        private void DeleteFiles_Click(object sender, RoutedEventArgs e)
        {
            List<string> filesToRemove = new List<string>();
            foreach(string file in FilesDisplayed.SelectedItems) filesToRemove.Add(file);

            string filesFormatted = "";
            foreach(string file in filesToRemove) { filesFormatted += $"{file}\n"; }
            string text = $"Do you wish to delete the following files from the program? This cannot be undone.\n\n{filesFormatted}";
            string caption = "Delete Files";

            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage image = MessageBoxImage.Warning;
            MessageBoxResult result;
            result = MessageBox.Show(text, caption, button, image);

            if(result == MessageBoxResult.Yes)
            {
                foreach (string file in filesToRemove) monitoredFiles.RemoveFile(file);
            }
        }
        

        private void FilesDisplayed_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DeleteFiles.IsEnabled = true;
        }

        private void PerformBackup_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = ChooseBackupLocation();

            if(result == MessageBoxResult.Yes)
            {
                string backupFolder = FolderDialogWindow.GetPath();
                if (backupFolder.Equals("")) return;
                Backup backup = new Backup(backupFolder);
                backup.Run(monitoredFiles.AllFilePaths);
            }
        }

        private void BackupRecentlyChangedFiles_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = ChooseBackupLocation();

            if (result == MessageBoxResult.Yes)
            {
                string backupFolder = FolderDialogWindow.GetPath();
                if (backupFolder.Equals("")) return;
                Backup backup = new Backup(backupFolder);
                backup.Run(monitoredFiles.RecentlyChangedFiles);
            }
        }

        private MessageBoxResult ChooseBackupLocation()
        {
            MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;
            MessageBoxImage image = MessageBoxImage.Information;

            string text = "You must choose a backup location. Do you wish to proceed?";
            string caption = "Choose Backup Location";
            return MessageBox.Show(text, caption, messageBoxButton, image);
        }
    }
}


