using DataAccessLayer.Entities;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// A repository interface for the SourceFile Entity.
    /// </summary>
    public interface ISourceFileRepository: IRepository<SourceFile>
    {
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
        );
    }
}
