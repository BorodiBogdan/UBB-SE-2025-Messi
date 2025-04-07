using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Linq;

namespace Duo.Data
{
    public class MockDatabaseConnectionCommentRepository : IDatabaseConnection
    {
        private MockDataTables _mockDataTables = new MockDataTables();

        public MockDatabaseConnectionCommentRepository() { }

        public void CloseConnection()
        {
            //No implementation needed for mock
        }

        public int ExecuteNonQuery(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {


            if (storedProcedure == "DeleteComment")
            {
                ConvertSqlParameterToInt(sqlParameters?[0]);//if 404 SqlException 
                return 1;
            }
            else
            {
                ConvertSqlParameterToInt(sqlParameters?[0]);//if 404 SqlException 
                return 1;
            }
        }

        public DataTable ExecuteReader(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            var CommentRepositoryDataTABLE = _mockDataTables.CommentRepositoryDataTABLE;

            if (storedProcedure == "GetCommentByID")
            {
                int commentId = ConvertSqlParameterToInt(sqlParameters?[0]);

                if(commentId == 40)
                    //return dataTable.Rows.Count = 0
                    return new DataTable();

                return CommentRepositoryDataTABLE.AsEnumerable()
                    .Where(row => row.Field<int>("CommentID") == commentId)
                    .CopyToDataTable();
            }
            else if (storedProcedure == "GetCommentsByPostID")
            {
                int postId = ConvertSqlParameterToInt(sqlParameters?[0]);

                var result = CommentRepositoryDataTABLE.AsEnumerable()
                    .Where(row => row.Field<int>("PostID") == postId)
                    .CopyToDataTable();

                return result;
            }
            else
            {
                int commentId = ConvertSqlParameterToInt(sqlParameters?[0]);

                return CommentRepositoryDataTABLE.AsEnumerable()
                    .Where(row => row.Field<int>("ParentCommentID") == commentId)
                    .CopyToDataTable();
            }
        }

        public T? ExecuteScalar<T>(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            if (storedProcedure == "CreateComment")
            {
                ConvertSqlParameterToInt(sqlParameters?[1]);//if 404 SqlException
                return (T)Convert.ChangeType(1, typeof(T));
            }
            else
            {
                ConvertSqlParameterToInt(sqlParameters?[0]);

                int postId = ConvertSqlParameterToInt(sqlParameters?[0]);
                

                int count = _mockDataTables.CommentRepositoryDataTABLE.AsEnumerable()
                    .Count(row => row.Field<int>("PostID") == postId);
                
                return (T)Convert.ChangeType(count, typeof(T));
            }
        }

        public void OpenConnection()
        {
            //No implementation needed for mock
        }
        private int ConvertSqlParameterToInt(SqlParameter? param)
        {
            int convertedValue = param.Value?.ToString() == null ? 0 : Convert.ToInt32(param.Value.ToString());
            
            if(convertedValue == 404)
                throw new SqlExceptionThrower().throwSqlException();
            
            return convertedValue;
        }
    }
}
