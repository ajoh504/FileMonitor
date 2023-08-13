using DataAccessLayer;
using DataAccessLayer.Repositories;
using System.Configuration;

namespace Services.Helpers
{
    /// <summary>
    /// A helper class for instantiating the code repositories. Use this class to provide the service classes with the necessary repository.
    /// </summary>
    public class RepositoryHelper
    {
        /// <summary>
        /// Creates an instance of ISourceFileRepository.
        /// </summary>
        public static ISourceFileRepository CreateSourceFileRepositoryInstance()
            => new SourceFileRepository(CreateDbContextInstance());

        /// <summary>
        /// Creates an instance of IBackupPathRepository.
        /// </summary>
        public static IBackupPathRepository CreateBackupPathRepositoryInstance()
            => new BackupPathRepository(CreateDbContextInstance());

        /// <summary>
        /// Creates an instance of ISourceFolderRepository.
        /// </summary>
        public static ISourceFolderRepository CreateSourceFolderRepositoryInstance() 
            => new SourceFolderRepository(CreateDbContextInstance());

        /// <summary>
        /// Creates an instance of IFolderFileMappingRepository.
        /// </summary>
        public static IFolderFileMappingRepository CreateFolderFileMappingInstance() 
            => new FolderFileMappingRepository(CreateDbContextInstance());

        /// <summary>
        /// Creates an instance of the IIgnorableFolderRepository.
        /// </summary>
        public static IIgnorableFolderRepository CreateIgnorableFolderRepositoryInstance()
            => new IgnorableFolderRepository(CreateDbContextInstance());

        // A private method to create an instance of the program's database context.
        // The database context is required for construction of a repository instance.
        private static FileMonitorDbContext CreateDbContextInstance()
            => new FileMonitorDbContext(ConfigurationManager.ConnectionStrings[nameof(FileMonitorDbContext)].ConnectionString);
    }
}
