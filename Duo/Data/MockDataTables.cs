using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Data
{
    public class MockDataTables
    {
        public DataTable CommentRepositoryDataTABLE;
        public DataTable HashtagRepositoryDataTABLE;

        public MockDataTables()
        {
            CommentRepositoryDataTABLE = new DataTable
            {
                Columns =
                {
                    new DataColumn("CommentID", typeof(int)),
                    new DataColumn("CommentText", typeof(string)),
                    new DataColumn("UserID", typeof(int)),
                    new DataColumn("PostID", typeof(int)),
                    new DataColumn("ParentCommentID", typeof(int)),
                    new DataColumn("CreatedDate", typeof(DateTime)),
                    new DataColumn("LikeCount", typeof(int)),
                    new DataColumn("Level", typeof(int)),
                    new DataColumn("Username", typeof(string))
                },
                Rows =
                {
                    { 1, "This is a comment", 1, 1, 1, DateTime.Now, 0, 0, "User1" },
                    { 2, "This is a reply", 2, 1, 1, DateTime.Now, 0, 1, "User2" },
                    { 3, "Another comment", 3, 1, 1, DateTime.Now, 0, 0, "User3" },
                }
            };

            HashtagRepositoryDataTABLE = new DataTable
            {
                Columns =
                {
                    new DataColumn("Id", typeof(int)),
                    new DataColumn("Tag", typeof(string)),
                    new DataColumn("PostID", typeof(int)),
                    new DataColumn("CategoryID", typeof(int))
                },
                Rows =
                {
                    { 1, "test", 1, 1 },
                    { 2, "tag1", 1, 1 },
                    { 3, "tag2", 1, 1 },
                    { 4, "category1", 2, 2 },
                    { 5, "category2", 2, 2 }
                }
            };
        }
    }
}
