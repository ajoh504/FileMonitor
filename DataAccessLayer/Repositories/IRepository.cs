﻿using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public interface IRepository<TEntity> : IDisposable
    {
        // Remove a single row from the database
        void Remove(TEntity row);

        // Remove multiple rows from the database
        void RemoveRange(List<TEntity> rows);

        // Add a single row to the database
        void Add(TEntity row);

        // Add multiple rows to the database
        void AddRange(List<TEntity> rows);

        // Save changes to the database
        void SaveChanges();

        /// <summary> Retrieve multiple rows from the database. </summary>
        /// <typeparam name="TResult"> 
        ///     The return type of the query result, typically a data transfer object. 
        /// </typeparam>
        /// <typeparam name="TProperty"> 
        ///     The Entity property type to order the query results by.
        /// </typeparam>
        /// <param name="predicate"> 
        ///     A lambda expression to be transformed into a conditional statement. Set the
        ///     expression body to True to retrieve all values. Example: foo => True;
        /// </param>
        /// <param name="select"> 
        ///     A lambda expression for selecting specified Entity properties.
        /// </param>
        /// <param name="order">
        ///     An optional parameter for selecting the value to order the results by.
        /// </param>
        /// <param name="distinct">
        ///     Set to True to get distinct values from the query. If values are duplicated 
        ///     in the database, they will be shown only once.
        /// </param>
        /// <returns> A list of the query results. </returns>
        List<TResult> GetRange<TResult, TProperty>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>> select,
            Expression<Func<TEntity, TProperty>> order = null,
            bool distinct = false
        );

        /// <summary> Retrieve multiple rows from the database. </summary>
        /// <param name="predicate"> 
        /// A lambda expression to be transformed into a conditional statement. Set the
        /// expression body to True to retrieve all values. Example: foo => True;
        /// </param>
        /// <param name="asNoTracking"> 
        /// An optional parameter to stop Entity Framework from tracking changes on the Entity.
        /// Use for read-only scenarios.
        /// </param>
        /// <returns> A list of all specified Entities. </returns>
        List<TEntity> GetRange(
            Expression<Func<TEntity, bool>> predicate,
            bool asNoTracking = false
        );

        /// <summary>
        /// Retrieve the first value matching the predicate expression. If no match is 
        /// found then the default value is returned.
        /// </summary>
        /// <param name="predicate"> 
        /// A lambda expression to be transformed into a conditional statement. Set the
        /// expression body to True to retrieve all values. Example: foo => True;
        /// </param>
        /// <param name="asNoTracking"> 
        /// An optional parameter to stop Entity Framework from tracking changes on the Entity.
        /// Use for read-only scenarios.
        /// </param>
        /// <param name="includeProperties">
        /// An optional parameter to include any navigation properties (related Entities) with 
        /// the result. 
        /// </param>
        /// <returns> The first matching value or the default value. </returns>
        TEntity? FirstOrDefault(
            Expression<Func<TEntity, bool>> predicate,
            bool asNoTracking = true,
            params string[] includeProperties
        );

        /// <summary>
        /// Retrieve the first value matching the predicate expression. If no match is 
        /// found then the default value is returned.
        /// </summary>
        /// <typeparam name="TResult">
        /// The return type of the query result, typically a data transfer object. 
        /// </typeparam>
        /// <param name="predicate"> 
        /// A lambda expression to be transformed into a conditional statement. Set the
        /// expression body to True to retrieve all values. Example: foo => True;
        /// </param>
        /// <param name="select"> 
        /// A lambda expression for selecting specified Entity properties.
        /// </param>
        /// <returns> 
        /// The first value that matches the query parameters or the default value.
        /// </returns>
        TResult? FirstOrDefault<TResult>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>> select
        );

        /// <summary>
        /// Return True if the Entity exists in the database, false otherwise.
        /// </summary>
        /// <param name="predicate"> 
        /// A lambda expression to be transformed into a conditional statement. Set the
        /// expression body to True to retrieve all values. Example: foo => True;
        /// </param>
        bool Exists(Expression<Func<TEntity, bool>> predicate);
    }
}
