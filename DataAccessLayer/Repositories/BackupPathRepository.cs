﻿using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// A repository for the BackupPath Entity. This class inherits from <see cref="RepositoryBase{TEntity}"/> and <see cref="IBackupPathRepository"/>.
    /// </summary>
    public class BackupPathRepository: RepositoryBase<BackupPath>, IBackupPathRepository
    {
        /// <summary>
        /// Defines the class constructor.
        /// </summary>
        /// <param name="db"> 
        /// The database context object of type <see cref="FileMonitorDbContext"/>. Provides access to the Entity Framework API. This parameter is passed
        /// to the <code>base()</code> constructor of the <see cref="RepositoryBase{TEntity}"/> class.
        /// </param>
        public BackupPathRepository(FileMonitorDbContext db) : base(db)
        {
        }
    }
}
