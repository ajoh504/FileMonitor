using Services;

namespace FileMonitorBackgroundService
{
    using Services.Helpers;

    public sealed class FileMonitorBackgroundService
    {
        public int ChangedFileCount { get; private set; }

        public void Run()
        {
            using var _sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());

            _sourceFileService.FilesChanged += changedFilesFound;
            _sourceFileService.CompareHashes();
        }

        public void changedFilesFound(object sender, FilesChangedEventArgs e) 
        { 
            ChangedFileCount = e.ChangedFileCount;
        }
    }
}
