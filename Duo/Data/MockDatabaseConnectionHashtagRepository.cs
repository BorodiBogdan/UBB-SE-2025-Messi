using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using Duo.Models;

namespace Duo.Data
{
    public class MockDatabaseConnectionHashtagRepository : IDatabaseConnection
    {
        private MockDataTables _mockDataTables = new MockDataTables();
        private bool _shouldThrowSqlException = false;

        public MockDatabaseConnectionHashtagRepository() { }

        public void SetShouldThrowSqlException(bool shouldThrow)
        {
            _shouldThrowSqlException = shouldThrow;
        }

        public void CloseConnection()
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            if (storedProcedure == "AddHashtagToPost")
            {
                int postId = ConvertSqlParameterToInt(sqlParameters?[0]); // PostID
                int hashtagId = ConvertSqlParameterToInt(sqlParameters?[1]); // HashtagID
                
                if (postId == 404 || hashtagId == 404)
                    throw new SqlExceptionThrower().throwSqlException();
                    
                return 1;
            }
            else if (storedProcedure == "DeleteHashtagFromPost")
            {
                int postId = ConvertSqlParameterToInt(sqlParameters?[0]); // PostID
                int hashtagId = ConvertSqlParameterToInt(sqlParameters?[1]); // HashtagID
                
                if (postId == 404 || hashtagId == 404)
                    throw new SqlExceptionThrower().throwSqlException();
                    
                return 1;
            }

            throw new NotImplementedException();
        }

        public DataTable ExecuteReader(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
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
                if (postId == 404)
                    throw new SqlExceptionThrower().throwSqlException();
                    
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
            else if (storedProcedure == "GetHashtagsByCategory")
            {
                int categoryId = ConvertSqlParameterToInt(sqlParameters?[0]);
                if (categoryId == 404)
                    throw new SqlExceptionThrower().throwSqlException();
                    
                return HashtagRepositoryDataTABLE.AsEnumerable()
                    .Where(row => row.Field<int>("CategoryID") == categoryId)
                    .CopyToDataTable();
            }

            throw new NotImplementedException();
        }

        public T? ExecuteScalar<T>(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            if (storedProcedure == "CreateHashtag")
            {
                string tag = sqlParameters?[0]?.Value?.ToString() ?? "";
                if (string.IsNullOrEmpty(tag))
                    throw new ArgumentException("Tag cannot be empty");
                if (tag == "error")
                    throw new SqlExceptionThrower().throwSqlException();
                return (T)Convert.ChangeType(1, typeof(T));
            }

            throw new NotImplementedException();
        }

        public void OpenConnection()
        {
            throw new NotImplementedException();
        }

        private int ConvertSqlParameterToInt(SqlParameter? param)
        {
            if (param == null)
                throw new ArgumentNullException(nameof(param), "SqlParameter cannot be null.");

            int convertedValue = param.Value?.ToString() == null ? 0 : Convert.ToInt32(param.Value.ToString());
            
            if (convertedValue == 404)
                throw new SqlExceptionThrower().throwSqlException();
            
            return convertedValue;
        }
    }
} 