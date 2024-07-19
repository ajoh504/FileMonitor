using Services;

namespace FileMonitorBackgroundService
{
    using Services.Helpers;

    public sealed class FileMonitorBackgroundService
    {
        private readonly SourceFileService? _sourceFileService;
        private readonly BackupPathService? _backupPathService;
        private readonly SourceFolderService? _sourceFolderService;
        private readonly IgnorableFolderService? _ignorableFolderService;

        private void FromDatabase()
        {
            _backupPathService.GetDirectories();
            _sourceFileService.GetFiles();
            _sourceFolderService.GetFolders();
            _ignorableFolderService.Get();
        }

        private void ComparedAgainstFileSystem()
        {
            _sourceFileService.GetModifiedFiles();
            _sourceFileService.GetMovedOrRenamedFiles();
            _backupPathService.GetMovedOrRenamedPaths();
        }

        // Run on system startup
        public void InitializeProjectData()
        {
            FromDatabase();
            ComparedAgainstFileSystem();
        }

        public FileMonitorBackgroundService()
        {
            using SourceFileService _sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());

            using BackupPathService _backupPathService = new BackupPathService(
                RepositoryHelper.CreateBackupPathRepositoryInstance());

            using SourceFolderService _sourceFolderService = new SourceFolderService(
                RepositoryHelper.CreateSourceFolderRepositoryInstance(),
                RepositoryHelper.CreateFolderFileMappingInstance(),
                RepositoryHelper.CreateSourceFileRepositoryInstance());

            using IgnorableFolderService _ignorableFolderService = new IgnorableFolderService(
                RepositoryHelper.CreateIgnorableFolderRepositoryInstance());
        }
    }
}
