using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Duo.Data
{
    public class MockDatabaseConnectionUserRepository : IDatabaseConnection
    {
        private DataTable _userTable;

        public MockDatabaseConnectionUserRepository()
        {
            _userTable = new DataTable();
            _userTable.Columns.Add("userID", typeof(int));
            _userTable.Columns.Add("username", typeof(string));
            
            // Add sample users
            _userTable.Rows.Add(1, "User1");
            _userTable.Rows.Add(2, "User2");
            _userTable.Rows.Add(3, "User3");
        }

        public void CloseConnection()
        {
            // Not implemented for mock
        }

        public void OpenConnection()
        {
            // Not implemented for mock
        }

        public int ExecuteNonQuery(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            throw new NotImplementedException();
        }

        public DataTable ExecuteReader(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            if (storedProcedure == "GetUserByID")
            {
                if (sqlParameters == null || sqlParameters.Length == 0)
                    throw new ArgumentException("UserID parameter is required");

                int userId = ConvertSqlParameterToInt(sqlParameters[0]);
                
                // Special case for 404 - always throw exception
                if (userId == 404 || userId == 505)
                    throw new Exception("Database error: SQL error occurred");
                
                if (userId == 0)
                    throw new ArgumentException("Invalid user ID.");
                
                if (userId == 40)
                    return new DataTable(); // Empty table for non-existent user

                var filteredRows = _userTable.AsEnumerable()
                    .Where(row => row.Field<int>("userID") == userId);
                    
                if (filteredRows.Any())
                {
                    return filteredRows.CopyToDataTable();
                }
                
                return new DataTable();
            }
            else if (storedProcedure == "GetUserByUsername")
            {
                if (sqlParameters == null || sqlParameters.Length == 0)
                    throw new ArgumentException("Username parameter is required");

                string username = sqlParameters[0].Value?.ToString() ?? "";
                
                if (string.IsNullOrEmpty(username))
                    throw new ArgumentException("Username cannot be empty");
                
                if (username == "NonExistentUser" || username == "NewUser")
                    return new DataTable(); // Empty table for non-existent user
                
                if (username == "SQLErrorUser" || username == "SQLExceptionUser")
                    throw new Exception("Database error: SQL error occurred");
                
                if (username == "ExistingUser")
                    return _userTable.AsEnumerable().Where(r => r.Field<int>("userID") == 1).CopyToDataTable();

                var filteredRows = _userTable.AsEnumerable()
                    .Where(row => row.Field<string>("username") == username);
                
                if (filteredRows.Any())
                {
                    return filteredRows.CopyToDataTable();
                }
                
                return new DataTable();
            }

            throw new NotImplementedException();
        }

        public T? ExecuteScalar<T>(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            if (storedProcedure == "CreateUser")
            {
                if (sqlParameters == null || sqlParameters.Length == 0)
                    return (T)Convert.ChangeType(4, typeof(T)); // Default return for CreateUser
                
                string username = sqlParameters[0].Value?.ToString() ?? "";
                
                if (username == "SQLErrorCreateUser" || username == "SqlExceptionUser" || username == "ErrorUser")
                    throw new Exception("Database error: SQL error occurred");
                
                return (T)Convert.ChangeType(4, typeof(T)); // Return new user ID
            }

            return default(T);
        }

        private int ConvertSqlParameterToInt(SqlParameter? param)
        {
            if (param == null)
                throw new ArgumentNullException(nameof(param), "SqlParameter cannot be null.");

            return param.Value == null ? 0 : Convert.ToInt32(param.Value);
        }
    }
} 