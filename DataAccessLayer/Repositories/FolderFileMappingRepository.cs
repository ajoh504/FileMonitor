using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// A repository for the FolderFileMapping Entity. This class inherits from <see cref="RepositoryBase{TEntity}"/>
    /// and <see cref="IFolderFileMappingRepository"/>.
    /// </summary>
    internal class FolderFileMappingRepository : RepositoryBase<FolderFileMapping>, IFolderFileMappingRepository
    {
        /// <summary>
        /// Defines the <see cref="FolderFileMappingRepository"/> class constructor.
        /// </summary>
        /// <param name="db"> The database context object of type <see cref="FileMonitorDbContext"/>. Provides access
        /// to the Entity Framework API. This parameter is passed to the <c>base()</c> constructor of the <see cref=
        /// "RepositoryBase{TEntity}"/> class. </param>
        internal FolderFileMappingRepository(FileMonitorDbContext db) : base(db)
        {
        }
    }
}
