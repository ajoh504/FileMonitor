using Services;

namespace FileMonitorBackgroundService
{
    using Services.Helpers;

    public sealed class FileMonitorBackgroundService
    {
        private void FromDatabase()
        {
            using var _sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());

            using var _backupPathService = new BackupPathService(
                RepositoryHelper.CreateBackupPathRepositoryInstance());

            using var _sourceFolderService = new SourceFolderService(
                RepositoryHelper.CreateSourceFolderRepositoryInstance(),
                RepositoryHelper.CreateFolderFileMappingInstance(),
                RepositoryHelper.CreateSourceFileRepositoryInstance());

            using var _ignorableFolderService = new IgnorableFolderService(
                RepositoryHelper.CreateIgnorableFolderRepositoryInstance());

            var _directories = _backupPathService.GetDirectories();
            var _files = _sourceFileService.GetFiles();
            var _folders = _sourceFolderService.GetFolders();
            var _ignorableFolders = _ignorableFolderService.Get();
        }

        private void ComparedAgainstFileSystem()
        {
            using var _sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());

            using var _backupPathService = new BackupPathService(
                RepositoryHelper.CreateBackupPathRepositoryInstance());

            _sourceFileService.CompareHashes();
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
    }
}
