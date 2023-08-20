using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// A repository for the SourceFolder Entity. This class inherits from <see cref="RepositoryBase{TEntity}"/> and
    /// <see cref="ISourceFolderRepository"/>
    /// </summary>
    internal class SourceFolderRepository : RepositoryBase<SourceFolder>, ISourceFolderRepository
    {
        /// <summary>
        /// Defines the <see cref="SourceFolderRepository"/> class constructor.
        /// </summary>
        /// <param name="db"> The database context object of type <see cref="FileMonitorDbContext"/>. Provides access
        /// to the Entity Framework API. This parameter is passed to the <c>base()</c> constructor of the <see cref=
        /// "RepositoryBase{TEntity}"/> class. </param>
        internal SourceFolderRepository(FileMonitorDbContext db) : base(db)
        {
        }
    }
}
