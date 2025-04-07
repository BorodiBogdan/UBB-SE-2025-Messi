// <copyright file="IPostRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Duo.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Duo.Models;

    /// <summary>
    /// Interface for managing post-related operations in the repository.
    /// </summary>
    public interface IPostRepository
    {
        /// <summary>
        /// Creates a new post in the repository.
        /// </summary>
        /// <param name="post">The post to create.</param>
        /// <returns>The ID of the newly created post.</returns>
        public int CreatePost(Post post);

        /// <summary>
        /// Deletes a post from the repository.
        /// </summary>
        /// <param name="id">The ID of the post to delete.</param>
        public void DeletePost(int id);

        /// <summary>
        /// Updates an existing post in the repository.
        /// </summary>
        /// <param name="post">The post with updated information.</param>
        public void UpdatePost(Post post);

        /// <summary>
        /// Retrieves a post by its ID.
        /// </summary>
        /// <param name="id">The ID of the post to retrieve.</param>
        /// <returns>The post if found; otherwise, null.</returns>
        public Post? GetPostById(int id);

        /// <summary>
        /// Retrieves posts belonging to a specific category with pagination.
        /// </summary>
        /// <param name="categoryId">The ID of the category.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of posts per page.</param>
        /// <returns>A collection of posts for the specified category.</returns>
        public Collection<Post> GetPostsByCategoryId(int categoryId, int page, int pageSize);

        /// <summary>
        /// Retrieves all post titles from the repository.
        /// </summary>
        /// <returns>A list of all post titles.</returns>
        public List<string> GetAllPostTitles();

        /// <summary>
        /// Searches for posts by title.
        /// </summary>
        /// <param name="title">The title to search for.</param>
        /// <returns>A list of posts matching the title.</returns>
        public List<Post> GetPostsByTitle(string title);

        /// <summary>
        /// Gets the user ID associated with a specific post.
        /// </summary>
        /// <param name="postId">The ID of the post.</param>
        /// <returns>The user ID if found; otherwise, null.</returns>
        public int? GetUserIdByPostId(int postId);

        /// <summary>
        /// Retrieves posts created by a specific user with pagination.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of posts per page.</param>
        /// <returns>A list of posts created by the user.</returns>
        public List<Post> GetPostsByUserId(int userId, int page, int pageSize);

        /// <summary>
        /// Retrieves posts containing specific hashtags with pagination.
        /// </summary>
        /// <param name="hashtags">The list of hashtags to search for.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of posts per page.</param>
        /// <returns>A list of posts containing the specified hashtags.</returns>
        public List<Post> GetPostsByHashtags(List<string> hashtags, int page, int pageSize);

        /// <summary>
        /// Increments the like count for a specific post.
        /// </summary>
        /// <param name="postId">The ID of the post.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public bool IncrementPostLikeCount(int postId);

        /// <summary>
        /// Retrieves posts with pagination.
        /// </summary>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of posts per page.</param>
        /// <returns>A list of paginated posts.</returns>
        public List<Post> GetPaginatedPosts(int page, int pageSize);

        /// <summary>
        /// Gets the total number of posts in the repository.
        /// </summary>
        /// <returns>The total count of posts.</returns>
        public int GetTotalPostCount();

        /// <summary>
        /// Gets the number of posts in a specific category.
        /// </summary>
        /// <param name="categoryId">The ID of the category.</param>
        /// <returns>The count of posts in the category.</returns>
        public int GetPostCountByCategory(int categoryId);

        /// <summary>
        /// Gets the number of posts containing specific hashtags.
        /// </summary>
        /// <param name="hashtags">The list of hashtags to search for.</param>
        /// <returns>The count of posts containing the specified hashtags.</returns>
        public int GetPostCountByHashtags(List<string> hashtags);
    }
}
