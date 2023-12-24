using Services.Dto;
using Services.Helpers;
using Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using FileMonitor.View;
using FileMonitor.Helpers;

namespace FileMonitor
{
    /// <summary>
    /// A helper class for the <see cref="MainWindow"/>.
    /// </summary>
    internal class MainWindowHelper
    {
        /// <summary>
        /// Receive a list of file paths, add them to the database, and update the view model.
        /// </summary>
        /// <param name="paths"> File paths to add. </param>
        /// <param name="fromSourceFolder"> True if the file path is from a source folder, false otherwise. </param>
        /// <param name="_viewModel"> An instance of the view model to update. </param>
        internal void AddFiles(List<string> paths, bool fromSourceFolder, MainWindowViewModel _viewModel)
        {
            using var sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());

            foreach (var path in paths)
            {
                var attributes = File.GetAttributes(path);
                // If the path is an empty string, if it exists in the database, or if it is a directory, then continue
                if (path == "" || sourceFileService.PathExists(path) || attributes.HasFlag(FileAttributes.Directory))
                    continue;
                var dto = sourceFileService.Add(path, fromSourceFolder);
                _viewModel.SourceFiles.Add(dto);
                _viewModel.UpdatedFiles.Add(dto);
            }
        }

        /// <summary>
        /// Verify that the user wants to add an entire folder, then display a Window with the number of files that 
        /// will be added by doing so.
        /// </summary>
        /// <param name="directory"> The folder to add as a source folder. </param>
        /// <param name="_viewModel"> An instance of the view model to update. </param>
        /// <param name="paths"> The list of paths to be added as source files from the given source folder.  </param>
        /// <param name="MonitorAllSubFolders"></param>
        /// <returns></returns>
        // 
        internal bool VerifyAddFolder(string directory, 
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

        /// <summary>
        /// Confirm that the user wants to remove the selected files, and inform the user that this cannot be undone.
        /// </summary>
        internal bool ConfirmRemoveFiles()
        {
            var text = "Do you wish to remove the selected file(s) from the program? This cannot be undone.";
            var caption = "Remove SourceFiles";
            var button = MessageBoxButton.YesNo;
            var image = MessageBoxImage.Warning;
            return MessageBox.Show(text, caption, button, image) == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Reset the collection of updated files, both in the UI and in the database.
        /// </summary>
        /// <param name="_viewModel"> An instance of the view model to update. </param>
        /// <remarks>
        /// Retrieve a list of Ids for each file in the UpdatedFiles collection. Then reset the IsModified checkbox 
        /// to false. Doing so informs the program that the copied version of the file is effectively the most 
        /// up-to-date version. Finally, update the hash for each file to the current hash. Doing so ensures that 
        /// the next time hashes are compared, these files will be ignored because they represent the latest version
        /// of the file.
        /// </remarks>
        internal void ResetUpdatedFiles(MainWindowViewModel _viewModel)
        {
            var ids = new List<int>();
            foreach (var dto in _viewModel.UpdatedFiles) ids.Add(dto.Id);
            _viewModel.UpdatedFiles.Clear();
            using var sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());
            sourceFileService.ResetIsModifiedFlag(ids);
            sourceFileService.UpdateHashesToCurrent(ids);
        }

        /// <summary>
        /// Check for files that have changed since the last backup.
        /// </summary>
        /// <param name="_viewModel"> An instance of the view model to update. </param>
        internal void RefreshUpdatedFilesView(MainWindowViewModel _viewModel)
        {
            using var sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());

            var sourceFileDtos = sourceFileService.GetModifiedFiles();
            foreach (var sourceFileDto in sourceFileDtos)
            {
                if (!_viewModel.UpdatedFiles.Contains(sourceFileDto))
                    _viewModel.UpdatedFiles.Add(sourceFileDto);
            }
        }

        /// <summary>
        /// Check for source folders that have newly added files since the last backup, and add them to the database.
        /// </summary>
        /// <param name="_viewModel"> An instance of the view model to update. </param>
        internal void RefreshMonitoredFolders(MainWindowViewModel _viewModel)
        {
            using SourceFolderService sourceFolderService = new SourceFolderService(
                RepositoryHelper.CreateSourceFolderRepositoryInstance(),
                RepositoryHelper.CreateFolderFileMappingInstance(),
                RepositoryHelper.CreateSourceFileRepositoryInstance()
            );

            if (sourceFolderService.FilesAddedToFolders(
                out List<IPathDto>? newFilesFromFolder))
            {
                foreach (var file in newFilesFromFolder)
                {
                    if (!_viewModel.SourceFiles.Contains(file))
                        _viewModel.SourceFiles.Add(file);
                    if (!_viewModel.UpdatedFiles.Contains(file))
                        _viewModel.UpdatedFiles.Add(file);
                }
            }
        }

        /// <summary>
        /// Check for files that have been moved, deleted, or renamed.
        /// </summary>
        /// <param name="_viewModel"> An instance of the view model to update. </param>
        internal void RefreshMovedOrRenamedFiles(MainWindowViewModel _viewModel)
        {
            using var sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());
            var files = sourceFileService.GetMovedOrRenamedFiles();
            foreach (var file in files)
            {
                if (!_viewModel.MovedOrRenamedFiles.Contains(file))
                    _viewModel.MovedOrRenamedFiles.Add(file);
                if (_viewModel.UpdatedFiles.Contains(file))
                    _viewModel.UpdatedFiles.Remove(file);
                if (_viewModel.SourceFiles.Contains(file))
                    _viewModel.SourceFiles.Remove(file);
            }
        }

        /// <summary>
        /// Confirms that the user wants to delete the selected folders, and informs the user that this cannot be
        /// undone.
        /// </summary>
        internal bool ConfirmRemoveFolders(MainWindow mw)
        {
            using SourceFolderService sourceFolderService = new SourceFolderService(
                RepositoryHelper.CreateSourceFolderRepositoryInstance(),
                RepositoryHelper.CreateFolderFileMappingInstance(),
                RepositoryHelper.CreateSourceFileRepositoryInstance()
            );
            var folders = new List<IPathDto>();
            int numberOfFiles = 0;
            int numberOfFolders = mw.FoldersDisplayed.SelectedItems.Count;
            foreach (object item in mw.FoldersDisplayed.SelectedItems)
            {
                var dto = (IPathDto)item;
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
