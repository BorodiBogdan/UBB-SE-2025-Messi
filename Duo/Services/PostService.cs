using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using Duo.Models;
using Duo.Services;
using Duo.Repositories;
using System.Diagnostics;
using System.Linq;

namespace Duo.Services
{
    public class PostService
    {
        private readonly PostRepository _postRepository;
        private readonly HashtagRepository _hashtagRepository;
        private readonly UserService _userService;
        private readonly SearchService _searchService;
        private const double FUZZY_SEARCH_SCORE_DEFAULT_THRESHOLD = 0.6;
        
        // Constants for validation
        private const int INVALID_ID = 0;
        private const int MIN_PAGE_NUMBER = 1;
        private const int MIN_PAGE_SIZE = 1;
        private const int DEFAULT_COUNT = 0;

        public PostService(PostRepository postRepository, HashtagRepository hashtagRepository, UserService userService, SearchService searchService)
        {
            _postRepository = postRepository;
            _hashtagRepository = hashtagRepository;
            _userService = userService;
            _searchService = searchService;
        }

        public int CreatePost(Post newPost)
        {
            if (string.IsNullOrWhiteSpace(newPost.Title) || string.IsNullOrWhiteSpace(newPost.Description))
            {
                throw new ArgumentException("Title and Description cannot be empty.");
            }

            try
            {
                return _postRepository.CreatePost(newPost);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating post: {ex.Message}");
            }
        }

        public void DeletePost(int postId)
        {
            if (postId <= INVALID_ID)
            {
                throw new ArgumentException("Invalid Post ID.");
            }

            try
            {
                _postRepository.DeletePost(postId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting post with ID {postId}: {ex.Message}");
            }
        }

        public void UpdatePost(Post postToUpdate)
        {
            if (postToUpdate.Id <= INVALID_ID)
            {
                throw new ArgumentException("Invalid Post ID.");
            }

            try
            {
                postToUpdate.UpdatedAt = DateTime.UtcNow;
                _postRepository.UpdatePost(postToUpdate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating post with ID {postToUpdate.Id}: {ex.Message}");
            }
        }

        public Post? GetPostById(int postId)
        {
            if (postId <= INVALID_ID)
            {
                throw new ArgumentException("Invalid Post ID.");
            }

            try
            {
                return _postRepository.GetPostById(postId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving post with ID {postId}: {ex.Message}");
            }
        }

        public Collection<Post> GetPostsByCategory(int categoryId, int pageNumber, int pageSize)
        {
            if (categoryId <= INVALID_ID || pageNumber < MIN_PAGE_NUMBER || pageSize < MIN_PAGE_SIZE)
            {
                throw new ArgumentException("Invalid pagination parameters.");
            }

            try
            {
                return _postRepository.GetPostsByCategoryId(categoryId, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving posts for category {categoryId}: {ex.Message}");
            }
        }

        public List<Post> GetPaginatedPosts(int pageNumber, int pageSize)
        {
            if (pageNumber < MIN_PAGE_NUMBER || pageSize < MIN_PAGE_SIZE)
            {
                throw new ArgumentException("Invalid pagination parameters.");
            }

            try
            {
                return _postRepository.GetPaginatedPosts(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving paginated posts: {ex.Message}");
            }
        }

        public int GetTotalPostCount()
        {
            try
            {
                return _postRepository.GetTotalPostCount();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving total post count: {ex.Message}");
            }
        }

        public int GetPostCountByCategoryId(int categoryId)
        {
            if (categoryId <= INVALID_ID)
            {
                throw new ArgumentException("Invalid Category ID.");
            }

            try
            {
                return _postRepository.GetPostCountByCategory(categoryId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving post count for category {categoryId}: {ex.Message}");
            }
        }

        public int GetPostCountByHashtags(List<string> hashtagList)
        {
            if (hashtagList == null || hashtagList.Count == DEFAULT_COUNT)
            {
                return GetTotalPostCount();
            }

            List<string> filteredHashtags = hashtagList.Where(h => !string.IsNullOrWhiteSpace(h)).ToList();
            if (filteredHashtags.Count == DEFAULT_COUNT)
            {
                return GetTotalPostCount();
            }

            try
            {
                return _postRepository.GetPostCountByHashtags(filteredHashtags);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving post count for hashtags: {ex.Message}");
            }
        }

        public List<Hashtag> GetAllHashtags()
        {
            try
            {
                return _hashtagRepository.GetAllHashtags();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all hashtags: {ex.Message}");
            }
        }

        public List<Hashtag> GetHashtagsByCategory(int categoryId)
        {
            if (categoryId <= INVALID_ID)
            {
                throw new ArgumentException("Invalid Category ID.");
            }

            try
            {
                return _hashtagRepository.GetHashtagsByCategory(categoryId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving hashtags for category {categoryId}: {ex.Message}");
            }
        }

        public List<Post> GetPostsByHashtags(List<string> hashtagList, int pageNumber, int pageSize)
        {
            if (pageNumber < MIN_PAGE_NUMBER || pageSize < MIN_PAGE_SIZE)
            {
                throw new ArgumentException("Invalid pagination parameters.");
            }

            if (hashtagList == null || hashtagList.Count == DEFAULT_COUNT)
            {
                return GetPaginatedPosts(pageNumber, pageSize);
            }

            List<string> filteredHashtags = hashtagList.Where(h => !string.IsNullOrWhiteSpace(h)).ToList();
            if (filteredHashtags.Count == DEFAULT_COUNT)
            {
                return GetPaginatedPosts(pageNumber, pageSize);
            }

            try
            {
                return _postRepository.GetPostsByHashtags(filteredHashtags, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving posts for hashtags: {ex.Message}");
            }
        }

        public bool ValidatePostOwnership(int authorUserId, int targetPostId)
        {
            int? postOwnerId = _postRepository.GetUserIdByPostId(targetPostId);
            return authorUserId == postOwnerId;
        }

        public List<Hashtag> GetHashtagsByPostId(int postId)
        {
            if (postId <= INVALID_ID) throw new ArgumentException("Invalid Post ID.");

            try
            {
                return _hashtagRepository.GetHashtagsByPostId(postId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving hashtags for post with ID {postId}: {ex.Message}");
            }
        }

        public bool LikePost(int postId)
        {
            if (postId <= INVALID_ID) throw new ArgumentException("Invalid Post ID.");

            try
            {
                var targetPost = _postRepository.GetPostById(postId);
                if (targetPost == null) throw new Exception("Post not found");

                targetPost.LikeCount++;

                _postRepository.UpdatePost(targetPost);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error liking post with ID {postId}: {ex.Message}");
            }
        }

        public bool AddHashtagToPost(int postId, string hashtagName, int userId)
        {
            if (postId <= INVALID_ID)
            {
                throw new ArgumentException("Invalid Post ID.");
            }
            
            if (string.IsNullOrWhiteSpace(hashtagName))
            {
                throw new ArgumentException("Tag name cannot be empty.");
            }
            
            if (userId <= INVALID_ID)
            {
                throw new ArgumentException("Invalid User ID.");
            }

            try
            {
                var targetPost = _postRepository.GetPostById(postId);
                if (targetPost == null)
                {
                    throw new Exception($"Post with ID {postId} not found");
                }
                
                if (_userService.GetCurrentUser().UserId != userId)
                {
                    throw new Exception("User does not have permission to add hashtags to this post.");
                }

                Hashtag? existingHashtag = null;
                existingHashtag = _hashtagRepository.GetHashtagByText(hashtagName);

                Hashtag hashtag = _hashtagRepository.CreateHashtag(hashtagName);

                bool addResult = _hashtagRepository.AddHashtagToPost(postId, hashtag.Id);
                return addResult;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding hashtag to post with ID {postId}: {ex.Message}");
            }
        }

        public bool RemoveHashtagFromPost(int postId, int hashtagId, int userId)
        {
            if (postId <= INVALID_ID) throw new ArgumentException("Invalid Post ID.");
            if (hashtagId <= INVALID_ID) throw new ArgumentException("Invalid Hashtag ID.");
            if (userId <= INVALID_ID) throw new ArgumentException("Invalid User ID.");

            try
            {
                if (_userService.GetCurrentUser().UserId != userId)
                    throw new Exception("User does not have permission to remove hashtags from this post.");

                return _hashtagRepository.RemoveHashtagFromPost(postId, hashtagId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing hashtag from post with ID {postId}: {ex.Message}");
            }
        }


        public int CreatePostWithHashtags(Post newPost, List<string> hashtagList, int authorId)
        {
            if (string.IsNullOrWhiteSpace(newPost.Title) || string.IsNullOrWhiteSpace(newPost.Description))
            {
                throw new ArgumentException("Title and Description cannot be empty.");
            }

            try
            {
                int createdPostId = _postRepository.CreatePost(newPost);
                
                if (createdPostId <= INVALID_ID)
                {
                    throw new Exception("Failed to create post: Invalid post ID returned from database");
                }
                
                try
                {
                    var createdPost = _postRepository.GetPostById(createdPostId);
                }
                catch (Exception ex)
                {
                }
                
                if (hashtagList != null && hashtagList.Count > DEFAULT_COUNT)
                {    
                    foreach (var hashtagName in hashtagList)
                    {
                        try
                        {
                            Hashtag? existingHashtag = _hashtagRepository.GetHashtagByText(hashtagName);
                            Hashtag hashtag = _hashtagRepository.CreateHashtag(hashtagName);
                            
                            bool addSuccess = _hashtagRepository.AddHashtagToPost(createdPostId, hashtag.Id);
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
    }
}