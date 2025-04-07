// <copyright file="PostRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Duo.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq;
    using Duo.Data;
    using Duo.Models;
    using Duo.Repositories.Interfaces;
    using Microsoft.Data.SqlClient;

    /// <inheritdoc/>
    public class PostRepository : IPostRepository
    {
        private const int INVALIDID = 0;
        private const int MINPAGENUMBER = 1;
        private const int MINPAGESIZE = 1;
        private const int DEFAULTCOUNT = 0;
        private readonly IDatabaseConnection databaseConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostRepository"/> class.
        /// </summary>
        /// <param name="databaseConnection">The database connection instance used for database operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when databaseConnection is null.</exception>
        public PostRepository(IDatabaseConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
        }

        /// <inheritdoc/>
        public int CreatePost(Post postToCreate)
        {
            if (postToCreate == null)
            {
                throw new ArgumentNullException(nameof(postToCreate), "Post cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(postToCreate.Title) || string.IsNullOrWhiteSpace(postToCreate.Description))
            {
                throw new ArgumentException("Title and Description cannot be empty.");
            }

            if (postToCreate.UserID <= INVALIDID || postToCreate.CategoryID <= INVALIDID)
            {
                throw new ArgumentException("Invalid UserID or CategoryID.");
            }

            SqlParameter[] postParameters = new SqlParameter[]
            {
                new SqlParameter("@Title", postToCreate.Title),
                new SqlParameter("@Description", postToCreate.Description),
                new SqlParameter("@UserID", postToCreate.UserID),
                new SqlParameter("@CategoryID", postToCreate.CategoryID),
                new SqlParameter("@CreatedAt", postToCreate.CreatedAt),
                new SqlParameter("@UpdatedAt", postToCreate.UpdatedAt),
                new SqlParameter("@LikeCount", postToCreate.LikeCount),
            };

            try
            {
                int? postId = this.databaseConnection.ExecuteScalar<int>("CreatePost", postParameters);

                return postId.Value;

            }
            catch (Exception ex)
            {
                throw new Exception($"Create post general error: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void DeletePost(int postId)
        {
            if (postId <= INVALIDID)
            {
                throw new ArgumentException("Invalid post ID.");
            }

            SqlParameter[] deleteParameters = new SqlParameter[]
            {
                new SqlParameter("@Id", postId),
            };

            this.databaseConnection.ExecuteNonQuery("DeletePost", deleteParameters);
        }

        /// <inheritdoc/>
        public void UpdatePost(Post postToUpdate)
        {
            if (postToUpdate == null)
            {
                throw new ArgumentNullException(nameof(postToUpdate), "Post cannot be null.");
            }

            if (postToUpdate.Id <= INVALIDID)
            {
                throw new ArgumentException("Invalid post ID.");
            }

            if (string.IsNullOrWhiteSpace(postToUpdate.Title) || string.IsNullOrWhiteSpace(postToUpdate.Description))
            {
                throw new ArgumentException("Title and Description cannot be empty.");
            }

            SqlParameter[] updateParameters = new SqlParameter[]
            {
                new SqlParameter("@Id", postToUpdate.Id),
                new SqlParameter("@Title", postToUpdate.Title),
                new SqlParameter("@Description", postToUpdate.Description),
                new SqlParameter("@UserID", postToUpdate.UserID),
                new SqlParameter("@CategoryID", postToUpdate.CategoryID),
                new SqlParameter("@UpdatedAt", postToUpdate.UpdatedAt),
                new SqlParameter("@LikeCount", postToUpdate.LikeCount),
            };

            this.databaseConnection.ExecuteNonQuery("UpdatePost", updateParameters);
        }

        /// <inheritdoc/>
        public Post? GetPostById(int postId)
        {
            if (postId <= INVALIDID)
            {
                throw new ArgumentException("Invalid post ID.");
            }

            SqlParameter[] queryParameters = new SqlParameter[]
            {
                new SqlParameter("@Id", postId),
            };

            DataTable postDataTable = this.databaseConnection.ExecuteReader("GetPostById", queryParameters);

            if (postDataTable.Rows.Count > DEFAULTCOUNT)
            {
                DataRow postRow = postDataTable.Rows[0];
                return new Post
                {
                    Id = Convert.ToInt32(postRow["Id"]),
                    Title = Convert.ToString(postRow["Title"]) ?? string.Empty,
                    Description = Convert.ToString(postRow["Description"]) ?? string.Empty,
                    UserID = Convert.ToInt32(postRow["UserID"]),
                    CategoryID = Convert.ToInt32(postRow["CategoryID"]),
                    CreatedAt = Convert.ToDateTime(postRow["CreatedAt"]),
                    UpdatedAt = Convert.ToDateTime(postRow["UpdatedAt"]),
                    LikeCount = Convert.ToInt32(postRow["LikeCount"]),
                };
            }

            return null;
        }

        /// <inheritdoc/>
        public Collection<Post> GetPostsByCategoryId(int categoryId, int pageNumber, int pageSize)
        {
            if (categoryId <= INVALIDID)
            {
                throw new ArgumentException("Invalid category ID.");
            }

            if (pageNumber <= INVALIDID)
            {
                throw new ArgumentException("Page number must be greater than 0.");
            }

            if (pageSize <= INVALIDID)
            {
                throw new ArgumentException("Page size must be greater than 0.");
            }

            int offsetRows = (pageNumber - MINPAGENUMBER) * pageSize;

            SqlParameter[] categoryParameters = new SqlParameter[]
            {
                new SqlParameter("@CategoryID", categoryId),
                new SqlParameter("@PageSize", pageSize),
                new SqlParameter("@Offset", offsetRows),
            };

            DataTable categoryPostsTable = this.databaseConnection.ExecuteReader("GetPostsByCategory", categoryParameters);
            List<Post> categoryPosts = new List<Post>();

            foreach (DataRow postRow in categoryPostsTable.Rows)
            {
                categoryPosts.Add(new Post
                {
                    Id = Convert.ToInt32(postRow["Id"]),
                    Title = Convert.ToString(postRow["Title"]) ?? string.Empty,
                    Description = Convert.ToString(postRow["Description"]) ?? string.Empty,
                    UserID = Convert.ToInt32(postRow["UserID"]),
                    CategoryID = Convert.ToInt32(postRow["CategoryID"]),
                    CreatedAt = Convert.ToDateTime(postRow["CreatedAt"]),
                    UpdatedAt = Convert.ToDateTime(postRow["UpdatedAt"]),
                    LikeCount = Convert.ToInt32(postRow["LikeCount"]),
                });
            }

            return new Collection<Post>(categoryPosts);
        }

        /// <inheritdoc/>
        public List<string> GetAllPostTitles()
        {
            var postTitles = new List<string>();

            try
            {
                DataTable titlesTable = this.databaseConnection.ExecuteReader("GetAllPostTitles", null);

                foreach (DataRow titleRow in titlesTable.Rows)
                {
                    postTitles.Add(titleRow["Title"]?.ToString() ?? string.Empty);
                }

                return postTitles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting post titles: {ex.Message}");
                return postTitles;
            }
        }

        /// <inheritdoc/>
        public List<Post> GetPostsByTitle(string searchTitle)
        {
            SqlParameter[] titleParameters = new SqlParameter[]
            {
                new SqlParameter("@Title", searchTitle),
            };

            try
            {
                DataTable matchingPostsTable = this.databaseConnection.ExecuteReader("GetPostsByTitle", titleParameters);
                List<Post> matchingPosts = new List<Post>();

                foreach (DataRow postRow in matchingPostsTable.Rows)
                {
                    matchingPosts.Add(new Post
                    {
                        Id = Convert.ToInt32(postRow["Id"]),
                        Title = Convert.ToString(postRow["Title"]) ?? string.Empty,
                        Description = Convert.ToString(postRow["Description"]) ?? string.Empty,
                        UserID = Convert.ToInt32(postRow["UserID"]),
                        CategoryID = Convert.ToInt32(postRow["CategoryID"]),
                        CreatedAt = Convert.ToDateTime(postRow["CreatedAt"]),
                        UpdatedAt = Convert.ToDateTime(postRow["UpdatedAt"]),
                        LikeCount = Convert.ToInt32(postRow["LikeCount"]),
                    });
                }

                return matchingPosts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting posts by title: {ex.Message}");
                return new List<Post>();
            }
        }

        /// <inheritdoc/>
        public int? GetUserIdByPostId(int postId)
        {
            SqlParameter[] userIdParameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", postId),
            };

            try
            {
                DataTable userDataTable = this.databaseConnection.ExecuteReader("GetUserIdByPostId", userIdParameters);

                if (userDataTable.Rows.Count > DEFAULTCOUNT)
                {
                    DataRow userRow = userDataTable.Rows[0];
                    return Convert.ToInt32(userRow["UserId"]);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetUserIdByPostId exception: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public List<Post> GetPostsByUserId(int userId, int pageNumber, int pageSize)
        {
            SqlParameter[] userParameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@PageSize", pageSize),
                new SqlParameter("@Offset", pageNumber),
            };

            try
            {
                DataTable userPostsTable = this.databaseConnection.ExecuteReader("GetPostsByUser", userParameters);
                List<Post> userPosts = new List<Post>();

                foreach (DataRow postRow in userPostsTable.Rows)
                {
                    userPosts.Add(new Post
                    {
                        Id = Convert.ToInt32(postRow["Id"]),
                        Title = Convert.ToString(postRow["Title"]) ?? string.Empty,
                        Description = Convert.ToString(postRow["Description"]) ?? string.Empty,
                        UserID = Convert.ToInt32(postRow["UserID"]),
                        CategoryID = Convert.ToInt32(postRow["CategoryID"]),
                        CreatedAt = Convert.ToDateTime(postRow["CreatedAt"]),
                        UpdatedAt = Convert.ToDateTime(postRow["UpdatedAt"]),
                        LikeCount = Convert.ToInt32(postRow["LikeCount"]),
                    });
                }

                return userPosts;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetByUser: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public List<Post> GetPostsByHashtags(List<string> hashtags, int pageNumber, int pageSize)
        {
            if (hashtags == null || hashtags.Count == DEFAULTCOUNT)
            {
                return this.GetPaginatedPosts(pageNumber, pageSize);
            }

            List<string> filteredHashtags = hashtags
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Select(h => h.Trim().ToLowerInvariant())
                .ToList();
            string hashtagsString = string.Join(",", filteredHashtags);
            int offsetRows = (pageNumber - MINPAGENUMBER) * pageSize;

            if (string.IsNullOrWhiteSpace(hashtagsString) || filteredHashtags.Count == DEFAULTCOUNT)
            {
                return this.GetPaginatedPosts(pageNumber, pageSize);
            }

            SqlParameter[] hashtagParameters = new SqlParameter[]
            {
                new SqlParameter("@Hashtags", hashtagsString),
                new SqlParameter("@PageSize", pageSize),
                new SqlParameter("@Offset", offsetRows),
            };

            try
            {
                DataTable hashtagPostsTable = this.databaseConnection.ExecuteReader("GetByHashtags", hashtagParameters);
                List<Post> hashtagPosts = new List<Post>();

                foreach (DataRow postRow in hashtagPostsTable.Rows)
                {
                    hashtagPosts.Add(new Post
                    {
                        Id = Convert.ToInt32(postRow["Id"]),
                        Title = Convert.ToString(postRow["Title"]) ?? string.Empty,
                        Description = Convert.ToString(postRow["Description"]) ?? string.Empty,
                        UserID = Convert.ToInt32(postRow["UserID"]),
                        CategoryID = Convert.ToInt32(postRow["CategoryID"]),
                        CreatedAt = Convert.ToDateTime(postRow["CreatedAt"]),
                        UpdatedAt = Convert.ToDateTime(postRow["UpdatedAt"]),
                        LikeCount = Convert.ToInt32(postRow["LikeCount"]),
                    });
                }

                return hashtagPosts;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetByHashtags: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public bool IncrementPostLikeCount(int postId)
        {
            if (postId <= INVALIDID)
            {
                throw new ArgumentException("Invalid post ID.");
            }

            SqlParameter[] likeParameters = new SqlParameter[]
            {
                new SqlParameter("@PostID", postId),
            };

            try
            {
                this.databaseConnection.ExecuteNonQuery("IncrementPostLikeCount", likeParameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <inheritdoc/>
        public List<Post> GetPaginatedPosts(int pageNumber, int pageSize)
        {
            if (pageNumber <= INVALIDID)
            {
                throw new ArgumentException("Page number must be greater than 0.");
            }

            if (pageSize <= INVALIDID)
            {
                throw new ArgumentException("Page size must be greater than 0.");
            }

            int offsetRows = (pageNumber - MINPAGENUMBER) * pageSize;

            SqlParameter[] paginationParameters = new SqlParameter[]
            {
                new SqlParameter("@PageSize", pageSize),
                new SqlParameter("@Offset", offsetRows),
            };

            try
            {
                DataTable paginatedPostsTable = this.databaseConnection.ExecuteReader("GetPaginatedPosts", paginationParameters);
                List<Post> paginatedPosts = new List<Post>();

                foreach (DataRow postRow in paginatedPostsTable.Rows)
                {
                    paginatedPosts.Add(new Post
                    {
                        Id = Convert.ToInt32(postRow["Id"]),
                        Title = Convert.ToString(postRow["Title"]) ?? string.Empty,
                        Description = Convert.ToString(postRow["Description"]) ?? string.Empty,
                        UserID = Convert.ToInt32(postRow["UserID"]),
                        CategoryID = Convert.ToInt32(postRow["CategoryID"]),
                        CreatedAt = Convert.ToDateTime(postRow["CreatedAt"]),
                        UpdatedAt = Convert.ToDateTime(postRow["UpdatedAt"]),
                        LikeCount = Convert.ToInt32(postRow["LikeCount"]),
                    });
                }

                return paginatedPosts;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetPaginatedPosts: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public int GetTotalPostCount()
        {
            try
            {
                object totalCountResult = this.databaseConnection.ExecuteScalar<int>("GetTotalPostCount");
                return totalCountResult != null ? Convert.ToInt32(totalCountResult) : DEFAULTCOUNT;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetTotalPostCount: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public int GetPostCountByCategory(int categoryId)
        {
            if (categoryId <= INVALIDID)
            {
                throw new ArgumentException("Invalid category ID.");
            }

            SqlParameter[] categoryParameters = new SqlParameter[]
            {
                new SqlParameter("@CategoryID", categoryId),
            };

            try
            {
                object categoryCountResult = this.databaseConnection.ExecuteScalar<int>("GetPostCountByCategory", categoryParameters);
                return categoryCountResult != null ? Convert.ToInt32(categoryCountResult) : DEFAULTCOUNT;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetPostCountByCategory: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public int GetPostCountByHashtags(List<string> hashtags)
        {
            if (hashtags == null || hashtags.Count == DEFAULTCOUNT)
            {
                return this.GetTotalPostCount();
            }

            List<string> filteredHashtags = hashtags
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Select(h => h.Trim().ToLowerInvariant())
                .ToList();

            if (filteredHashtags.Count == DEFAULTCOUNT)
            {
                return this.GetTotalPostCount();
            }

            string hashtagsString = string.Join(",", filteredHashtags);

            SqlParameter[] hashtagParameters = new SqlParameter[]
            {
                new SqlParameter("@Hashtags", hashtagsString),
            };

            try
            {
                object hashtagCountResult = this.databaseConnection.ExecuteScalar<int>("GetPostCountByHashtags", hashtagParameters);
                return hashtagCountResult != null ? Convert.ToInt32(hashtagCountResult) : DEFAULTCOUNT;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetPostCountByHashtags: {ex.Message}");
            }
        }
    }
}