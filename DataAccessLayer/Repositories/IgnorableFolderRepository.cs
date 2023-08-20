using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// A repository for the IgnorableFolder Entity. This class inherits from <see cref="RepositoryBase{TEntity}"/> and
    /// <see cref="IIgnorableFolderRepository"/>.
    /// </summary>
    internal class IgnorableFolderRepository : RepositoryBase<IgnorableFolder>, IIgnorableFolderRepository
    {
        /// <summary>
        /// Defines the <see cref="IgnorableFolderRepository"/> class constructor.
        /// </summary>
        /// <param name="db"> The database context object of type <see cref="FileMonitorDbContext"/>. Provides access
        /// to the Entity Framework API. This parameter is passed to the <c>base()</c> constructor of the <see cref=
        /// "RepositoryBase{TEntity}"/> class. </param>
        public IgnorableFolderRepository(FileMonitorDbContext db) : base(db)
        {
        }
    }
}
