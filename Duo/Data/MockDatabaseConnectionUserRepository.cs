using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Duo.Data
{
    public class MockDatabaseConnectionUserRepository : IDatabaseConnection
    {
        private MockDataTables _mockDataTables = new MockDataTables();
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
                int userId = ConvertSqlParameterToInt(sqlParameters?[0]);
                
                if (userId == 40)
                {
                    // Return empty table to simulate user not found
                    return new DataTable();
                }

                var result = _userTable.AsEnumerable()
                    .Where(row => row.Field<int>("userID") == userId)
                    .CopyToDataTable();

                return result;
            }
            else if (storedProcedure == "GetUserByUsername")
            {
                string username = sqlParameters?[0].Value.ToString();
                
                if (username == "NonExistentUser")
                {
                    // Return empty table to simulate user not found
                    return new DataTable();
                }

                var filteredRows = _userTable.AsEnumerable()
                    .Where(row => row.Field<string>("username") == username);
                
                if (filteredRows.Any())
                {
                    return filteredRows.CopyToDataTable();
                }
                else
                {
                    return new DataTable();
                }
            }

            throw new NotImplementedException();
        }

        public T? ExecuteScalar<T>(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            if (storedProcedure == "CreateUser")
            {
                if (sqlParameters?[0].Value.ToString() == "ErrorUser")
                {
                    throw new SqlExceptionThrower().throwSqlException();
                }
                
                return (T)Convert.ChangeType(4, typeof(T)); // Return new user ID
            }

            return default(T);
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