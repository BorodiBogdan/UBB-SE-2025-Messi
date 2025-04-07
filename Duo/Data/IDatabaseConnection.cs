// <copyright file="IDatabaseConnection.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Duo.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Interface for managing database connections and executing database operations.
    /// </summary>
    public interface IDatabaseConnection
    {
        /// <summary>
        /// Opens a connection to the database.
        /// </summary>
        public void OpenConnection();

        /// <summary>
        /// Closes the current database connection.
        /// </summary>
        public void CloseConnection();

        /// <summary>
        /// Executes a stored procedure and returns a single value of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the value to return.</typeparam>
        /// <param name="storedProcedure">The name of the stored procedure to execute.</param>
        /// <param name="sqlParameters">Optional array of SQL parameters for the stored procedure.</param>
        /// <returns>The first column of the first row in the result set, or null if the result set is empty.</returns>
        public T? ExecuteScalar<T>(string storedProcedure, SqlParameter[]? sqlParameters = null);

        /// <summary>
        /// Executes a stored procedure and returns the results as a DataTable.
        /// </summary>
        /// <param name="storedProcedure">The name of the stored procedure to execute.</param>
        /// <param name="sqlParameters">Optional array of SQL parameters for the stored procedure.</param>
        /// <returns>A DataTable containing the results of the query.</returns>
        public DataTable ExecuteReader(string storedProcedure, SqlParameter[]? sqlParameters = null);

        /// <summary>
        /// Executes a stored procedure that does not return any data.
        /// </summary>
        /// <param name="storedProcedure">The name of the stored procedure to execute.</param>
        /// <param name="sqlParameters">Optional array of SQL parameters for the stored procedure.</param>
        /// <returns>The number of rows affected by the execution.</returns>
        public int ExecuteNonQuery(string storedProcedure, SqlParameter[]? sqlParameters = null);
    }
}