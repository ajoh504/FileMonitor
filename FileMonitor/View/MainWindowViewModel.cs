using Services.Dto;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FileMonitor.View
{
    /// <summary>
    /// Provides a view model for binding to the UI. 
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private FileExplorerTreeView _backupPaths;
        private ObservableCollection<IPathDto> _sourceFiles;
        private ObservableCollection<IPathDto> _updatedFiles;
        private ObservableCollection<ISourceFolderDto> _sourceFolders;
        private ObservableCollection<IPathDto> _movedOrRenamedFiles;
        private ObservableCollection<IBackupPathDto> _movedOrRenamedBackupPaths;
        private ObservableCollection<IIgnorableFolderDto> _ignorableFolders;
        private bool _backupSelected;

        /// <summary>
        /// A public event handler to notify any data bindings when a property has changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Determines whether or not a backup path is selected in the UI. If a path is selected, the "Copy All" button
        /// is clickable. If not, the button is greyed out. This property is bound to the <see cref=
        /// "MainWindow.CopyAllFiles"/> button in the UI.
        /// </summary>
        public bool BackupSelected
        {
            get { return _backupSelected; }
            set
            {
                _backupSelected = value;
                OnPropertyChanged(nameof(BackupSelected));
            }
        }

        ///// <summary>
        ///// An observable collection of <see cref="BackupPathDto"/> objects. This collection displays all possible
        ///// backup path locations for the user to select, add, or remove. This property is bound to the <see cref=
        ///// "MainWindow.BackupPathsDisplayed"/> list view in the UI.
        ///// </summary>
        //public ObservableCollection<BackupPathDto> BackupPaths { get { return _backupPaths; } }

        /// <summary>
        /// A tree view for interacting with all backup paths. This property is bound to the <see cref=
        /// "MainWindow.BackupPathsDisplayed"/>
        /// </summary>
        public FileExplorerTreeView BackupPaths { get { return _backupPaths; } }

        /// <summary>
        /// An observable collection of <see cref="SourceFileDto"/> objects. This collection displays all files
        /// monitored by the program for the user to add or remove. This property is bound to the <see cref=
        /// "MainWindow.FilesDisplayed"/> list view in the UI.
        /// </summary>
        public ObservableCollection<IPathDto> SourceFiles { get { return _sourceFiles; } }

        /// <summary>
        /// An observable collection of <see cref="SourceFileDto"/> objects. This collection displays only the files
        /// that have been updated since the last time they were copied to a backup location. This property is bound to
        /// the <see cref="MainWindow.UpdatedFilesDisplayed"/> list view.
        /// </summary>
        public ObservableCollection<IPathDto> UpdatedFiles { get { return _updatedFiles; } }

        /// <summary>
        /// An observable collection of <see cref="SourceFolderDto"/> objects. This collection displays all folders
        /// monitored by the program. This property is bound to the <see cref="MainWindow.FoldersDisplayed"/> list 
        /// view.
        /// </summary>
        public ObservableCollection<ISourceFolderDto> SourceFolders { get { return _sourceFolders; } }

        /// <summary>
        /// An observable collection of <see cref="SourceFileDto"/> objects. This collection displays all files whose
        /// names or paths have been moved, renamed, or deleted since being monitored by the program. This property is
        /// bound to the <see cref="MainWindow.MovedOrRenamedFilesDisplayed"/> list view.
        /// </summary>
        public ObservableCollection<IPathDto> MovedOrRenamedFiles { get { return _movedOrRenamedFiles; } }

        /// <summary>
        /// An observable collection of <see cref="BackupPathDto"/> objects. This collection displays all backup paths
        /// whose names or paths have been moved, renamed, or deleted since being monitored by the program. This
        /// property is bound to the <see cref="MainWindow.MovedOrRenamedBackupPathsDisplayed"/> list view.
        /// </summary>
        public ObservableCollection<IBackupPathDto> MovedOrRenamedBackupPaths { get { return _movedOrRenamedBackupPaths; } }

        /// <summary>
        /// An observable collection of <see cref="IgnorableFolderDto"/> objects. This collection displays all backup
        /// paths whose names or paths have been moved, renamed, or deleted since being monitored by the program. This
        /// property is bound to the <see cref="MainWindow.IgnorableFoldersDisplayed"/> list view.
        /// </summary>
        public ObservableCollection<IIgnorableFolderDto> IgnorableFolders { get { return _ignorableFolders;  } }

        /// <summary>
        /// If false, the program will make copies of updated files whenever they are backed up. If true, the program
        /// will overwrite the previously copied file.
        /// </summary>
        public bool OverwriteUpdatedFiles { get; set; }

        /// <summary>
        /// If false, the program will only monitor files contained within the monitored folder. If true, the program
        /// will monitor all files an sub-files contained within the monitored folder. 
        /// </summary>
        public bool IncludeAllSubfolders { get; set; }

        /// <summary>
        /// Defines the <see cref="MainWindowViewModel"/> class constructor.
        /// </summary>
        /// <param name="backupPaths">
        /// All backup paths stored in the database, formatted as data transfer objects.
        /// </param>
        /// <param name="sourceFiles">
        /// All files monitored by the program, formatted as data transfer objects.
        /// </param>
        /// <param name="updatedFiles">
        /// Only the files that have been updated since the last time they were copied to a backup location, formatted
        /// as data transfer objects.
        /// </param>
        /// <param name="sourceFolders">
        /// All folders monitored by the program, formatted as data transfer objects.
        /// </param>
        /// <param name="movedOrRenamedFiles">
        /// All files that have been moved, renamed, or deleted in Windows since being added to the program.
        /// </param>
        /// <param name="movedOrRenamedBackupPaths">
        /// All backup paths that have been moved, renamed, or deleted in Windows since being added to the program.
        /// </param>
        /// <param name="ignorableFolders"> All folders that are ignored by the program. </param>
        /// <param name="overwriteUpdatedFiles"> Value from Settings.json, bound to a CheckBox. </param>
        /// <param name="includeAllSubfolders"> Value from Settings.json, bound to a CheckBox. </param>
        public MainWindowViewModel(
            FileExplorerTreeView backupPaths, 
            ObservableCollection<IPathDto> sourceFiles,
            ObservableCollection<IPathDto> updatedFiles,
            ObservableCollection<ISourceFolderDto> sourceFolders,
            ObservableCollection<IPathDto> movedOrRenamedFiles,
            ObservableCollection<IBackupPathDto> movedOrRenamedBackupPaths,
            ObservableCollection<IIgnorableFolderDto> ignorableFolders,
            bool overwriteUpdatedFiles,
            bool includeAllSubfolders)
        {
            _backupPaths = backupPaths;
            _sourceFiles = sourceFiles;
            _updatedFiles = updatedFiles;
            _sourceFolders = sourceFolders;
            _movedOrRenamedFiles = movedOrRenamedFiles;
            _movedOrRenamedBackupPaths = movedOrRenamedBackupPaths;
            _ignorableFolders = ignorableFolders;
            _backupSelected = IsAnyBackupSelected();
            OverwriteUpdatedFiles = overwriteUpdatedFiles;
            IncludeAllSubfolders = includeAllSubfolders;
        }

        /// <summary>
        /// A public method to invoke the <see cref="PropertyChanged"/> delegate.
        /// </summary>
        /// <param name="propertyName"> The name of the property from which the method is called. </param>
        public void OnPropertyChanged(string propertyName) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// If at least one backup path is selected in the UI, return true. If none are selected, return false.
        /// </summary>
        public bool IsAnyBackupSelected()
        {
            foreach (IBackupPathDto dto in _backupPaths.FullPaths)
            {
                if (dto.IsSelected == true) return true;
            }
            return false;
        }

        /// <summary>
        /// Ensures that any source files or backup paths that have been moved, renamed, or deleted do not display in
        /// the main UI.
        /// </summary>
        public void RemovePossibleRenamedFiles()
        {
            foreach(var file in _movedOrRenamedFiles)
            {
                if (_sourceFiles.Contains(file)) _sourceFiles.Remove(file);
                if (_updatedFiles.Contains(file)) _updatedFiles.Remove(file);
            }

            foreach(var path in _movedOrRenamedBackupPaths)
            {
                //if(_backupPaths.FileTreeItems.Contains(p => p = path.Path)) _backupPaths.RemovePath(path.Path);
            }
        }
    }
}
