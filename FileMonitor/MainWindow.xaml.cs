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
                new ObservableCollection<IPathDto>(sourceFileService.GetFiles()),
                new ObservableCollection<IPathDto>(sourceFileService.GetModifiedFiles()),
                new ObservableCollection<ISourceFolderDto>(sourceFolderService.GetFolders()),
                new ObservableCollection<IPathDto>(sourceFileService.GetMovedOrRenamedFiles()),
                new ObservableCollection<IBackupPathDto>(backupPathService.GetMovedOrRenamedPaths()),
                new ObservableCollection<IIgnorableFolderDto>(ignorableFolderService.Get()),
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
                var paths = FileDialogWindow.GetPath().ToList();
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
                var directory = FolderDialogWindow.GetPath();
                if (directory.Equals("")) return;
                if (_helper.VerifyAddFolder(directory, _viewModel, out List<string> paths, out bool MonitorAllSubFolders))
                {
                    using var sourceFolderService = new SourceFolderService(
                        RepositoryHelper.CreateSourceFolderRepositoryInstance(),
                        RepositoryHelper.CreateFolderFileMappingInstance(),
                        RepositoryHelper.CreateSourceFileRepositoryInstance()
                    );
                    _helper.AddFiles(paths, fromSourceFolder: true, _viewModel);
                    // Directory must be added after adding "paths" to avoid an exception.
                    var dto = sourceFolderService.Add(directory, paths, MonitorAllSubFolders);
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
                var selectedFiles = new List<IPathDto>();
                foreach (object item in FilesDisplayed.SelectedItems)
                {
                    var dto = (IPathDto)item;
                    selectedFiles.Add(dto);
                    ids.Add(dto.Id);
                }
                sourceFileService.Remove(ids);
                _viewModel.SourceFiles.RemoveRange(selectedFiles);
                _viewModel.UpdatedFiles.RemoveRange(selectedFiles);
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
            foreach(IBackupPathDto dto in _viewModel.BackupPaths.FullPaths)
            {
                if(dto.IsSelected)
                {
                    var backup = new Backup(dto.Path);
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
            foreach (IBackupPathDto dto in _viewModel.BackupPaths.FullPaths)
            {
                if (dto.IsSelected)
                {
                    var backup = new Backup(dto.Path);
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
            var backupPathDto = backupPathService.Add(backupPath);
            _viewModel.BackupPaths.AddPath(backupPathDto);
        }

        // An event handler to be called when the BackupPathCheckBox is checked in the UI. This method updates the
        // IsSelected property in the database using a data transfer object. This ensures that the CheckBox remains
        // selected even after the program exits. 
        private void BackupPathCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var selectedNode = (IPathNode)checkBox.DataContext;
            var dto = (IBackupPathDto)_viewModel.BackupPaths.FullPaths.Where(path => path.Id == selectedNode.PathId).FirstOrDefault();
            var isChecked = checkBox.IsChecked;

            using var backupPathService = new BackupPathService(
                RepositoryHelper.CreateBackupPathRepositoryInstance());
            backupPathService.Update(dto, isChecked: isChecked);
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

                var foldersToRemove = new List<ISourceFolderDto>();
                var filesToRemove = new List<IPathDto>();
                var folderIds = new List<int>();

                foreach (object item in FoldersDisplayed.SelectedItems)
                {
                    var dto = (ISourceFolderDto)item;
                    foldersToRemove.Add(dto);
                    folderIds.Add(dto.Id);
                    filesToRemove = sourceFolderService.GetStoredFilesFromFolder(dto.Id);
                    _viewModel.SourceFiles.RemoveRange(filesToRemove);
                    _viewModel.UpdatedFiles.RemoveRange(filesToRemove);
                }
                sourceFolderService.Remove(folderIds);
                _viewModel.SourceFolders.RemoveRange(foldersToRemove);
            }
        }

        private void RemovePossibleDeletedPaths_Click(object sender, RoutedEventArgs e)
        {
            if (_helper.ConfirmRemoveFiles())
            {
                using var sourceFileService = new SourceFileService(
                    RepositoryHelper.CreateSourceFileRepositoryInstance());
                var ids = new List<int>();
                var selectedFiles = new List<IPathDto>();

                foreach (object item in MovedOrRenamedFilesDisplayed.SelectedItems)
                {
                    var dto = (IPathDto)item;
                    selectedFiles.Add(dto);
                    ids.Add(dto.Id);
                }

                sourceFileService.Remove(ids);
                _viewModel.MovedOrRenamedFiles.RemoveRange(selectedFiles);
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

            // todo: refactor to only get paths from the FullPaths property inside
            // the tree view. Additionally, redo the tree view API to not expose the nodes

            //var nodes = _viewModel.BackupPaths.GetNodes(node => node.IsChecked == true);
            //_viewModel.BackupPaths.RemovePaths(nodes);

            //var ids = nodes.Select(node => node.PathId);
            //backupPathService.Remove(ids);
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
            var ids = new List<int>();
            var selectedPaths = new List<IBackupPathDto>();

            foreach (object item in MovedOrRenamedBackupPathsDisplayed.SelectedItems)
            {
                var dto = (IBackupPathDto)item;
                selectedPaths.Add(dto);
                ids.Add(dto.Id);
            }

            backupPathService.Remove(ids);
            _viewModel.MovedOrRenamedBackupPaths.RemoveRange(selectedPaths);
        }

        private void AddIgnorableFolder_Click(object sender, RoutedEventArgs e)
        {
            using var ignorableFolderService = new IgnorableFolderService(
                RepositoryHelper.CreateIgnorableFolderRepositoryInstance());

            var folderName = Microsoft.VisualBasic.Interaction.InputBox(
                "Add the name of a folder to be ignored by the program, such as .git or .vs binary locations.",
                "Add Ignorable Folder",
                "",
                -1,
                -1);

            var dto = ignorableFolderService.Add(folderName);
            _viewModel.IgnorableFolders.Add(dto);
        }

        private void RemoveIgnorableFolder_Click(object sender, RoutedEventArgs e)
        {
            using var ignorableFolderService = new IgnorableFolderService(
                RepositoryHelper.CreateIgnorableFolderRepositoryInstance());

            var ids = new List<int>();
            var ignorableFolders = new List<IIgnorableFolderDto>();

            foreach (object item in IgnorableFoldersDisplayed.SelectedItems)
            {
                var dto = (IgnorableFolderDto)item;
                ignorableFolders.Add(dto);
                ids.Add(dto.Id);
            }

            ignorableFolderService.Remove(ids);
            _viewModel.IgnorableFolders.RemoveRange<IIgnorableFolderDto>(ignorableFolders);
        }
    }
}


