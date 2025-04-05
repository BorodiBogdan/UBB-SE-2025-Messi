using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using Duo.Models;
using Duo.Data;
using Duo.Repositories.Interfaces;

namespace Duo.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly IDatabaseConnection dataLink;

        public PostRepository(IDatabaseConnection dataLink)
        {
            this.dataLink = dataLink ?? throw new ArgumentNullException(nameof(dataLink));
        }

        public int CreatePost(Post post)
        {
            if (post == null)
            {
                throw new ArgumentNullException(nameof(post), "Post cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(post.Title) || string.IsNullOrWhiteSpace(post.Description))
            {
                throw new ArgumentException("Title and Description cannot be empty.");
            }

            if (post.UserID <= 0 || post.CategoryID <= 0)
            {
                throw new ArgumentException("Invalid UserID or CategoryID.");
            }

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Title", post.Title),
                new SqlParameter("@Description", post.Description),
                new SqlParameter("@UserID", post.UserID),
                new SqlParameter("@CategoryID", post.CategoryID),
                new SqlParameter("@CreatedAt", post.CreatedAt),
                new SqlParameter("@UpdatedAt", post.UpdatedAt),
                new SqlParameter("@LikeCount", post.LikeCount)
            };

            try
            {
                int? result = dataLink.ExecuteScalar<int>("CreatePost", parameters);

                return result.Value;

            }
            catch (Exception ex)
            {
                throw new Exception($"Create post general error: {ex.Message}");
            }
        }

        public void DeletePost(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid post ID.");
            }

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id)
            };

            dataLink.ExecuteNonQuery("DeletePost", parameters);
        }

        public void UpdatePost(Post post)
        {
            if (post == null)
            {
                throw new ArgumentNullException(nameof(post), "Post cannot be null.");
            }

            if (post.Id <= 0)
            {
                throw new ArgumentException("Invalid post ID.");
            }

            if (string.IsNullOrWhiteSpace(post.Title) || string.IsNullOrWhiteSpace(post.Description))
            {
                throw new ArgumentException("Title and Description cannot be empty.");
            }

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", post.Id),
                new SqlParameter("@Title", post.Title),
                new SqlParameter("@Description", post.Description),
                new SqlParameter("@UserID", post.UserID),
                new SqlParameter("@CategoryID", post.CategoryID),
                new SqlParameter("@UpdatedAt", post.UpdatedAt),
                new SqlParameter("@LikeCount", post.LikeCount)
            };

            dataLink.ExecuteNonQuery("UpdatePost", parameters);
        }

        public Post? GetPostById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid post ID.");
            }

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id)
            };

            DataTable dataTable = dataLink.ExecuteReader("GetPostById", parameters);

            if (dataTable.Rows.Count > 0)
            {
                DataRow row = dataTable.Rows[0];
                return new Post
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Title = Convert.ToString(row["Title"]) ?? string.Empty,
                    Description = Convert.ToString(row["Description"]) ?? string.Empty,
                    UserID = Convert.ToInt32(row["UserID"]),
                    CategoryID = Convert.ToInt32(row["CategoryID"]),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                    UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]),
                    LikeCount = Convert.ToInt32(row["LikeCount"])
                };
            }

            return null;
        }

        public Collection<Post> GetPostsByCategoryId(int categoryId, int page, int pageSize)
        {
            if (categoryId <= 0)
            {
                throw new ArgumentException("Invalid category ID.");
            }

            if (page <= 0)
            {
                throw new ArgumentException("Page number must be greater than 0.");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentException("Page size must be greater than 0.");
            }

            int offset = (page - 1) * pageSize;

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@CategoryID", categoryId),
                new SqlParameter("@PageSize", pageSize),
                new SqlParameter("@Offset", offset)
            };

            DataTable dataTable = dataLink.ExecuteReader("GetPostsByCategory", parameters);
            List<Post> posts = new List<Post>();

            foreach (DataRow row in dataTable.Rows)
            {
                posts.Add(new Post
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Title = Convert.ToString(row["Title"]) ?? string.Empty,
                    Description = Convert.ToString(row["Description"]) ?? string.Empty,
                    UserID = Convert.ToInt32(row["UserID"]),
                    CategoryID = Convert.ToInt32(row["CategoryID"]),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                    UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]),
                    LikeCount = Convert.ToInt32(row["LikeCount"])
                });
            }

            return new Collection<Post>(posts);
        }

        public List<string> GetAllPostTitles()
        {
            var titles = new List<string>();

            try
            {
                DataTable dataTable = dataLink.ExecuteReader("GetAllPostTitles", null);

                foreach (DataRow row in dataTable.Rows)
                {
                    titles.Add(row["Title"].ToString());
                }

                return titles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting post titles: {ex.Message}");
                return titles;
            }
        }

        public List<Post> GetPostsByTitle(string title)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Title", title)
            };

            try
            {
                DataTable dataTable = dataLink.ExecuteReader("GetPostsByTitle", parameters);
                List<Post> posts = new List<Post>();

                foreach (DataRow row in dataTable.Rows)
                {
                    posts.Add(new Post
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Title = Convert.ToString(row["Title"]) ?? string.Empty,
                        Description = Convert.ToString(row["Description"]) ?? string.Empty,
                        UserID = Convert.ToInt32(row["UserID"]),
                        CategoryID = Convert.ToInt32(row["CategoryID"]),
                        CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                        UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]),
                        LikeCount = Convert.ToInt32(row["LikeCount"])
                    });
                }

                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting posts by title: {ex.Message}");
                return new List<Post>();
            }
        }

        public int? GetUserIdByPostId(int postId)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PostId", postId)
            };

            try
            {
                DataTable dataTable = dataLink.ExecuteReader("GetUserIdByPostId", parameters);

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    return Convert.ToInt32(row["UserId"]);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetUserIdByPostId exception: {ex.Message}");
            }
        }

        public List<Post> GetPostsByUserId(int userId, int page, int pageSize)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@PageSize", pageSize),
                new SqlParameter("@Offset", page)
            };

            try
            {
                DataTable dataTable = dataLink.ExecuteReader("GetPostsByUser", parameters);
                List<Post> posts = new List<Post>();

                foreach (DataRow row in dataTable.Rows)
                {
                    posts.Add(new Post
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Title = Convert.ToString(row["Title"]) ?? string.Empty,
                        Description = Convert.ToString(row["Description"]) ?? string.Empty,
                        UserID = Convert.ToInt32(row["UserID"]),
                        CategoryID = Convert.ToInt32(row["CategoryID"]),
                        CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                        UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]),
                        LikeCount = Convert.ToInt32(row["LikeCount"])
                    });
                }

                return posts;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetByUser: {ex.Message}");
            }
        }

        public List<Post> GetPostsByHashtags(List<string> hashtags, int page, int pageSize)
        {
            if (hashtags == null || hashtags.Count == 0)
            {
                return GetPaginatedPosts(page, pageSize);
            }

            hashtags = hashtags
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Select(h => h.Trim().ToLowerInvariant())
                .ToList();
            
            string hashtagsString = string.Join(",", hashtags);
            int offset = (page - 1) * pageSize;

            if (string.IsNullOrWhiteSpace(hashtagsString) || hashtags.Count == 0)
            {
                
                return GetPaginatedPosts(page, pageSize);
            }

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Hashtags", hashtagsString),
                new SqlParameter("@PageSize", pageSize),
                new SqlParameter("@Offset", offset)
            };

            try
            {

                DataTable dataTable = dataLink.ExecuteReader("GetByHashtags", parameters);
                List<Post> posts = new List<Post>();

                foreach (DataRow row in dataTable.Rows)
                {
                    posts.Add(new Post
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Title = Convert.ToString(row["Title"]) ?? string.Empty,
                        Description = Convert.ToString(row["Description"]) ?? string.Empty,
                        UserID = Convert.ToInt32(row["UserID"]),
                        CategoryID = Convert.ToInt32(row["CategoryID"]),
                        CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                        UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]),
                        LikeCount = Convert.ToInt32(row["LikeCount"])
                    });
                }

                return posts;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetByHashtags: {ex.Message}", ex);
            }
        }

        public bool IncrementPostLikeCount(int postId)
        {
            if (postId <= 0)
            {
                throw new ArgumentException("Invalid post ID.");
            }

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PostID", postId)
            };

            try
            {
                dataLink.ExecuteNonQuery("IncrementPostLikeCount", parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<Post> GetPaginatedPosts(int page, int pageSize)
        {
            if (page <= 0)
            {
                throw new ArgumentException("Page number must be greater than 0.");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentException("Page size must be greater than 0.");
            }

            int offset = (page - 1) * pageSize;

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PageSize", pageSize),
                new SqlParameter("@Offset", offset)
            };

            try
            {
                DataTable dataTable = dataLink.ExecuteReader("GetPaginatedPosts", parameters);
                List<Post> posts = new List<Post>();

                foreach (DataRow row in dataTable.Rows)
                {
                    posts.Add(new Post
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Title = Convert.ToString(row["Title"]) ?? string.Empty,
                        Description = Convert.ToString(row["Description"]) ?? string.Empty,
                        UserID = Convert.ToInt32(row["UserID"]),
                        CategoryID = Convert.ToInt32(row["CategoryID"]),
                        CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                        UpdatedAt = Convert.ToDateTime(row["UpdatedAt"]),
                        LikeCount = Convert.ToInt32(row["LikeCount"])
                    });
                }

                return posts;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetPaginatedPosts: {ex.Message}");
            }
        }

        public int GetTotalPostCount()
        {
            try
            {
                object result = dataLink.ExecuteScalar<int>("GetTotalPostCount");
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetTotalPostCount: {ex.Message}");
            }
        }

        public int GetPostCountByCategory(int categoryId)
        {
            if (categoryId <= 0)
            {
                throw new ArgumentException("Invalid category ID.");
            }

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@CategoryID", categoryId)
            };

            try
            {
                object result = dataLink.ExecuteScalar<int>("GetPostCountByCategory", parameters);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetPostCountByCategory: {ex.Message}");
            }
        }

        public int GetPostCountByHashtags(List<string> hashtags)
        {
            if (hashtags == null || hashtags.Count == 0)
            {
                return GetTotalPostCount();
            }

            hashtags = hashtags
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Select(h => h.Trim().ToLowerInvariant())
                .ToList();
            
            string hashtagsString = string.Join(",", hashtags);

            if (string.IsNullOrWhiteSpace(hashtagsString) || hashtags.Count == 0)
            {
                
                return GetTotalPostCount();
            }

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Hashtags", hashtagsString)
            };

            try
            {
                object result = dataLink.ExecuteScalar<int>("GetPostCountByHashtags", parameters);
                int count = result != null ? Convert.ToInt32(result) : 0;
                
                return count;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetPostCountByHashtags: {ex.Message}", ex);
            }
        }
    }
}