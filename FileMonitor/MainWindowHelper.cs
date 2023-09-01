using Services.Dto;
using Services.Helpers;
using Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileMonitor.ViewModels;
using System.Windows;

namespace FileMonitor
{
    /// <summary>
    /// 
    /// </summary>
    public class MainWindowHelper
    {
        /// <summary>
        /// Add all file paths to the database and update the view model.
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="fromSourceFolder"></param>
        /// <param name="_viewModel"></param>
        public void AddFiles(List<string> paths, bool fromSourceFolder, MainWindowViewModel _viewModel)
        {
            using SourceFileService sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());
            foreach (string path in paths)
            {
                FileAttributes attributes = File.GetAttributes(path);
                // If the path is an empty string, if it exists in the database, or if it is a directory, then continue
                if (path == "" || sourceFileService.PathExists(path) || attributes.HasFlag(FileAttributes.Directory))
                    continue;
                SourceFileDto dto = sourceFileService.Add(path, fromSourceFolder);
                _viewModel.SourceFiles.Add(dto);
                _viewModel.UpdatedFiles.Add(dto);
            }
        }

        /// Verifies that the user wants to add an entire folder. Displays the number of files that will be added by
        // doing so.
        public bool VerifyAddFolder(string directory, 
            MainWindowViewModel _viewModel,
            out List<string> paths, 
            out bool MonitorAllSubFolders)
        {
            paths = GetPathsFromFolder(directory, _viewModel, out int numberOfDirectories, out bool MonitorAll);
            MonitorAllSubFolders = MonitorAll;
            int numberOfFiles = paths.Count;

            return MessageBox.Show(
                $@"
The program will monitor {numberOfFiles} file(s) from {numberOfDirectories} subfolders(s). Do you wish to continue?",
                "Confirm Add Folder",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }


        private List<string> GetPathsFromFolder(
            string directory, 
            MainWindowViewModel _viewModel,
            out int numberOfDirectories, 
            out bool MonitorAll)
        {
            if (JsonSettingsHelper.IncludeAllSubFolders)
            {
                if (MessageBox.Show(
                    "Do you wish to monitor all files from all subfolders?\n\nSelect yes to monitor all. Select no " +
                    "to only monitor the files within this folder.",
                    "Include All Subfolders?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                    ) == MessageBoxResult.Yes)
                {
                    // Filter results to exclude ignorable folders.
                    numberOfDirectories = Directory.GetFileSystemEntries(
                        directory, "*", SearchOption.AllDirectories)
                        .Where(fse => !_viewModel.IgnorableFolders.Contains(
                            new IgnorableFolderDto
                            {
                                Name = fse
                            })
                        ) 
                        .Count();
                    MonitorAll = true;

                    // Filter results to exclude files contained in ignorable folders
                    return Directory.GetFileSystemEntries(directory, "*", SearchOption.AllDirectories)
                        .Where(
                            // Determine if path contains an entry from "ignorable"
                            f => IgnorableFolderHelper.NotIgnorable(f, _viewModel.IgnorableFolders)
                        )
                        .ToList();
                }
            }
            numberOfDirectories = 1;
            MonitorAll = false;
            return Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly).ToList();
        }

        // Confirms that the user wants to remove the selected files, and informs the user that this cannot be undone.
        internal bool ConfirmRemoveFiles()
        {
            string text = "Do you wish to remove the selected file(s) from the program? This cannot be undone.";
            string caption = "Remove SourceFiles";

            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage image = MessageBoxImage.Warning;
            return MessageBox.Show(text, caption, button, image) == MessageBoxResult.Yes;
        }

        // The main goal here is to reset the collection of updated files, both in the UI and in the database. To do
        // so, the method retrieves a list of Ids for each file in the UpdatedFiles collection. Then it resets the
        // IsModified checkbox to be false. Doing so informs the program that the copied version of the files is
        // effectively the most up-to-date version. Finally, it updates the hash for each file to the current hash.
        // Doing so ensures that the next time hashes are compared, these files will be ignored because they represent
        // the latest version of the file.
        public void ResetUpdatedFiles(MainWindowViewModel _viewModel)
        {
            List<int> ids = new List<int>();
            foreach (SourceFileDto dto in _viewModel.UpdatedFiles) ids.Add(dto.Id);
            _viewModel.UpdatedFiles.Clear();
            using SourceFileService sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());
            sourceFileService.ResetIsModifiedFlag(ids);
            sourceFileService.UpdateHashesToCurrent(ids);
        }

        // Check for files that have changed since the last backup
        public void RefreshUpdatedFilesView(MainWindowViewModel _viewModel)
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
        public void RefreshMonitoredFolders(MainWindowViewModel _viewModel)
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
        public void RefreshMovedOrRenamedFiles(MainWindowViewModel _viewModel)
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

        // Confirms that the user wants to delete the selected folders, and informs the user that this cannot be
        // undone.
        public bool ConfirmRemoveFolders(MainWindow mw)
        {
            using SourceFolderService sourceFolderService = new SourceFolderService(
                RepositoryHelper.CreateSourceFolderRepositoryInstance(),
                RepositoryHelper.CreateFolderFileMappingInstance(),
                RepositoryHelper.CreateSourceFileRepositoryInstance()
            );
            List<SourceFolderDto> folders = new List<SourceFolderDto>();
            int numberOfFiles = 0;
            int numberOfFolders = mw.FoldersDisplayed.SelectedItems.Count;
            foreach (object item in mw.FoldersDisplayed.SelectedItems)
            {
                SourceFolderDto dto = (SourceFolderDto)item;
                folders.Add(dto);
                numberOfFiles += sourceFolderService.GetStoredFilesFromFolder(dto.Id).Count();
            }

            string text = $"Do you wish to delete the selected folder(s) from the program? This cannot be undone." +
                $"\nDoing so will delete {numberOfFiles} file(s) from {numberOfFolders} monitored folder(s)";
            string caption = "Delete SourceFolders";

            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage image = MessageBoxImage.Warning;
            return MessageBox.Show(text, caption, button, image) == MessageBoxResult.Yes;
        }
    }
}
