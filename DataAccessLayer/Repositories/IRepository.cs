using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// A public interface providing database access through LINQ Expressions and the Entity Framework API.
    /// </summary>
    /// <typeparam name="TEntity"> The Entity type for this <see cref="IRepository{TEntity}"/> instance. </typeparam>
    public interface IRepository<TEntity> : IDisposable
    {
        /// <summary> 
        /// Remove a single row from the database.
        /// </summary>
        /// <param name="row"> The row to remove from the database, designated by the Id. </param>
        void Remove(TEntity row);

        /// <summary>
        /// Remove multiple rows from the database.
        /// </summary>
        /// <param name="rows"> A list of rows to remove from the database, designated by the Ids. </param>
        void RemoveRange(List<TEntity> rows);

        /// <summary>
        /// Add a single row to the database.
        /// </summary>
        /// <param name="row"> The Entity to add to the database. </param>
        void Add(TEntity row);

        /// <summary>
        /// Add multiple rows to the database.
        /// </summary>
        /// <param name="rows"> A list of Entities to add to the database. </param>
        void AddRange(List<TEntity> rows);

        /// <summary>
        /// Save any tracked changes to the database. Changes on the Entities are tracked by default unless 
        /// "asNoTracking" is enabled, but changes are not saved until this method is called. 
        /// </summary>
        void SaveChanges();

        /// <summary> 
        /// Retrieve multiple rows from the database. 
        /// </summary>
        /// <typeparam name="TResult">
        /// The return type of the query result, typically a data transfer object. The <paramref name="select"/> lambda
        /// expression transforms the Entity into the TResult type. TResult is inferred by the compiler based on the
        /// method arguments. 
        /// </typeparam>
        /// <typeparam name="TProperty"> 
        /// The Entity property type to order the query results by. TProperty is inferred by the compiler based on the
        /// method arguments. 
        /// </typeparam>
        /// <param name="predicate"> 
        ///     <para>
        ///         A lambda expression to be transformed into a conditional statement. The <paramref name=
        ///         "predicate"/> expression is passed to <c>IQueryable&lt;TEntity&gt;.Where()</c>, which returns an
        ///         object of type <see cref="IQueryable{TEntity}"/>
        ///     </para>
        ///     <remarks>
        ///         Set the expression body to true to search through all values. Example: <c> foo => true; </c>.
        ///     </remarks>
        /// </param>
        /// <param name="select"> A lambda expression for selecting specified Entity properties. </param>
        /// <param name="order"> An optional parameter for selecting the value to order the results by. </param>
        /// <param name="distinct"> 
        /// An optional parameter to get distinct values from the query. If values are duplicated in the database, they
        /// will be shown only once. 
        /// </param>
        /// <returns> A list of the query results. </returns>
        /// <remarks>
        /// Example method call:
        /// <code>
        /// GetRange(
        ///     foo => foo.Id > 100, // Get all Entities of type Foo with an Id greater than 100
        ///     foo => new FooDto // Transform all matching Foo entities into FooDto i.e., a data transfer object.
        ///     {
        ///         Id = foo.Id,
        ///         Value = foo.Value
        ///     })
        /// </code>
        /// </remarks>
        List<TResult> GetRange<TResult, TProperty>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>> select,
            Expression<Func<TEntity, TProperty>> order = null,
            bool distinct = false
        );

        /// <summary>
        /// Retrieve multiple rows from the database. 
        /// </summary>
        /// <param name="predicate"> 
        ///     <para>
        ///         A lambda expression to be transformed into a conditional statement. The <paramref name=
        ///         "predicate"/> expression is passed to <c>IQueryable&lt;TEntity&gt;.Where()</c>, which returns an 
        ///         object of type <see cref="IQueryable{TEntity}" />
        ///     </para>
        ///     <remarks>
        ///         Set the expression body to true to search through all values. Example: <c> foo => true; </c>.
        ///     </remarks>
        /// </param>
        /// <param name="asNoTracking">
        /// An optional parameter to stop Entity Framework from tracking changes on the Entity. Use for read-only
        /// scenarios.
        /// </param>
        /// <returns> A list of all matching Entities. </returns>
        /// <remarks>
        /// Example method call:
        /// <code>
        /// GetRange(
        ///     foo => true, // Get all Entities.
        ///     true) // Do not track changes on the Entities.
        /// </code>
        /// </remarks>
        List<TEntity> GetRange(
            Expression<Func<TEntity, bool>> predicate,
            bool asNoTracking = false
        );

        /// <summary>
        /// Retrieve the first value matching the predicate expression. If no match is found then the default value is
        /// returned.
        /// </summary>
        /// <param name="predicate"> 
        ///     <para>
        ///         A lambda expression to be transformed into a conditional statement. The <paramref name="predicate"/>
        ///         expression is passed to <c>IQueryable&lt;TEntity&gt;.FirstOrDefault()</c>, which returns the first
        ///         occurrence of the requested type, or the default value.
        ///     </para>
        ///     <remarks>
        ///         Set the expression body to true to search through all values. Example: <c> foo => true; </c>.
        ///     </remarks>
        /// </param>
        /// <param name="asNoTracking">
        /// An optional parameter to stop Entity Framework from tracking changes on the Entity. Use for read-only
        /// scenarios.
        /// </param>
        /// <param name="includeProperties"> 
        /// An optional parameter to include any navigation properties (related Entities) with the result.
        /// </param>
        /// <returns> The first matching value or the default value. </returns>
        /// <remarks>
        /// Example method call:
        /// <code>
        /// FirstOrDefault(foo => foo.Id > 100)
        /// </code>
        /// </remarks>
        TEntity? FirstOrDefault(
            Expression<Func<TEntity, bool>> predicate,
            bool asNoTracking = true,
            params string[] includeProperties
        );

        /// <summary>
        /// Retrieve the first value matching the predicate expression. If no match is found then the default value is
        /// returned.
        /// </summary>
        /// <typeparam name="TResult"> 
        /// The return type of the query result, typically a data transfer object. The <paramref name="select"/> lambda
        /// expression transforms the Entity into the provided TResult type. 
        /// </typeparam>
        /// <param name="predicate"> 
        ///     <para>
        ///         A lambda expression to be transformed into a conditional statement. The <paramref name=
        ///         "predicate"/> expression is passed to <c>IQueryable&lt;TEntity&gt;.FirstOrDefault()</c>, which
        ///         returns the first occurrence of the requested type, or the default value.
        ///     </para>
        ///     <remarks>
        ///         Set the expression body to true to search through all values. Example: <c> foo => true; </c>.
        ///     </remarks>
        /// </param>
        /// <param name="select"> A lambda expression for selecting specified Entity properties. </param>
        /// <returns> The first value that matches the query parameters or the default value. </returns>
        /// <remarks>
        /// Example method call:
        /// <code>
        /// FirstOrDefault(
        ///     foo => foo.Id > 100,
        ///     foo => new FooDto
        ///     {
        ///         Id = foo.Id,
        ///         Value = foo.Value
        ///     })
        /// </code>
        /// </remarks>
        TResult? FirstOrDefault<TResult>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>> select
        );

        /// <summary>
        /// Return true if the Entity exists in the database, false otherwise.
        /// </summary>
        /// <param name="predicate"> 
        ///     <para>
        ///         A lambda expression to be transformed into a conditional statement. Example: 
        ///         <c> foo => foo.Id > 100; </c>. The <paramref name="predicate"/> expression is passed to 
        ///         <c>IQueryable&lt;TEntity&gt;.Any()</c>, which returns true if the value exists in the database,
        ///         false otherwise.
        ///     </para>
        ///     <remarks>
        ///         Set the expression body to true to search through all values. Example: <c> foo => true; </c>.
        ///     </remarks>
        /// </param>
        /// <remarks>
        /// Example method call:
        /// <code>
        /// Exists(foo => foo.Id > 100)
        /// </code>
        /// </remarks>
        bool Exists(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Update Entity records in the database.
        /// </summary>
        /// <param name="predicate"> 
        ///     <para>
        ///         A lambda expression to be transformed into a conditional statement. Example:
        ///         <c> foo => foo.Id > 100; </c>. The <paramref name="predicate"/> expression is passed to
        ///         <c>IQueryable&lt;TEntity&gt;.Where()</c>, which returns an object of type <see cref=
        ///         "IQueryable{TEntity}" />
        ///     </para>
        ///     <remarks>
        ///         Set the expression body to true to search through all values. Example: <c> foo => true; </c>.
        ///     </remarks>
        /// </param>
        /// <param name="update">
        /// A lambda expression of type <see cref="Action{TEntity}"/>. Specifies the action to be performed on the
        /// Entity or Entities.
        /// </param>
        public void Update(
            Expression<Func<TEntity, bool>> predicate,
            Action<TEntity> update
        );
    }
}
