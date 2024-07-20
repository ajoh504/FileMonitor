using Services;

namespace FileMonitorBackgroundService
{
    using Services.Helpers;

    public sealed class FileMonitorBackgroundService
    {
        public void Run()
        {
            using var _sourceFileService = new SourceFileService(
                RepositoryHelper.CreateSourceFileRepositoryInstance());

            _sourceFileService.CompareHashes();
        }
    }
}
