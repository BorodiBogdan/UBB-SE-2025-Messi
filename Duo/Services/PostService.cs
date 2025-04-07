// <copyright file="PostService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Duo.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using Duo.Models;
    using Duo.Repositories;
    using Duo.Repositories.Interfaces;
    using Duo.Services;
    using Duo.Services.Interfaces;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Service class for managing post-related operations.
    /// </summary>
    public class PostService : IPostService
    {

        // Constants for validation
        private const int INVALIDID = 0;
        private const int MINPAGENUMBER = 1;
        private const int MINPAGESIZE = 1;
        private const int DEFAULTCOUNT = 0;

        private readonly IPostRepository postRepository;
        private readonly IHashtagRepository hashtagRepository;
        private readonly IUserService userService;
        private readonly ISearchService searchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostService"/> class.
        /// </summary>
        /// <param name="postRepository">The post repository instance.</param>
        /// <param name="hashtagRepository">The hashtag repository instance.</param>
        /// <param name="userService">The user service instance.</param>
        /// <param name="searchService">The search service instance.</param>
        public PostService(IPostRepository postRepository, IHashtagRepository hashtagRepository, IUserService userService, ISearchService searchService)
        {
            this.postRepository = postRepository;
            this.hashtagRepository = hashtagRepository;
            this.userService = userService;
            this.searchService = searchService;
        }

        /// <inheritdoc/>
        public int CreatePost(Post newPost)
        {
            if (string.IsNullOrWhiteSpace(newPost.Title) || string.IsNullOrWhiteSpace(newPost.Description))
            {
                throw new ArgumentException("Title and Description cannot be empty.");
            }

            try
            {
                return this.postRepository.CreatePost(newPost);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating post: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void DeletePost(int postId)
        {
            if (postId <= INVALIDID)
            {
                throw new ArgumentException("Invalid Post ID.");
            }

            try
            {
                this.postRepository.DeletePost(postId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting post with ID {postId}: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void UpdatePost(Post postToUpdate)
        {
            if (postToUpdate.Id <= INVALIDID)
            {
                throw new ArgumentException("Invalid Post ID.");
            }

            try
            {
                postToUpdate.UpdatedAt = DateTime.UtcNow;
                this.postRepository.UpdatePost(postToUpdate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating post with ID {postToUpdate.Id}: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public Post? GetPostById(int postId)
        {
            if (postId <= INVALIDID)
            {
                throw new ArgumentException("Invalid Post ID.");
            }

            try
            {
                return this.postRepository.GetPostById(postId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving post with ID {postId}: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public Collection<Post> GetPostsByCategory(int categoryId, int pageNumber, int pageSize)
        {
            if (categoryId <= INVALIDID || pageNumber < MINPAGENUMBER || pageSize < MINPAGESIZE)
            {
                throw new ArgumentException("Invalid pagination parameters.");
            }

            try
            {
                return this.postRepository.GetPostsByCategoryId(categoryId, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving posts for category {categoryId}: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public List<Post> GetPaginatedPosts(int pageNumber, int pageSize)
        {
            if (pageNumber < MINPAGENUMBER || pageSize < MINPAGESIZE)
            {
                throw new ArgumentException("Invalid pagination parameters.");
            }

            try
            {
                return this.postRepository.GetPaginatedPosts(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving paginated posts: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public int GetTotalPostCount()
        {
            try
            {
                return this.postRepository.GetTotalPostCount();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving total post count: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public int GetPostCountByCategoryId(int categoryId)
        {
            if (categoryId <= INVALIDID)
            {
                throw new ArgumentException("Invalid Category ID.");
            }

            try
            {
                return this.postRepository.GetPostCountByCategory(categoryId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving post count for category {categoryId}: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public int GetPostCountByHashtags(List<string> hashtagList)
        {
            if (hashtagList == null || hashtagList.Count == DEFAULTCOUNT)
            {
                return this.GetTotalPostCount();
            }

            List<string> filteredHashtags = hashtagList.Where(h => !string.IsNullOrWhiteSpace(h)).ToList();
            if (filteredHashtags.Count == DEFAULTCOUNT)
            {
                return this.GetTotalPostCount();
            }

            try
            {
                return this.postRepository.GetPostCountByHashtags(filteredHashtags);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving post count for hashtags: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public List<Hashtag> GetAllHashtags()
        {
            try
            {
                return this.hashtagRepository.GetAllHashtags();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all hashtags: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public List<Hashtag> GetHashtagsByCategory(int categoryId)
        {
            if (categoryId <= INVALIDID)
            {
                throw new ArgumentException("Invalid Category ID.");
            }

            try
            {
                return this.hashtagRepository.GetHashtagsByCategory(categoryId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving hashtags for category {categoryId}: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public List<Post> GetPostsByHashtags(List<string> hashtagList, int pageNumber, int pageSize)
        {
            if (pageNumber < MINPAGENUMBER || pageSize < MINPAGESIZE)
            {
                throw new ArgumentException("Invalid pagination parameters.");
            }

            if (hashtagList == null || hashtagList.Count == DEFAULTCOUNT)
            {
                return this.GetPaginatedPosts(pageNumber, pageSize);
            }

            List<string> filteredHashtags = hashtagList.Where(h => !string.IsNullOrWhiteSpace(h)).ToList();
            if (filteredHashtags.Count == DEFAULTCOUNT)
            {
                return this.GetPaginatedPosts(pageNumber, pageSize);
            }

            try
            {
                return this.postRepository.GetPostsByHashtags(filteredHashtags, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving posts for hashtags: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public bool ValidatePostOwnership(int authorUserId, int targetPostId)
        {
            int? postOwnerId = this.postRepository.GetUserIdByPostId(targetPostId);
            return authorUserId == postOwnerId;
        }

        /// <inheritdoc/>
        public List<Hashtag> GetHashtagsByPostId(int postId)
        {
            if (postId <= INVALIDID) throw new ArgumentException("Invalid Post ID.");

            try
            {
                return this.hashtagRepository.GetHashtagsByPostId(postId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving hashtags for post with ID {postId}: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public bool LikePost(int postId)
        {
            if (postId <= INVALIDID)
            {
                throw new ArgumentException("Invalid Post ID.");
            }

            try
            {
                var targetPost = this.postRepository.GetPostById(postId);
                if (targetPost == null)
                {
                    throw new Exception("Post not found");
                }

                targetPost.LikeCount++;

                this.postRepository.UpdatePost(targetPost);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error liking post with ID {postId}: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public bool AddHashtagToPost(int postId, string hashtagName, int userId)
        {
            if (postId <= INVALIDID)
            {
                throw new ArgumentException("Invalid Post ID.");
            }

            if (string.IsNullOrWhiteSpace(hashtagName))
            {
                throw new ArgumentException("Tag name cannot be empty.");
            }

            if (userId <= INVALIDID)
            {
                throw new ArgumentException("Invalid User ID.");
            }

            try
            {
                var targetPost = this.postRepository.GetPostById(postId);
                if (targetPost == null)
                {
                    throw new Exception($"Post with ID {postId} not found");
                }

                if (this.userService.GetCurrentUser().UserId != userId)
                {
                    throw new Exception("User does not have permission to add hashtags to this post.");
                }

                Hashtag? existingHashtag = null;
                existingHashtag = this.hashtagRepository.GetHashtagByText(hashtagName);

                Hashtag hashtag = this.hashtagRepository.CreateHashtag(hashtagName);

                bool addResult = this.hashtagRepository.AddHashtagToPost(postId, hashtag.Id);
                return addResult;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding hashtag to post with ID {postId}: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public bool RemoveHashtagFromPost(int postId, int hashtagId, int userId)
        {
            if (postId <= INVALIDID)
            {
                throw new ArgumentException("Invalid Post ID.");
            }

            if (hashtagId <= INVALIDID)
            {
                throw new ArgumentException("Invalid Hashtag ID.");
            }

            if (userId <= INVALIDID)
            {
                throw new ArgumentException("Invalid User ID.");
            }

            try
            {
                if (this.userService.GetCurrentUser().UserId != userId)
                {
                    throw new Exception("User does not have permission to remove hashtags from this post.");
                }

                return this.hashtagRepository.RemoveHashtagFromPost(postId, hashtagId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing hashtag from post with ID {postId}: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public int CreatePostWithHashtags(Post newPost, List<string> hashtagList, int authorId)
        {
            if (string.IsNullOrWhiteSpace(newPost.Title) || string.IsNullOrWhiteSpace(newPost.Description))
            {
                throw new ArgumentException("Title and Description cannot be empty.");
            }

            try
            {
                int createdPostId = this.postRepository.CreatePost(newPost);

                if (createdPostId <= INVALIDID)
                {
                    throw new Exception("Failed to create post: Invalid post ID returned from database");
                }

                try
                {
                    var createdPost = this.postRepository.GetPostById(createdPostId);
                }
                catch (Exception ex)
                {
                }

                if (hashtagList != null && hashtagList.Count > DEFAULTCOUNT)
                {
                    foreach (var hashtagName in hashtagList)
                    {
                        try
                        {
                            Hashtag? existingHashtag = this.hashtagRepository.GetHashtagByText(hashtagName);
                            Hashtag hashtag = this.hashtagRepository.CreateHashtag(hashtagName);
                            bool addSuccess = this.hashtagRepository.AddHashtagToPost(createdPostId, hashtag.Id);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                return createdPostId;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                }

                throw new Exception($"Error creating post with hashtags: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets hashtags based on an optional category ID.
        /// </summary>
        /// <param name="categoryId">The optional category ID to filter hashtags.</param>
        /// <returns>A list of hashtags, either all hashtags or filtered by category.</returns>
        public List<Hashtag> GetHashtags(int? categoryId)
        {
            if (categoryId == null || categoryId <= INVALIDID)
            {
                return this.GetAllHashtags();
            }

            return this.GetHashtagsByCategory(categoryId.Value);
        }

        /// <summary>
        /// Gets filtered and formatted posts based on various criteria.
        /// </summary>
        /// <param name="categoryId">The optional category ID to filter posts.</param>
        /// <param name="selectedHashtags">The list of selected hashtags to filter posts.</param>
        /// <param name="filterText">The text to filter posts by title.</param>
        /// <param name="currentPage">The current page number.</param>
        /// <param name="itemsPerPage">The number of items per page.</param>
        /// <returns>A tuple containing the filtered posts and total count.</returns>
        /// <exception cref="ArgumentException">Thrown when pagination parameters are invalid.</exception>
        /// <exception cref="Exception">Thrown when an error occurs during post retrieval or formatting.</exception>
        public (List<Post> Posts, int TotalCount) GetFilteredAndFormattedPosts(
            int? categoryId,
            List<string> selectedHashtags,
            string filterText,
            int currentPage,
            int itemsPerPage)
        {
            if (currentPage < MINPAGENUMBER || itemsPerPage < MINPAGESIZE)
            {
                throw new ArgumentException("Invalid pagination parameters.");
            }

            try
            {
                IEnumerable<Post> filteredPosts;
                int totalCount;

                if (selectedHashtags.Count > DEFAULTCOUNT && !selectedHashtags.Contains("All"))
                {
                    filteredPosts = this.GetPostsByHashtags(selectedHashtags, currentPage, itemsPerPage);
                    totalCount = this.GetPostCountByHashtags(selectedHashtags);
                }
                else if (categoryId.HasValue && categoryId.Value > INVALIDID)
                {
                    if (!string.IsNullOrEmpty(filterText))
                    {
                        filteredPosts = this.GetPostsByCategory(categoryId.Value, MINPAGENUMBER, int.MaxValue);
                    }
                    else
                    {
                        filteredPosts = this.GetPostsByCategory(categoryId.Value, currentPage, itemsPerPage);
                    }

                    totalCount = this.GetPostCountByCategoryId(categoryId.Value);
                }
                else
                {
                    if (!string.IsNullOrEmpty(filterText))
                    {
                        filteredPosts = this.GetPaginatedPosts(MINPAGENUMBER, int.MaxValue);
                    }
                    else
                    {
                        filteredPosts = this.GetPaginatedPosts(currentPage, itemsPerPage);
                    }

                    totalCount = this.GetTotalPostCount();
                }

                if (!string.IsNullOrEmpty(filterText))
                {
                    var searchResults = new List<Post>();
                    foreach (var post in filteredPosts)
                    {
                        if (this.searchService.FindFuzzySearchMatches(filterText, new[] { post.Title }).Any())
                        {
                            searchResults.Add(post);
                        }
                    }

                    totalCount = searchResults.Count;
                    filteredPosts = searchResults
                        .Skip((currentPage - MINPAGENUMBER) * itemsPerPage)
                        .Take(itemsPerPage);
                }

                var resultPosts = new List<Post>();
                foreach (var post in filteredPosts)
                {
                    if (string.IsNullOrEmpty(post.Username))
                    {
                        var postAuthor = this.userService.GetUserById(post.UserID);
                        post.Username = postAuthor?.Username ?? "Unknown User";
                    }

                    DateTime localCreatedAt = Helpers.DateTimeHelper.ConvertUtcToLocal(post.CreatedAt);
                    post.Date = Helpers.DateTimeHelper.GetRelativeTime(localCreatedAt);

                    post.Hashtags.Clear();
                    try
                    {
                        var postHashtags = this.GetHashtagsByPostId(post.Id);
                        foreach (var hashtag in postHashtags)
                        {
                            post.Hashtags.Add(hashtag.Name);
                        }
                    }
                    catch
                    {
                        // Continue if hashtag retrieval fails
                    }

                    resultPosts.Add(post);
                }

                return (resultPosts, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving filtered and formatted posts: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles the selection state of a hashtag in a set of hashtags.
        /// </summary>
        /// <param name="currentHashtags">The current set of selected hashtags.</param>
        /// <param name="hashtagToToggle">The hashtag to toggle.</param>
        /// <param name="allHashtagsFilter">The filter value representing all hashtags.</param>
        /// <returns>The updated set of hashtags after toggling.</returns>
        public HashSet<string> ToggleHashtagSelection(HashSet<string> currentHashtags, string hashtagToToggle, string allHashtagsFilter)
        {
            if (string.IsNullOrEmpty(hashtagToToggle))
            {
                return currentHashtags;
            }

            var updatedHashtags = new HashSet<string>(currentHashtags);

            if (hashtagToToggle == allHashtagsFilter)
            {
                updatedHashtags.Clear();
                updatedHashtags.Add(allHashtagsFilter);
            }
            else
            {
                if (updatedHashtags.Contains(hashtagToToggle))
                {
                    updatedHashtags.Remove(hashtagToToggle);

                    if (updatedHashtags.Count == DEFAULTCOUNT)
                    {
                        updatedHashtags.Add(allHashtagsFilter);
                    }
                }
                else
                {
                    updatedHashtags.Add(hashtagToToggle);

                    if (updatedHashtags.Contains(allHashtagsFilter))
                    {
                        updatedHashtags.Remove(allHashtagsFilter);
                    }
                }
            }

            return updatedHashtags;
        }

        public void AddAllHashtagsToPost(int createdPostId, string[] hashtags, int currentUserId)
        {
            foreach (var hashtag in hashtags)
            {
                try
                {
                    this.AddHashtagToPost(createdPostId, hashtag, currentUserId);
                }
                catch (Exception hashtagException)
                {
                    Debug.WriteLine($"Error adding hashtag '{hashtag}' to post: {hashtagException.Message}");
                    // Continue with other hashtags even if one fails
                }
            }
        }
    }
}