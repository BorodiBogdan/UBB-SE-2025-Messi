// <copyright file="MockDatabaseConnectionPostRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Duo.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Duo.Models;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Mock implementation of IDatabaseConnection for testing post repository operations.
    /// </summary>
    public class MockDatabaseConnectionPostRepository : IDatabaseConnection
    {
        private readonly MockDataTablesPost mockDataTables = new MockDataTablesPost();

        /// <summary>
        /// Initializes a new instance of the <see cref="MockDatabaseConnectionPostRepository"/> class.
        /// </summary>
        public MockDatabaseConnectionPostRepository()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether ExecuteReader should throw an exception.
        /// </summary>
        public bool ThrowExceptionOnExecuteReader { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether ExecuteScalar should throw an exception.
        /// </summary>
        public bool ThrowExceptionOnExecuteScalar { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to return an empty result set.
        /// </summary>
        public bool ReturnEmptyResultSet { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to return a table with zero rows but with defined columns.
        /// </summary>
        public bool ReturnZeroRowsButNonEmpty { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to throw a SQL exception.
        /// </summary>
        public bool ThrowSqlException { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to return a table with incorrect column names.
        /// </summary>
        public bool ReturnTableWithWrongColumns { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether ExecuteScalar should return null.
        /// </summary>
        public bool ReturnNullForExecuteScalar { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to complete all loops in data operations.
        /// </summary>
        public bool EnsureLoopCompletion { get; set; } = false;

        /// <inheritdoc/>
        public void CloseConnection()
        {
        }

        /// <inheritdoc/>
        public int ExecuteNonQuery(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            if (this.ThrowSqlException && (storedProcedure == "IncrementPostLikeCount" || storedProcedure == "DeletePost"))
            {
                throw new Exception("SQL Exception");
            }

            return 1;
        }

        /// <inheritdoc/>
        public DataTable ExecuteReader(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            if (this.ThrowExceptionOnExecuteReader)
            {
                throw new Exception("Test exception thrown from mock");
            }

            var postDataTable = this.mockDataTables.PostRepositoryDataTable;

            if (this.ReturnEmptyResultSet)
            {
                DataTable emptyTable = postDataTable.Clone();
                return emptyTable;
            }

            if (this.ReturnZeroRowsButNonEmpty)
            {
                DataTable zeroRowsTable = postDataTable.Clone();
                return zeroRowsTable;
            }

            if (this.ReturnTableWithWrongColumns)
            {
                DataTable wrongColumnsTable = new DataTable();
                wrongColumnsTable.Columns.Add("DifferentId", typeof(int));
                wrongColumnsTable.Columns.Add("DifferentTitle", typeof(string));
                wrongColumnsTable.Rows.Add(1, "Wrong Column Name");
                return wrongColumnsTable;
            }

            if (storedProcedure == "GetPostById")
            {
                int postId = sqlParameters?[0].Value?.ToString() == null ? 0 : Convert.ToInt32(sqlParameters[0].Value.ToString());

                DataTable resultTable = postDataTable.Clone();
                foreach (DataRow row in postDataTable.Rows)
                {
                    if (Convert.ToInt32(row["Id"]) == postId)
                    {
                        resultTable.ImportRow(row);
                    }
                }
                return resultTable;
            }
            else if (storedProcedure == "GetPostsByCategory" || storedProcedure == "GetPostsByUser" || storedProcedure == "GetByHashtags")
            {
                DataTable resultTable = postDataTable.Clone();
                int skip = 0;
                int take = 10;
                if (storedProcedure == "GetPostsByCategory")
                {
                    int categoryId = sqlParameters?[0].Value?.ToString() == null ? 0 : Convert.ToInt32(sqlParameters[0].Value.ToString());
                    take = sqlParameters?[1].Value?.ToString() == null ? 10 : Convert.ToInt32(sqlParameters[1].Value.ToString());
                    skip = sqlParameters?[2].Value?.ToString() == null ? 0 : Convert.ToInt32(sqlParameters[2].Value.ToString());

                    int count = 0;
                    int added = 0;
                    foreach (DataRow row in postDataTable.Rows)
                    {
                        if (Convert.ToInt32(row["CategoryID"]) == categoryId)
                        {
                            if (count >= skip && added < take)
                            {
                                resultTable.ImportRow(row);
                                added++;
                            }

                            count++;
                        }
                    }
                }
                else if (storedProcedure == "GetPostsByUser")
                {
                    int userId = sqlParameters?[0].Value?.ToString() == null ? 0 : Convert.ToInt32(sqlParameters[0].Value.ToString());
                    take = sqlParameters?[1].Value?.ToString() == null ? 10 : Convert.ToInt32(sqlParameters[1].Value.ToString());
                    skip = sqlParameters?[2].Value?.ToString() == null ? 0 : Convert.ToInt32(sqlParameters[2].Value.ToString());

                    int count = 0;
                    int added = 0;
                    foreach (DataRow row in postDataTable.Rows)
                    {
                        if (Convert.ToInt32(row["UserID"]) == userId)
                        {
                            if (count >= skip && added < take)
                            {
                                resultTable.ImportRow(row);
                                added++;
                            }

                            count++;
                        }
                    }

                    if (userId == 999)
                    {
                        resultTable = postDataTable.Clone();
                    }
                }
                else if (storedProcedure == "GetByHashtags")
                {
                    take = sqlParameters?[1].Value?.ToString() == null ? 10 : Convert.ToInt32(sqlParameters[1].Value.ToString());
                    skip = sqlParameters?[2].Value?.ToString() == null ? 0 : Convert.ToInt32(sqlParameters[2].Value.ToString());

                    int count = 0;
                    int added = 0;
                    foreach (DataRow row in postDataTable.Rows)
                    {
                        if (count >= skip && added < take)
                        {
                            resultTable.ImportRow(row);
                            added++;
                        }

                        count++;
                        if (added >= 3)
                        {
                            break;
                        }
                    }
                }

                return resultTable;
            }
            else if (storedProcedure == "GetAllPostTitles")
            {
                DataTable titlesTable = new DataTable();
                titlesTable.Columns.Add("Title", typeof(string));

                foreach (DataRow row in postDataTable.Rows)
                {
                    titlesTable.Rows.Add(row["Title"]);
                }

                return titlesTable;
            }
            else if (storedProcedure == "GetPostsByTitle")
            {
                string title = sqlParameters?[0].Value?.ToString() ?? string.Empty;
                if (title == "NonExistentTitle")
                {
                    return postDataTable.Clone();
                }

                DataTable resultTable = postDataTable.Clone();
                foreach (DataRow row in postDataTable.Rows)
                {
                    string rowTitle = Convert.ToString(row["Title"]) ?? string.Empty;
                    if (rowTitle.Contains(title, StringComparison.OrdinalIgnoreCase))
                    {
                        resultTable.ImportRow(row);
                    }
                }

                return resultTable;
            }
            else if (storedProcedure == "GetUserIdByPostId")
            {
                int postId = sqlParameters?[0].Value?.ToString() == null ? 0 : Convert.ToInt32(sqlParameters[0].Value.ToString());

                var userIdTable = new DataTable();
                userIdTable.Columns.Add("UserId", typeof(int));

                foreach (DataRow row in postDataTable.Rows)
                {
                    if (Convert.ToInt32(row["Id"]) == postId)
                    {
                        userIdTable.Rows.Add(Convert.ToInt32(row["UserID"]));
                        if (!this.EnsureLoopCompletion)
                        {
                            break;
                        }
                    }
                }

                return userIdTable;
            }
            else if (storedProcedure == "GetPaginatedPosts")
            {
                int pageSize = sqlParameters?[0].Value?.ToString() == null ? 10 : Convert.ToInt32(sqlParameters[0].Value.ToString());
                int offset = sqlParameters?[1].Value?.ToString() == null ? 0 : Convert.ToInt32(sqlParameters[1].Value.ToString());

                DataTable resultTable = postDataTable.Clone();
                int count = 0;
                int added = 0;
                foreach (DataRow row in postDataTable.Rows)
                {
                    if (count >= offset && added < pageSize)
                    {
                        resultTable.ImportRow(row);
                        added++;
                    }

                    count++;
                }

                return resultTable;
            }

            return postDataTable.Clone();
        }

        /// <inheritdoc/>
        public T? ExecuteScalar<T>(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            if (this.ThrowExceptionOnExecuteScalar)
            {
                if (this.ThrowSqlException && storedProcedure == "CreatePost")
                {
                    throw new Exception("Test SQL exception message");
                }

                throw new Exception("Test exception thrown from mock for ExecuteScalar");
            }

            if (this.ReturnNullForExecuteScalar && storedProcedure == "CreatePost")
            {
                return default;
            }

            if (storedProcedure == "CreatePost")
            {
                return (T)Convert.ChangeType(1, typeof(T));
            }
            else if (storedProcedure == "GetTotalPostCount")
            {
                int count = this.mockDataTables.PostRepositoryDataTable.Rows.Count;
                return (T)Convert.ChangeType(count, typeof(T));
            }
            else if (storedProcedure == "GetPostCountByCategory")
            {
                int categoryId = Convert.ToInt32(sqlParameters![0].Value);
                int count = 0;
                foreach (DataRow row in this.mockDataTables.PostRepositoryDataTable.Rows)
                {
                    if (Convert.ToInt32(row["CategoryID"]) == categoryId)
                    {
                        count++;
                    }
                }

                return (T)Convert.ChangeType(count, typeof(T));
            }
            else if (storedProcedure == "GetPostCountByHashtags")
            {
                return (T)Convert.ChangeType(3, typeof(T));
            }

            return default;
        }

        /// <inheritdoc/>
        public void OpenConnection()
        {
        }
    }
}