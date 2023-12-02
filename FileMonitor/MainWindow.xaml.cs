using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System;
using FileMonitor.Dialogs;
using FileMonitor.View;
using FileMonitor.Helpers;
using Services;
using Services.Dto;
using Services.Extensions;
using Services.Helpers;

namespace FileMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private MainWindowHelper _helper;

        /// <summary>
        /// Defines the <see cref="MainWindow"/> class constructor. Uses <see cref="SourceFileService"/> and <see cref=
        /// "BackupPathService"/> to initialize the data bindings in the view model. ALso sets the data context for the
        /// UI.
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
            using IgnorableFolderService ignorableFolderService = new IgnorableFolderService(
                RepositoryHelper.CreateIgnorableFolderRepositoryInstance());

            _viewModel = new MainWindowViewModel(
                new FileExplorerTreeView(backupPathService.GetDirectories()),
                new ObservableCollection<SourceFileDto>(sourceFileService.GetFiles()),
                new ObservableCollection<SourceFileDto>(sourceFileService.GetModifiedFiles()),
                new ObservableCollection<SourceFolderDto>(sourceFolderService.GetFolders()),
                new ObservableCollection<SourceFileDto>(sourceFileService.GetMovedOrRenamedFiles()),
                new ObservableCollection<BackupPathDto>(backupPathService.GetMovedOrRenamedPaths()),
                new ObservableCollection<IgnorableFolderDto>(ignorableFolderService.Get()),
                JsonSettingsHelper.OverwriteUpdatedFiles,
                JsonSettingsHelper.IncludeAllSubFolders
            );

            _viewModel.RemovePossibleRenamedFiles();
            DataContext = _viewModel;
            _helper = new MainWindowHelper();
            _helper.RefreshMonitoredFolders(_viewModel);
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
                _helper.AddFiles(paths, fromSourceFolder: false, _viewModel);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"{ex}");
                return;
            }
        }


        // A button click event handler for adding a folder, which effectively adds all files in any subfolders to the
        // program. Newly added files are also added to the MainWindowViewModel.UpdatedFiles collection. This assumes
        // that the file must first be copied to a backup location.
        private void AddNewFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string directory = FolderDialogWindow.GetPath();
                if (directory.Equals("")) return;
                if (_helper.VerifyAddFolder(directory, _viewModel, out List<string> paths, out bool MonitorAllSubFolders))
                {
                    using SourceFolderService sourceFolderService = new SourceFolderService(
                        RepositoryHelper.CreateSourceFolderRepositoryInstance(),
                        RepositoryHelper.CreateFolderFileMappingInstance(),
                        RepositoryHelper.CreateSourceFileRepositoryInstance()
                    );
                    _helper.AddFiles(paths, fromSourceFolder: true, _viewModel);
                    // Directory must be added after adding "paths" to avoid an exception.
                    SourceFolderDto dto = sourceFolderService.Add(directory, paths, MonitorAllSubFolders);
                    _viewModel.SourceFolders.Add(dto);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}");
                return;
            }
        }


        // A button click event handler to remove a file or files from the collection of monitored files. Deleted
        // files are also removed from the MainWindowViewModel.UpdatedFiles collection.
        private void RemoveFiles_Click(object sender, RoutedEventArgs e)
        {
            if (_helper.ConfirmRemoveFiles())
            {
                using SourceFileService sourceFileService = new SourceFileService(
                    RepositoryHelper.CreateSourceFileRepositoryInstance());
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
            foreach(BackupPathDto dto in _viewModel.BackupPaths.FullPaths)
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
            foreach (BackupPathDto dto in _viewModel.BackupPaths.FullPaths)
            {
                if (dto.IsSelected)
                {
                    Backup backup = new Backup(dto.Path);
                    backup.CopyUpdated(_viewModel.UpdatedFiles.Select(f => f.Path));
                }
            }
            _helper.ResetUpdatedFiles(_viewModel);
            MessageBox.Show("Backup complete.");
        }

        // A button click event handler for adding a backup folder path. 
        private void AddBackupPath_Click(object sender, RoutedEventArgs e)
        {
            string backupPath = FolderDialogWindow.GetPath();
            using BackupPathService backupPathService = new BackupPathService(
                RepositoryHelper.CreateBackupPathRepositoryInstance());
            if (backupPath == "" || backupPathService.PathExists(backupPath)) return;
            BackupPathDto backupPathDto = backupPathService.Add(backupPath);
            _viewModel.BackupPaths.AddPath(backupPathDto);
        }

        // An event handler to be called when the BackupPathCheckBox is checked in the UI. This method updates the
        // IsSelected property in the database using a data transfer object. This ensures that the CheckBox remains
        // selected even after the program exits. 
        private void BackupPathCheckBox_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = (System.Windows.Controls.CheckBox)sender;
            BackupPathDto backupPathDto = (BackupPathDto)checkBox.DataContext;
            using BackupPathService backupPathService = new BackupPathService(
                RepositoryHelper.CreateBackupPathRepositoryInstance());
            backupPathService.Update(backupPathDto, updatePath: false, updateIsSelected: true);
            _viewModel.BackupSelected = _viewModel.IsAnyBackupSelected();
        }

        // A button click event handler to refresh all ListViews in the UI.
        private void RefreshView_Click(object sender, RoutedEventArgs e)
        {
            _helper.RefreshUpdatedFilesView(_viewModel);
            _helper.RefreshMonitoredFolders(_viewModel);
            _helper.RefreshMovedOrRenamedFiles(_viewModel);
        }


        // An asynchronous button click event handler to remove monitored folders from the program, along with any
        // files contained within them.
        private void RemoveFolders_Click(object sender, RoutedEventArgs e)
        {
            if (_helper.ConfirmRemoveFolders(this))
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
            }
        }

        private void RemovePossibleDeletedPaths_Click(object sender, RoutedEventArgs e)
        {
            if (_helper.ConfirmRemoveFiles())
            {
                using SourceFileService sourceFileService = new SourceFileService(
                    RepositoryHelper.CreateSourceFileRepositoryInstance());
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

        private void RemoveBackupPath_Click(object sender, RoutedEventArgs e)
        {
            using BackupPathService backupPathService = new BackupPathService(
                RepositoryHelper.CreateBackupPathRepositoryInstance());

            /*BackupPathDto dto = (BackupPathDto)BackupPathsDisplayed.SelectedItem;*/

            // todo: update class to remove path by node
            //var node = IPathNode

            //if(node != null)
            //{
            //    backupPathService.Remove(ids: new List<int> { node.PathId });
            //    _viewModel.BackupPaths.RemovePath(node);
            //}

            //List<int> ids = new List<int>();
            //List<BackupPathDto> selectedPaths = new List<BackupPathDto>();
            //foreach (object item in BackupPathsDisplayed.SelectedItems)
            //{
            //    BackupPathDto dto = (BackupPathDto)item;
            //    selectedPaths.Add(dto);
            //    ids.Add(dto.Id);
            //}
            //backupPathService.Remove(ids);
            //_viewModel.BackupPaths.FileTreeItems.RemoveRange<BackupPathDto>(selectedPaths);
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Clear all ListViews when the mouse is clicked anywhere in the main window.
            FilesDisplayed.SelectedItems.Clear();
            FoldersDisplayed.SelectedItems.Clear();
            UpdatedFilesDisplayed.SelectedItems.Clear();
            //BackupPathsDisplayed.SelectedItems.Clear();
            MovedOrRenamedBackupPathsDisplayed.SelectedItems.Clear();
            MovedOrRenamedFilesDisplayed.SelectedItems.Clear();
        }

        private void RemovePossibleDeletedBackupPaths_Click(object sender, RoutedEventArgs e)
        {
            using BackupPathService backupPathService = new BackupPathService(
                RepositoryHelper.CreateBackupPathRepositoryInstance());
            List<int> ids = new List<int>();
            List<BackupPathDto> selectedPaths = new List<BackupPathDto>();
            foreach (object item in MovedOrRenamedBackupPathsDisplayed.SelectedItems)
            {
                BackupPathDto dto = (BackupPathDto)item;
                selectedPaths.Add(dto);
                ids.Add(dto.Id);
            }
            backupPathService.Remove(ids);
            _viewModel.MovedOrRenamedBackupPaths.RemoveRange<BackupPathDto>(selectedPaths);
        }

        private void AddIgnorableFolder_Click(object sender, RoutedEventArgs e)
        {
            using IgnorableFolderService ignorableFolderService = new IgnorableFolderService(
                RepositoryHelper.CreateIgnorableFolderRepositoryInstance());

            string folderName = Microsoft.VisualBasic.Interaction.InputBox(
                "Add the name of a folder to be ignored by the program, such as .git or .vs binary locations.",
                "Add Ignorable Folder",
                "",
                -1,
                -1);
            IgnorableFolderDto dto = ignorableFolderService.Add(folderName);
            _viewModel.IgnorableFolders.Add(dto);
        }

        private void RemoveIgnorableFolder_Click(object sender, RoutedEventArgs e)
        {
            using IgnorableFolderService ignorableFolderService = new IgnorableFolderService(
                RepositoryHelper.CreateIgnorableFolderRepositoryInstance());

            List<int> ids = new List<int>();
            List<IgnorableFolderDto> ignorableFolders = new List<IgnorableFolderDto>();

            foreach (object item in IgnorableFoldersDisplayed.SelectedItems)
            {
                IgnorableFolderDto dto = (IgnorableFolderDto)item;
                ignorableFolders.Add(dto);
                ids.Add(dto.Id);
            }
            ignorableFolderService.Remove(ids);
            _viewModel.IgnorableFolders.RemoveRange<IgnorableFolderDto>(ignorableFolders);
        }
    }
}


