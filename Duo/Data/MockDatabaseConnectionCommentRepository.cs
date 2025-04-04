using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Data
{
    public class MockDatabaseConnectionCommentRepository : IDatabaseConnection
    {
        private MockDataTables _mockDataTables = new MockDataTables();

        public MockDatabaseConnectionCommentRepository() { }

        public void CloseConnection()
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            if (storedProcedure == "AddComment")
                return 1;
            else if (storedProcedure == "DeleteComment")
                return 1; 
            else if (storedProcedure == "IncrementLikeCount")
                return 1;
            
            throw new NotImplementedException();
        }

        public DataTable ExecuteReader(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            var CommentRepositoryDataTABLE = _mockDataTables.CommentRepositoryDataTABLE;

            if (storedProcedure == "GetCommentByID")
            {
                int commentId = sqlParameters?[0].Value?.ToString() == null ? 0 : Convert.ToInt32(sqlParameters[0].Value.ToString());

                return CommentRepositoryDataTABLE.AsEnumerable()
                    .Where(row => row.Field<int>("CommentID") == commentId)
                    .CopyToDataTable();
            }
            else if (storedProcedure == "GetCommentsByPostID")
            {
                int postId = sqlParameters?[0].Value?.ToString() == null ? 0 : Convert.ToInt32(sqlParameters[0].Value.ToString());

                return CommentRepositoryDataTABLE.AsEnumerable()
                    .Where(row => row.Field<int>("PostID") == postId)
                    .CopyToDataTable();
            }
            else if (storedProcedure == "GetReplies")
            {
                int commentId = sqlParameters?[0].Value?.ToString() == null ? 0 : Convert.ToInt32(sqlParameters[0].Value.ToString());

                return CommentRepositoryDataTABLE.AsEnumerable()
                    .Where(row => row.Field<int>("ParentCommentID") == commentId)
                    .CopyToDataTable();
            }

            throw new NotImplementedException();
        }

        public T? ExecuteScalar<T>(string storedProcedure, SqlParameter[]? sqlParameters = null)
        {
            if (storedProcedure == "CreateComment")
            {
                return (T)Convert.ChangeType(1, typeof(T));
            }
            else if (storedProcedure == "GetCommentsCountForPost")
            {
                if (sqlParameters == null || sqlParameters.Length == 0 || sqlParameters[0].Value == null)
                {
                    throw new ArgumentException("Invalid SQL parameters for GetCommentsCountForPost.");
                }

                int postId = Convert.ToInt32(sqlParameters[0].Value);
                int count = _mockDataTables.CommentRepositoryDataTABLE.AsEnumerable()
                    .Count(row => row.Field<int>("PostID") == postId);

                return (T)Convert.ChangeType(count, typeof(T));
            }

            throw new NotImplementedException();
        }

        public void OpenConnection()
        {
            throw new NotImplementedException();
        }
    }
}
