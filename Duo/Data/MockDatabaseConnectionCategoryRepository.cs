using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Duo.Data
{
    public class MockDatabaseConnectionCategoryRepository : IDatabaseConnection
    {
        private DataTable _categoryTable;

        public MockDatabaseConnectionCategoryRepository()
        {
            _categoryTable = new DataTable();
            _categoryTable.Columns.Add("Id", typeof(int));
            _categoryTable.Columns.Add("Name", typeof(string));
            
            // Add sample categories
            _categoryTable.Rows.Add(1, "Technology");
            _categoryTable.Rows.Add(2, "Science");
            _categoryTable.Rows.Add(3, "Music");
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
            if (storedProcedure == "GetCategories")
            {
                return _categoryTable;
            }
            else if (storedProcedure == "GetCategoryByName")
            {
                string categoryName = sqlParameters?[0].Value.ToString();
                
                if (categoryName == "NonExistentCategory")
                {
                    // Return empty table to simulate category not found
                    return new DataTable();
                }

                var filteredRows = _categoryTable.AsEnumerable()
                    .Where(row => row.Field<string>("Name") == categoryName);
                
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
            throw new NotImplementedException();
        }
    }
} 