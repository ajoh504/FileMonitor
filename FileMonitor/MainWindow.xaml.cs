using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System;
using FileMonitor.Dialogs;
using FileMonitor.FileBackups;
using FileMonitor.ViewModels;
using Services;
using Services.Dto;
using Services.Extensions;
using Services.Helpers;
using System.Windows.Controls;

namespace FileMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        /// <summary>
        /// Defines the <see cref="MainWindow"/> class constructor. Uses <see cref="SourceFileService"/> and <see cref="BackupPathService"/> to initialize the data bindings in the view model. ALso sets the data context for the UI.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            using SourceFileService sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());
            using BackupPathService backupPathService = new BackupPathService(
                RepositoryHelper.CreateBackupPathRepositoryInstance());
            using SourceFolderService sourceFolderService = new SourceFolderService(
                RepositoryHelper.CreateSourceFolderRepositoryInstance(),
                RepositoryHelper.CreateFolderFileMappingInstance(),
                RepositoryHelper.CreateSourceFileRepositoryInstance());

            _viewModel = new MainWindowViewModel(
                new ObservableCollection<BackupPathDto>(backupPathService.GetDirectories()),
                new ObservableCollection<SourceFileDto>(sourceFileService.GetFiles()),
                new ObservableCollection<SourceFileDto>(sourceFileService.GetModifiedFiles()),
                new ObservableCollection<SourceFolderDto>(sourceFolderService.GetFolders()),
                new ObservableCollection<SourceFileDto>(sourceFileService.GetMovedOrRenamedFiles()),
                JsonSettingsHelper.OverwriteUpdatedFiles,
                JsonSettingsHelper.IncludeAllSubFolders
            );
            DataContext = _viewModel;
            RefreshMonitoredFolders();
        }


        // A button click event handler for adding a file to be monitored by the program. Newly added files are also added
        // to the MainWindowViewModel.UpdatedFiles collection. This assumes that the file must first be copied to a backup
        // location. 
        private void AddNewFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> paths = FileDialogWindow.GetPath().ToList();
                if (paths.Count == 0) return;
                AddFiles(paths, fromSourceFolder: false);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($@"Access to files denied. One of the following issues may have occurred:

1. The program attempted to access files that are in use
2. The program attempted to access files that are read-only
3. The program does not have administrative privileges (Try running as admin) 
4. A folder is being accessed as a file

NOTE: Using this program to access critical system files is not recommended. Doing so may result in unintended consequences. This program is intended for use with the user's personal files.

{ex}
");
                return;
            }
        }

        // This method adds all file paths to the database and updates the view models.
        private void AddFiles(List<string> paths, bool fromSourceFolder)
        {
            using SourceFileService sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());
            foreach (string path in paths)
            {
                FileAttributes attributes = File.GetAttributes(path);
                // If the path is an empty string, if it exists in the database, or if it is a directory, then continue
                if (path == "" || sourceFileService.PathExists(path) || attributes.HasFlag(FileAttributes.Directory)) continue;
                SourceFileDto dto = sourceFileService.Add(path, fromSourceFolder);
                _viewModel.SourceFiles.Add(dto);
                _viewModel.UpdatedFiles.Add(dto);
            }
        }


        // A button click event handler for adding a folder, which effectively adds all files in any subfolders to the program.
        // Newly added files are also added to the MainWindowViewModel.UpdatedFiles collection. This assumes that the file must
        // first be copied to a backup location.
        private void AddNewFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string directory = FolderDialogWindow.GetPath();
                if (directory.Equals("")) return;
                if (VerifyAddFolder(directory, out List<string> paths))
                {
                    using SourceFolderService sourceFolderService = new SourceFolderService(
                        RepositoryHelper.CreateSourceFolderRepositoryInstance(),
                        RepositoryHelper.CreateFolderFileMappingInstance(),
                        RepositoryHelper.CreateSourceFileRepositoryInstance()
                    );
                    AddFiles(paths, fromSourceFolder: true);
                    SourceFolderDto dto = sourceFolderService.Add(directory, paths); // Must be called after adding paths to avoid an exception.
                    _viewModel.SourceFolders.Add(dto);
                    MessageBox.Show("The selected folders and their files have been added. You may close this window.", "Task Complete", MessageBoxButton.OK);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($@"Access to files denied. One of the following issues may have occurred:

1. The program attempted to access files that are in use
2. The program attempted to access files that are read-only
3. The program does not have administrative privileges (Try running as admin) 
4. A folder is being accessed as a file

NOTE: Using this program to access critical system files is not recommended. Doing so may result in unintended consequences. This program is intended for use with the user's personal files.

{ex}
");
                return;
            }
        }

        // Verifies that the user wants to add an entire folder. Displays the number of files that will be added by doing so.
        private bool VerifyAddFolder(string directory, out List<string> paths)
        {
            paths = GetPathsFromFolder(directory, out int numberOfDirectories);
            int numberOfFiles = paths.Count;

            return MessageBox.Show(
                $@"The program will monitor {numberOfFiles} file(s) from {numberOfDirectories} subfolders(s). Do you wish to continue?", 
                "Confirm Add Folder", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        private List<string> GetPathsFromFolder(string directory, out int numberOfDirectories)
        {
            if (JsonSettingsHelper.IncludeAllSubFolders)
            {
                if (MessageBox.Show(
                    "Do you wish to monitor all files from all subfolders?\n\nSelect yes to to monitor all. Select no to only monitor the files within this folder.",
                    "Include All Subfolders?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                    ) == MessageBoxResult.Yes)
                {
                    numberOfDirectories = Directory.GetFileSystemEntries(directory, "*", SearchOption.AllDirectories).Length;
                    return Directory.GetFileSystemEntries(directory, "*", SearchOption.AllDirectories).ToList();
                }
            }
            numberOfDirectories = 1;
            return Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly).ToList();
        }

        // A button click event handler to remove a file or files from the collection of monitored files. Deleted
        // files are also removed from the MainWindowViewModel.UpdatedFiles collection.
        private void RemoveFiles_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmRemoveFiles())
            {
                using SourceFileService sourceFileService = new SourceFileService(RepositoryHelper.CreateSourceFileRepositoryInstance());
                List<int> ids = new List<int>();
                List<SourceFileDto> selectedFiles = new List<SourceFileDto>();
                foreach (object item in FilesDisplayed.SelectedItems)
                {
                    SourceFileDto dto = (SourceFileDto)item;
                    selectedFiles.Add(dto);
                    ids.Add(dto.Id);
                }
                sourceFileService.Remove(ids);
                _viewModel.SourceFiles.RemoveRange<SourceFileDto>(selectedFiles);
                _viewModel.UpdatedFiles.RemoveRange<SourceFileDto>(selectedFiles);
            }
        }

        // Confirms that the user wants to remove the selected files, and informs the user that this cannot be undone.
        private bool ConfirmRemoveFiles()
        {
            string text = "Do you wish to remove the selected file(s) from the program? This cannot be undone.";
            string caption = "Remove SourceFiles";

            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage image = MessageBoxImage.Warning;
            return MessageBox.Show(text, caption, button, image) == MessageBoxResult.Yes;
        }

        // A button click event handler to create a full backup of all files monitored by the program. The full backup
        // copies every file to the backup location, and stores them in a unique directory. The name of the directory
        // uses the current date and time.
        private void CopyAllFiles_Click(object sender, RoutedEventArgs e)
        {
            if(!_viewModel.BackupSelected)
            {
                MessageBox.Show("Please add a backup path.");
                return;
            }
            foreach(BackupPathDto dto in _viewModel.BackupPaths)
            {
                if(dto.IsSelected)
                {
                    Backup backup = new Backup(dto.Path);
                    backup.CopyAll(_viewModel.SourceFiles.Select(f => f.Path));
                }
            }
            MessageBox.Show("Backup complete.");
        }

        // A button click event handler to copy only the files that have been updated or changed since the last backup.
        private void CopyUpdatedFiles_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.BackupSelected)
            {
                MessageBox.Show("Please add a backup path.");
                return;
            }
            foreach (BackupPathDto dto in _viewModel.BackupPaths)
            {
                if (dto.IsSelected)
                {
                    Backup backup = new Backup(dto.Path);
                    backup.CopyUpdated(_viewModel.UpdatedFiles.Select(f => f.Path));
                }
            }
            ResetUpdatedFiles();
            MessageBox.Show("Backup complete.");
        }

        // The main goal here is to reset the collection of updated files, both in the UI and in the database. To do so, the 
        // method retrieves a list of Ids for each file in the UpdatedFiles collection. Then it resets the IsModified checkbox 
        // to be false. Doing so informs the program that the copied version of the files is effectively the most up-to-date
        // version. Finally, it updates the hash for each file to the current hash. Doing so ensures that the next time hashes
        // are compared, these files will be ignored because they represent the latest version of the file.
        private void ResetUpdatedFiles()
        {
            List<int> ids = new List<int>();
            foreach (SourceFileDto dto in _viewModel.UpdatedFiles) ids.Add(dto.Id);
            _viewModel.UpdatedFiles.Clear();
            using SourceFileService sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());
            sourceFileService.ResetIsModifiedFlag(ids);
            sourceFileService.UpdateHashesToCurrent(ids);
        }

        // A button click event handler for adding a backup folder path. 
        private void AddBackupPath_Click(object sender, RoutedEventArgs e)
        {
            string backupPath = FolderDialogWindow.GetPath();
            using BackupPathService backupPathService = new BackupPathService(
                RepositoryHelper.CreateBackupPathRepositoryInstance());
            if (backupPath == "" || backupPathService.PathExists(backupPath)) return;
            BackupPathDto backupPathDto = backupPathService.Add(backupPath);
            _viewModel.BackupPaths.Add(backupPathDto);
        }

        // An event handler to be called when the BackupPathCheckBox is checked in the UI. This method updates the IsSelected property
        // in the database using a data transfer object. This ensures that the CheckBox remains selected even after the program exits. 
        private void BackupPathCheckBox_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = (System.Windows.Controls.CheckBox)sender;
            BackupPathDto backupPathDto = (BackupPathDto)checkBox.DataContext;
            using BackupPathService backupPathService = new BackupPathService(RepositoryHelper.CreateBackupPathRepositoryInstance());
            backupPathService.Update(backupPathDto, updatePath: false, updateIsSelected: true);
            _viewModel.BackupSelected = _viewModel.IsAnyBackupSelected();
        }

        // A button click event handler to refresh all ListViews in the UI.
        private void RefreshView_Click(object sender, RoutedEventArgs e)
        {
            RefreshUpdatedFilesView();
            RefreshMonitoredFolders();
            RefreshMovedOrRenamedFiles();
        }

        // Check for files that have changed since the last backup
        private void RefreshUpdatedFilesView()
        {
            using SourceFileService sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());
            
            List<SourceFileDto> sourceFileDtos = sourceFileService.GetModifiedFiles();
            foreach (SourceFileDto sourceFileDto in sourceFileDtos)
            {
                if (!_viewModel.UpdatedFiles.Contains(sourceFileDto))
                    _viewModel.UpdatedFiles.Add(sourceFileDto);
            }
        }

        // Check for folders that have newly added files since the last backup, and add them to the database
        // Also retrieves files that were removed from a monitored folder
        private void RefreshMonitoredFolders()
        {
            using SourceFolderService sourceFolderService = new SourceFolderService(
                RepositoryHelper.CreateSourceFolderRepositoryInstance(),
                RepositoryHelper.CreateFolderFileMappingInstance(),
                RepositoryHelper.CreateSourceFileRepositoryInstance()
            );

            if (sourceFolderService.FilesAddedToFolders(
                out List<SourceFileDto>? newFilesFromFolder))
            {
                foreach (SourceFileDto file in newFilesFromFolder)
                {
                    if (!_viewModel.SourceFiles.Contains(file))
                        _viewModel.SourceFiles.Add(file);
                    if (!_viewModel.UpdatedFiles.Contains(file))
                        _viewModel.UpdatedFiles.Add(file);
                }
            }
        }

        // Check for files that have been moved, deleted, or renamed 
        private void RefreshMovedOrRenamedFiles()
        {
            using SourceFileService sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());
            List<SourceFileDto> files = sourceFileService.GetMovedOrRenamedFiles();
            foreach (SourceFileDto file in files)
            {
                if (!_viewModel.MovedOrRenamedFiles.Contains(file))
                    _viewModel.MovedOrRenamedFiles.Add(file);
                if (_viewModel.UpdatedFiles.Contains(file))
                    _viewModel.UpdatedFiles.Remove(file);
                if (_viewModel.SourceFiles.Contains(file))
                    _viewModel.SourceFiles.Remove(file);
            }
        }

        // An asynchronous button click event handler to remove monitored folders from the program, along with any files contained within them.
        private void RemoveFolders_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmRemoveFolders())
            {
                using SourceFileService sourceFileService = new SourceFileService(
                    RepositoryHelper.CreateSourceFileRepositoryInstance());
                using SourceFolderService sourceFolderService = new SourceFolderService(
                    RepositoryHelper.CreateSourceFolderRepositoryInstance(),
                    RepositoryHelper.CreateFolderFileMappingInstance(),
                    RepositoryHelper.CreateSourceFileRepositoryInstance()
                );
                List<SourceFolderDto> foldersToRemove = new List<SourceFolderDto>();
                List<SourceFileDto> filesToRemove = new List<SourceFileDto>();
                List<int> folderIds = new List<int>();
                foreach (object item in FoldersDisplayed.SelectedItems)
                {
                    SourceFolderDto dto = (SourceFolderDto)item;
                    foldersToRemove.Add(dto);
                    folderIds.Add(dto.Id);
                    filesToRemove = sourceFolderService.GetStoredFilesFromFolder(dto.Id);
                    _viewModel.SourceFiles.RemoveRange<SourceFileDto>(filesToRemove);
                    _viewModel.UpdatedFiles.RemoveRange<SourceFileDto>(filesToRemove);
                }
                sourceFolderService.Remove(folderIds);
                _viewModel.SourceFolders.RemoveRange<SourceFolderDto>(foldersToRemove);
                MessageBox.Show("The selected folders and their files have been removed. You may close this window.", "Task Complete", MessageBoxButton.OK);
            }
        }

        // Confirms that the user wants to delete the selected folders, and informs the user that this cannot be undone.
        private bool ConfirmRemoveFolders()
        {
            using SourceFolderService sourceFolderService = new SourceFolderService(
                RepositoryHelper.CreateSourceFolderRepositoryInstance(),
                RepositoryHelper.CreateFolderFileMappingInstance(),
                RepositoryHelper.CreateSourceFileRepositoryInstance()
            );
            List<SourceFolderDto> folders = new List<SourceFolderDto>();
            int numberOfFiles = 0;
            int numberOfFolders = FoldersDisplayed.SelectedItems.Count;
            foreach (object item in FoldersDisplayed.SelectedItems)
            {
                SourceFolderDto dto = (SourceFolderDto)item;
                folders.Add(dto);
                numberOfFiles += sourceFolderService.GetStoredFilesFromFolder(dto.Id).Count();
            }
            
            string text = $"Do you wish to delete the selected folder(s) from the program? This cannot be undone.\nDoing so will delete {numberOfFiles} file(s) from {numberOfFolders} monitored folder(s)";
            string caption = "Delete SourceFolders";

            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage image = MessageBoxImage.Warning;
            return MessageBox.Show(text, caption, button, image) == MessageBoxResult.Yes;
        }

        private void RemovePossibleDeletedPaths_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmRemoveFiles())
            {
                using SourceFileService sourceFileService = new SourceFileService(RepositoryHelper.CreateSourceFileRepositoryInstance());
                List<int> ids = new List<int>();
                List<SourceFileDto> selectedFiles = new List<SourceFileDto>();
                foreach (object item in MovedOrRenamedFilesDisplayed.SelectedItems)
                {
                    SourceFileDto dto = (SourceFileDto)item;
                    selectedFiles.Add(dto);
                    ids.Add(dto.Id);
                }
                sourceFileService.Remove(ids);
                _viewModel.MovedOrRenamedFiles.RemoveRange<SourceFileDto>(selectedFiles);
            }
        }

        private void OverwriteUpdatedFilesCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            JsonSettingsHelper.OverwriteUpdatedFiles = (bool)checkBox.IsChecked;
        }

        private void IncludeAllSubfoldersCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            JsonSettingsHelper.IncludeAllSubFolders = (bool)checkBox.IsChecked;
        }
    }
}


