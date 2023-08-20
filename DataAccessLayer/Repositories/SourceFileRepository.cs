using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// A repository for the SourceFile Entity. This class inherits from <see cref="RepositoryBase{TEntity}"/> and 
    /// <see cref="ISourceFileRepository"/>.
    /// </summary>
    internal class SourceFileRepository : RepositoryBase<SourceFile>, ISourceFileRepository
    {
        /// <summary>
        /// Defines the <see cref="SourceFileRepository"/> class constructor.
        /// </summary>
        /// <param name="db"> The database context object of type <see cref="FileMonitorDbContext"/>. Provides access
        /// to the Entity Framework API. This parameter is passed to the <c>base()</c> constructor of the <see cref=
        /// "RepositoryBase{TEntity}"/> class. </param>
        internal SourceFileRepository(FileMonitorDbContext db) : base(db)
        {
        }
    }
}
