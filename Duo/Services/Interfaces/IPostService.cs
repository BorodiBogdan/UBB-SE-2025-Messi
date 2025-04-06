using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Duo.Models;

namespace Duo.Services.Interfaces
{
    public interface IPostService
    {
        int CreatePost(Post newPost);
        void DeletePost(int postId);
        void UpdatePost(Post postToUpdate);
        Post? GetPostById(int postId);
        Collection<Post> GetPostsByCategory(int categoryId, int pageNumber, int pageSize);
        List<Post> GetPaginatedPosts(int pageNumber, int pageSize);
        int GetTotalPostCount();
        int GetPostCountByCategoryId(int categoryId);
        int GetPostCountByHashtags(List<string> hashtagList);
        List<Hashtag> GetAllHashtags();
        List<Hashtag> GetHashtagsByCategory(int categoryId);
        List<Post> GetPostsByHashtags(List<string> hashtagList, int pageNumber, int pageSize);
        bool ValidatePostOwnership(int authorUserId, int targetPostId);
        List<Hashtag> GetHashtagsByPostId(int postId);
        bool LikePost(int postId);
        bool AddHashtagToPost(int postId, string hashtagName, int userId);
        bool RemoveHashtagFromPost(int postId, int hashtagId, int userId);
        int CreatePostWithHashtags(Post newPost, List<string> hashtagList, int authorId);
    }
} 