using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// A repository for the BackupPath Entity. This class inherits from <see cref="RepositoryBase{BackupPath}"/> and
    /// <see cref="IBackupPathRepository"/>.
    /// </summary>
    internal class BackupPathRepository: RepositoryBase<BackupPath>, IBackupPathRepository
    {
        /// <summary>
        /// Defines the class constructor.
        /// </summary>
        /// <param name="db"> The database context object of type <see cref="FileMonitorDbContext"/>. Provides access
        /// to the Entity Framework API. This parameter is passed to the <c>base()</c> constructor of the <see cref=
        /// "RepositoryBase{TEntity}"/> class. </param>
        internal BackupPathRepository(FileMonitorDbContext db) : base(db)
        {
        }
    }
}
