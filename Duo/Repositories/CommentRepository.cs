using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using Duo.Models;
using Duo.Data;
using Duo.Repositories.Interfaces;
using Moq;

namespace Duo.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IDatabaseConnection _dataLink;
        private const int MINIMUM_ALLOWED_ID_NUMBER = 0;

        public CommentRepository(IDatabaseConnection dataLink)
        {
            _dataLink = dataLink ?? throw new ArgumentNullException(nameof(dataLink));
        }

        public Comment GetCommentById(int commentId)
        {
            if (commentId <= MINIMUM_ALLOWED_ID_NUMBER) throw new ArgumentException("Invalid comment ID", nameof(commentId));

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@CommentID", commentId)
            };

            DataTable? dataTable = null;
            try
            {
                dataTable = _dataLink.ExecuteReader("GetCommentByID", parameters);
                if (dataTable.Rows.Count == 0)
                    throw new Exception("Comment not found");

                const int ID_INDEX = 0;
                const int CONTENT_INDEX = 1;
                const int USER_ID_INDEX = 2;
                const int POST_ID_INDEX = 3;
                const int PARENT_COMMENT_ID_INDEX = 4;
                const int CREATED_AT_INDEX = 5;
                const int LIKE_COUNT_INDEX = 6;
                const int LEVEL_INDEX = 7;
                var row = dataTable.Rows[0];
                return new Comment(
                    Convert.ToInt32(row[ID_INDEX]),
                    row[CONTENT_INDEX]?.ToString() ?? string.Empty,
                    Convert.ToInt32(row[USER_ID_INDEX]),
                    Convert.ToInt32(row[POST_ID_INDEX]),
                    row[PARENT_COMMENT_ID_INDEX] == DBNull.Value ? 0 : Convert.ToInt32(row[PARENT_COMMENT_ID_INDEX]),
                    Convert.ToDateTime(row[CREATED_AT_INDEX]),
                    Convert.ToInt32(row[LIKE_COUNT_INDEX]),
                    Convert.ToInt32(row[LEVEL_INDEX])
                );
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                dataTable?.Dispose();
            }
        }

        public List<Comment> GetCommentsByPostId(int postId)
        {
            if (postId <= 0) throw new ArgumentException("Invalid post ID", nameof(postId));

            List<Comment> comments = new List<Comment>();
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PostID", postId)
            };

            DataTable? dataTable = null;
            try
            {
                dataTable = _dataLink.ExecuteReader("GetCommentsByPostID", parameters);

                comments = convertDataTableToCommentList(dataTable);

                return comments;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                dataTable?.Dispose();
            }
        }

        public int CreateComment(Comment comment)
        {
            if (comment == null) throw new ArgumentNullException(nameof(comment));
            if (string.IsNullOrEmpty(comment.Content)) throw new ArgumentException("Content cannot be empty");
            if (comment.UserId <= MINIMUM_ALLOWED_ID_NUMBER) throw new ArgumentException("Invalid user ID");
            if (comment.PostId <= MINIMUM_ALLOWED_ID_NUMBER) throw new ArgumentException("Invalid post ID");

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Content", comment.Content),
                new SqlParameter("@UserID", comment.UserId),
                new SqlParameter("@PostID", comment.PostId),
                new SqlParameter("@ParentCommentID", (object?)comment.ParentCommentId ?? DBNull.Value),
                new SqlParameter("@Level", comment.Level)
            };

            try
            {
                int? result = _dataLink.ExecuteScalar<int>("CreateComment", parameters);

                comment.Id = result.Value;
                return result.Value;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool DeleteComment(int id)
        {
            if (id <= MINIMUM_ALLOWED_ID_NUMBER) throw new ArgumentException("Invalid comment ID", nameof(id));

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@CommentID", id)
            };

            try
            {
                _dataLink.ExecuteNonQuery("DeleteComment", parameters);
                return true;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<Comment> GetRepliesByCommentId(int parentCommentId)
        {
            if (parentCommentId <= MINIMUM_ALLOWED_ID_NUMBER) throw new ArgumentException("Invalid parent comment ID", nameof(parentCommentId));

            List<Comment> comments = new List<Comment>();
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ParentCommentID", parentCommentId)
            };

            DataTable? dataTable = null;
            try
            {
                dataTable = _dataLink.ExecuteReader("GetReplies", parameters);

                comments = convertDataTableToCommentList(dataTable);

                return comments;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                dataTable?.Dispose();
            }
        }

        public bool IncrementLikeCount(int commentId)
        {
            if (commentId <= MINIMUM_ALLOWED_ID_NUMBER) throw new ArgumentException("Invalid comment ID", nameof(commentId));

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@CommentID", commentId)
            };

            try
            {
                _dataLink.ExecuteNonQuery("IncrementLikeCount", parameters);
                return true;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int GetCommentsCountForPost(int postId)
        {
            if (postId <= MINIMUM_ALLOWED_ID_NUMBER) throw new ArgumentException("Invalid post ID", nameof(postId));

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PostID", postId)
            };

            try
            {
                int? result = _dataLink.ExecuteScalar<int>("GetCommentsCountForPost", parameters);

                return result.Value;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private List<Comment> convertDataTableToCommentList(DataTable dataTable)
        {
            List<Comment> comments = new List<Comment>();
            const int ID_INDEX = 0;
            const int CONTENT_INDEX = 1;
            const int USER_ID_INDEX = 2;
            const int POST_ID_INDEX = 3;
            const int PARENT_COMMENT_ID_INDEX = 4;
            const int CREATED_AT_INDEX = 5;
            const int LIKE_COUNT_INDEX = 6;
            const int LEVEL_INDEX = 7;

            foreach (DataRow row in dataTable.Rows)
            {
                int commentId = Convert.ToInt32(row[ID_INDEX]);
                Comment comment = new Comment(
                    commentId,
                    row[CONTENT_INDEX]?.ToString() ?? string.Empty,
                    Convert.ToInt32(row[USER_ID_INDEX]),
                    Convert.ToInt32(row[POST_ID_INDEX]),
                    row[PARENT_COMMENT_ID_INDEX] == DBNull.Value ? null : Convert.ToInt32(row[PARENT_COMMENT_ID_INDEX]),
                    Convert.ToDateTime(row[CREATED_AT_INDEX]),
                    Convert.ToInt32(row[LEVEL_INDEX]),
                    Convert.ToInt32(row[LIKE_COUNT_INDEX])
                );
                comments.Add(comment);
            }
            return comments;
        }
    }
}