using System;
using System.Collections.Generic;
using Duo.Models;
using Duo.Services;
using Duo.Repositories;
using Duo.Services.Interfaces;
using Duo.Repositories.Interfaces;
using System.Linq;

namespace Duo.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;
        private const int MINIMUM_ALLOWED_ID_NUMBER = 0;
        private const int MAXIMUM_COMMENT_COUNT = 1000;
        private const int MAXIMUM_COMMENT_LEVEL = 5;


        public CommentService(ICommentRepository commentRepository, IPostRepository postRepository, IUserService userService)
        {
            _commentRepository = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
            _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public List<Comment> GetCommentsByPostId(int postId)
        {
            if (postId <= MINIMUM_ALLOWED_ID_NUMBER) throw new ArgumentException("Invalid post ID", nameof(postId));

            try
            {
                var comments = _commentRepository.GetCommentsByPostId(postId);

                if (comments != null && comments.Count > 0)
                {
                    foreach (var comment in comments)
                    {
                        try
                        {
                            User user = _userService.GetUserById(comment.UserId);
                            comment.Username = user.Username;

                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }
                else
                {
                    return new List<Comment>();
                }

                return comments;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving comments for post ID {postId}: {ex.Message}", ex);
            }
        }

        public (List<Comment> AllComments, List<Comment> TopLevelComments, Dictionary<int, List<Comment>> RepliesByParentId) GetProcessedCommentsByPostId(int postId)
        {
            // Get all comments for the post
            var allComments = GetCommentsByPostId(postId);
            
            if (allComments == null || !allComments.Any())
            {
                return (new List<Comment>(), new List<Comment>(), new Dictionary<int, List<Comment>>());
            }
            
            // Organize comments into hierarchical structure
            var topLevelComments = allComments.Where(c => c.ParentCommentId == null).ToList();
            
            var repliesByParentId = allComments
                            .Where(c => c.ParentCommentId.HasValue)
                            .GroupBy(c => c.ParentCommentId.Value)
                            .ToDictionary(g => g.Key, g => g.ToList());
                            
            // Set proper level values for all comments
            foreach (var comment in topLevelComments)
            {
                comment.Level = 1;
            }

            foreach (var parentId in repliesByParentId.Keys)
            {
                var parentComment = allComments.FirstOrDefault(c => c.Id == parentId);
                if (parentComment != null)
                {
                    foreach (var reply in repliesByParentId[parentId])
                    {
                        reply.Level = parentComment.Level + 1;
                    }
                }
            }
            
            return (allComments, topLevelComments, repliesByParentId);
        }

        public int CreateComment(string content, int postId, int? parentCommentId = null)
        {
            if (postId <= MINIMUM_ALLOWED_ID_NUMBER) throw new ArgumentException("Invalid post ID", nameof(postId));
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be empty", nameof(content));

            try
            {
                ValidateCommentCount(postId);

                int level = 1;
                if (parentCommentId.HasValue)
                {
                    var parentComment = _commentRepository.GetCommentById(parentCommentId.Value);
                    if (parentComment == null) throw new Exception("Parent comment not found");
                    if (parentComment.Level >= MAXIMUM_COMMENT_LEVEL) throw new Exception("Comment nesting limit reached");
                    level = parentComment.Level + 1;
                }

                User user = _userService.GetCurrentUser();

                var comment = new Comment
                {
                    Content = content,
                    PostId = postId,
                    UserId = user.UserId,
                    ParentCommentId = parentCommentId,
                    CreatedAt = DateTime.Now,
                    Level = level
                };

                return _commentRepository.CreateComment(comment);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating comment: {ex.Message}", ex);
            }
        }

        public bool DeleteComment(int commentId, int userId)
        {
            if (commentId <= MINIMUM_ALLOWED_ID_NUMBER) throw new ArgumentException("Invalid comment ID", nameof(commentId));
            if (userId <= MINIMUM_ALLOWED_ID_NUMBER) throw new ArgumentException("Invalid user ID", nameof(userId));

            try
            {
                User user = _userService.GetCurrentUser();
                if (user.UserId != userId) throw new Exception("User does not have permission to delete this comment");

                return _commentRepository.DeleteComment(commentId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting comment with ID {commentId}: {ex.Message}", ex);
            }
        }

        public bool LikeComment(int commentId)
        {
            if (commentId <= MINIMUM_ALLOWED_ID_NUMBER) throw new ArgumentException("Invalid comment ID", nameof(commentId));

            try
            {
                return _commentRepository.IncrementLikeCount(commentId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error liking comment with ID {commentId}: {ex.Message}", ex);
            }
        }


        public T FindCommentInHierarchy<T>(
            int commentId, 
            IEnumerable<T> topLevelComments,
            Func<T, IEnumerable<T>> getRepliesFunc,
            Func<T, int> getIdFunc) where T : class
        {
            if (topLevelComments == null)
                return null;

            // First, check top-level comments
            var comment = topLevelComments.FirstOrDefault(c => getIdFunc(c) == commentId);
            if (comment != null)
            {
                return comment;
            }
            
            // If not found in top-level, recursively search through replies
            foreach (var topLevelComment in topLevelComments)
            {
                var replies = getRepliesFunc(topLevelComment);
                var foundInReplies = FindCommentInRepliesRecursive(commentId, replies, getRepliesFunc, getIdFunc);
                if (foundInReplies != null)
                {
                    return foundInReplies;
                }
            }
            
            return null;
        }


        private T FindCommentInRepliesRecursive<T>(
            int commentId, 
            IEnumerable<T> replies,
            Func<T, IEnumerable<T>> getRepliesFunc,
            Func<T, int> getIdFunc) where T : class
        {
            if (replies == null)
                return null;
                
            foreach (var reply in replies)
            {
                if (getIdFunc(reply) == commentId)
                    return reply;
                    
                var foundInNestedReplies = FindCommentInRepliesRecursive(commentId, getRepliesFunc(reply), getRepliesFunc, getIdFunc);
                if (foundInNestedReplies != null)
                    return foundInNestedReplies;
            }
            
            return null;
        }

        private void ValidateCommentCount(int postId)
        {
            var post = _postRepository.GetPostById(postId);
            if (post == null) throw new Exception("Post not found");

            var commentCount = _commentRepository.GetCommentsCountForPost(postId);
            if (commentCount >= MAXIMUM_COMMENT_COUNT) throw new Exception("Comment limit reached");
        }

        public (bool Success, string ReplySignature) CreateReplyWithDuplicateCheck(
            string replyText, 
            int postId, 
            int parentCommentId, 
            IEnumerable<Comment> existingComments,
            string lastProcessedReplySignature = null)
        {

            // Create a unique signature for this reply
            string replySignature = $"{parentCommentId}_{replyText}";

            // Check if this is a duplicate of the last processed reply
            if (lastProcessedReplySignature == replySignature)
            {
                return (false, replySignature);
            }

            // Check for duplicate comments in the existing comments
            bool isDuplicate = existingComments != null && existingComments.Any(comment => 
                comment.ParentCommentId == parentCommentId && 
                comment.Content.Equals(replyText, StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
            {
                return (false, replySignature);
            }

            // Create the comment/reply
            int commentId = CreateComment(replyText, postId, parentCommentId);
            
            return (commentId > 0, replySignature);
        }

    }
}