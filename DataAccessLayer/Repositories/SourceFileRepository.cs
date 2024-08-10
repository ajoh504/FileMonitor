using DataAccessLayer.Entities;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// A repository for the SourceFile Entity. This class inherits from <see cref="RepositoryBase{TEntity}"/> and 
    /// <see cref="ISourceFileRepository"/>.
    /// </summary>
    public class SourceFileRepository : RepositoryBase<SourceFile>, ISourceFileRepository
    {
        /// <summary>
        /// Defines the <see cref="SourceFileRepository"/> class constructor.
        /// </summary>
        /// <param name="db"> The database context object of type <see cref="FileMonitorDbContext"/>. Provides access
        /// to the Entity Framework API. This parameter is passed to the <c>base()</c> constructor of the <see cref=
        /// "RepositoryBase{TEntity}"/> class. </param>
        public SourceFileRepository(FileMonitorDbContext db) : base(db)
        {
        }

        /// <summary>
        /// Get SourceFile entities paginated.
        /// </summary>
        /// <typeparam name="TResult">
        /// The DTO for the projected return value.
        /// </typeparam>
        /// <param name="predicate">A lambda expression to be transformed into a conditional statement.</param>
        /// <param name="select">A lambda expression for selecting specified Entity properties.</param>
        /// <param name="lastId">Database primary key that signifies the starting point of the curent page.</param>
        /// <param name="pageCount">The number of pages.</param>
        /// <returns>A list of the query results.</returns>
        public List<TResult> GetRangePaginated<TResult>(
            Expression<Func<SourceFile, bool>> predicate,
            Expression<Func<SourceFile, TResult>> select,
            int lastId = -1,
            int pageCount = 50
        )
        {
            IQueryable<SourceFile> query = _dbSet
                .Where(predicate)
                .Where(sourceFile => sourceFile.Id > lastId)
                .OrderBy(sourceFile => sourceFile.Id)
                .Take(pageCount);

            IQueryable<TResult> projected = query.Select(select);

            return projected.ToList();
        }
    }
}
