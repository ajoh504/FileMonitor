﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;

namespace FileMonitor.Database
{
    /// <summary>
    /// Defines an abstract class for all SQL table classes to inherit from 
    /// </summary>
    internal abstract class BaseTable
    {
        /// <summary>
        /// Return an opened database connection object
        /// </summary>
        protected SQLiteConnection GetConnection()
        {
            SQLiteConnection connection = new SQLiteConnection($"Data Source={MainWindow.databasePath};Version=3;");
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Check a list of IDs and retrieve the next available ID
        /// </summary>
        protected int GetNextAvailableID(List<int> iDs)
        {
            if (iDs.Count == 0) return 0;
            else
            {
                int lastId = iDs[iDs.Count - 1];
                return 1 + lastId;
            }
        }

        /// <summary>
        /// Get a list of object values from the specified database column
        /// </summary>
        /// <remarks> 
        /// Example query: "SELECT {column} FROM {table}" where "column" and 
        /// "table" represent the method arguments.
        /// </remarks>
        protected List<object> SQLSelectFromColumn(string table, string column)
        {
            List<object> result = new List<object>();
            string query = $"SELECT {column} FROM {table}";
            using (SQLiteConnection connection = GetConnection())
            {
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read()) result.Add(reader[column]);
                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// A method for executing the SQL INSERT statement
        /// Sample statement: INSERT INTO source_file (id, path) values (56, 'C:\NewFilePath') 
        /// </summary>
        /// <remarks> Overload for any table that has an integer and a varchar column </remarks>
        protected void SQLInsertInto(string table, string column1, string column2, int id, string varCharValue)
        {
            string insertStatement = $"INSERT INTO {table} ({column1}, {column2}) values ({id}, \'{varCharValue}\')";
            using (SQLiteConnection connection = GetConnection())
            {
                using (SQLiteCommand command = new SQLiteCommand(insertStatement, connection))
                {
                    command.ExecuteNonQuery();
                }
            }         
        }

        /// <summary>
        /// A method for executing the SQL INSERT statement
        /// Sample statement: INSERT INTO source_file_hash_rel (source_file_id, source_hash_id) values (56, 72) 
        /// </summary>
        /// <remarks> Overload for any table that has two integer columns </remarks>
        protected void SQLInsertInto(string table, string column1, string column2, int id1, int id2)
        {
            string insertStatement = $"INSERT INTO {table} ({column1}, {column2}) values ({id1}, {id2})";
            using (SQLiteConnection connection = GetConnection())
            {
                using (SQLiteCommand command = new SQLiteCommand(insertStatement, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Cast a List of objects to a List of the specified type
        /// </summary>
        /// <param name="values"> A List of objects from the database column </param>
        protected List<T>? ToGenericList<T>(List<object> values)
        {
            List<T> valuesCast = new List<T>();
            foreach (object entry in values)
            {
                valuesCast.Add((T)entry); // Cast object to <T>
            }
            return valuesCast;
        }
    }
}
