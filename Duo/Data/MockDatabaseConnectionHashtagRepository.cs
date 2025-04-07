using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using Duo.Models;
using Microsoft.UI.Xaml.Controls;

namespace Duo.Data
{
    public class MockDatabaseConnectionHashtagRepository : IDatabaseConnection
    {
        private MockDataTables _mockDataTables = new MockDataTables();
        private bool _shouldThrowSqlException = false;
        private Dictionary<string, DataTable> _customResults = new Dictionary<string, DataTable>();
        private HashSet<string> _queryErrors = new HashSet<string>();

        public MockDatabaseConnectionHashtagRepository() { }

        public void SetShouldThrowSqlException(bool shouldThrow)
        {
            _shouldThrowSqlException = shouldThrow;
        }

        public void ClearCustomResults()
        {
            _customResults.Clear();
            _queryErrors.Clear();
            _shouldThrowSqlException = false;
        }

        public void SetupEmptyResult(string storedProcedure, DataTable result)
        {
            ClearCustomResults();
            _customResults[storedProcedure] = result;
        }

        public void SetupQueryError(string storedProcedure)
        {
            _queryErrors.Add(storedProcedure);
        }

        public void CloseConnection()
        {
        }

        public int ExecuteNonQuery(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            int postId, hashtagId;
            if (_queryErrors.Contains(storedProcedure))
                return 0; // Simulate query error

            if (storedProcedure == "AddHashtagToPost")
            {
                postId = ConvertSqlParameterToInt(sqlParameters?[0]); // PostID
                hashtagId = ConvertSqlParameterToInt(sqlParameters?[1]); // HashtagID


                return 1;
            }
            postId = ConvertSqlParameterToInt(sqlParameters?[0]); // PostID
            hashtagId = ConvertSqlParameterToInt(sqlParameters?[1]); // HashtagID

            CloseConnection();
            return 1;
        }

        public DataTable ExecuteReader(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            this.OpenConnection();
            if (_customResults.ContainsKey(storedProcedure))
            {
                var result = _customResults[storedProcedure];

                return result;
            }

            var HashtagRepositoryDataTABLE = _mockDataTables.HashtagRepositoryDataTABLE;

            if (storedProcedure == "GetHashtagByText")
            {
                string text = sqlParameters?[0]?.Value?.ToString() ?? "";
                if (text == "error")
                    throw new SqlExceptionThrower().throwSqlException();

                return HashtagRepositoryDataTABLE.AsEnumerable()
                    .Where(row => row.Field<string>("Tag") == text)
                    .CopyToDataTable();
            }
            else if (storedProcedure == "GetHashtagsForPost")
            {
                int postId = ConvertSqlParameterToInt(sqlParameters?[0]);

                return HashtagRepositoryDataTABLE.AsEnumerable()
                    .Where(row => row.Field<int>("PostID") == postId)
                    .CopyToDataTable();
            }
            else if (storedProcedure == "GetAllHashtags")
            {
                if (_shouldThrowSqlException)
                    throw new SqlExceptionThrower().throwSqlException();

                return HashtagRepositoryDataTABLE;
            }
            int categoryId = ConvertSqlParameterToInt(sqlParameters?[0]);

            return HashtagRepositoryDataTABLE.AsEnumerable()
                .Where(row => row.Field<int>("CategoryID") == categoryId)
                .CopyToDataTable();
        }

        public T? ExecuteScalar<T>(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            if (_queryErrors.Contains(storedProcedure))
                return (T)Convert.ChangeType(0, typeof(T)); // Simulate query error

            string tag = sqlParameters?[0]?.Value?.ToString() ?? "";

            if (tag == "error")
                throw new SqlExceptionThrower().throwSqlException();
            return (T)Convert.ChangeType(1, typeof(T));


        }

        public void OpenConnection()
        {

        }

        private int ConvertSqlParameterToInt(SqlParameter? param)
        {
            int convertedValue = param.Value?.ToString() == null ? 0 : Convert.ToInt32(param.Value.ToString());

            if (convertedValue == 404)
                throw new SqlExceptionThrower().throwSqlException();

            return convertedValue;
        }
    }
}