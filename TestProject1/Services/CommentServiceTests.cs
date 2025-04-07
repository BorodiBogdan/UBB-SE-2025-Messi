using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using Duo.Models;
using Duo.Services;
using Duo.Services.Interfaces;
using Duo.Repositories.Interfaces;

namespace TestProject1.Services
{
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _mockCommentRepository;
        private readonly Mock<IPostRepository> _mockPostRepository;
        private readonly Mock<IUserService> _mockUserService;
        private readonly CommentService _commentService;

        public CommentServiceTests()
        {
            _mockCommentRepository = new Mock<ICommentRepository>();
            _mockPostRepository = new Mock<IPostRepository>();
            _mockUserService = new Mock<IUserService>();
            _commentService = new CommentService(_mockCommentRepository.Object, _mockPostRepository.Object, _mockUserService.Object);
        }

        [Fact]
        public void Constructor_WithNullDependencies_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CommentService(null, _mockPostRepository.Object, _mockUserService.Object));
            Assert.Throws<ArgumentNullException>(() => new CommentService(_mockCommentRepository.Object, null, _mockUserService.Object));
            Assert.Throws<ArgumentNullException>(() => new CommentService(_mockCommentRepository.Object, _mockPostRepository.Object, null));
        }

        [Fact]
        public void GetCommentsByPostId_WithValidPostId_ReturnsCommentsWithUsernames()
        {
            // Arrange
            int postId = 1;
            var comments = new List<Comment>
            {
                new Comment(1, "Test comment 1", 1, postId, null, DateTime.Now, 0, 1),
                new Comment(2, "Test comment 2", 2, postId, null, DateTime.Now, 0, 1)
            };

            _mockCommentRepository.Setup(r => r.GetCommentsByPostId(postId)).Returns(comments);
            _mockUserService.Setup(s => s.GetUserById(1)).Returns(new User(1, "User1"));
            _mockUserService.Setup(s => s.GetUserById(2)).Returns(new User(2, "User2"));

            // Act
            var result = _commentService.GetCommentsByPostId(postId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("User1", result[0].Username);
            Assert.Equal("User2", result[1].Username);
        }

        [Fact]
        public void GetCommentsByPostId_WithInvalidPostId_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => _commentService.GetCommentsByPostId(0));
        }

        [Fact]
        public void GetCommentsByPostId_WhenRepositoryThrowsException_ThrowsException()
        {
            // Arrange
            int postId = 1;
            _mockCommentRepository.Setup(r => r.GetCommentsByPostId(postId)).Throws(new Exception("Database error"));

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _commentService.GetCommentsByPostId(postId));
            Assert.Contains("Error retrieving comments", exception.Message);
        }

        [Fact]
        public void GetCommentsByPostId_WhenGetUserByIdFails_ThrowsException()
        {
            // Arrange
            int postId = 1;
            var comments = new List<Comment>
            {
                new Comment(1, "Test comment", 1, postId, null, DateTime.Now, 0, 1)
            };

            _mockCommentRepository.Setup(r => r.GetCommentsByPostId(postId)).Returns(comments);
            _mockUserService.Setup(s => s.GetUserById(1)).Throws(new Exception("User not found"));

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _commentService.GetCommentsByPostId(postId));
        }

        [Fact]
        public void GetCommentsByPostId_WithNoComments_ReturnsEmptyList()
        {
            // Arrange
            int postId = 1;
            var emptyComments = new List<Comment>();

            _mockCommentRepository.Setup(r => r.GetCommentsByPostId(postId)).Returns(emptyComments);

            // Act
            var result = _commentService.GetCommentsByPostId(postId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void CreateComment_WithValidData_CreatesComment()
        {
            // Arrange
            string content = "Test comment";
            int postId = 1;
            int userId = 1;
            var user = new User(userId, "TestUser");
            var post = new Post { Id = postId };

            _mockUserService.Setup(s => s.GetCurrentUser()).Returns(user);
            _mockPostRepository.Setup(r => r.GetPostById(postId)).Returns(post);
            _mockCommentRepository.Setup(r => r.GetCommentsCountForPost(postId)).Returns(0);
            _mockCommentRepository.Setup(r => r.CreateComment(It.IsAny<Comment>())).Returns(1);

            // Act
            var result = _commentService.CreateComment(content, postId);

            // Assert
            Assert.Equal(1, result);
            _mockCommentRepository.Verify(r => r.CreateComment(It.Is<Comment>(c => 
                c.Content == content && 
                c.PostId == postId && 
                c.UserId == userId)), Times.Once);
        }

        [Fact]
        public void CreateComment_WithInvalidData_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => _commentService.CreateComment("", 1));
            Assert.Throws<ArgumentException>(() => _commentService.CreateComment("Test", 0));
        }

        [Fact]
        public void CreateComment_WhenCommentLimitReached_ThrowsException()
        {
            // Arrange
            string content = "Test comment";
            int postId = 1;
            var user = new User(1, "TestUser");
            var post = new Post { Id = postId };

            _mockUserService.Setup(s => s.GetCurrentUser()).Returns(user);
            _mockPostRepository.Setup(r => r.GetPostById(postId)).Returns(post);
            _mockCommentRepository.Setup(r => r.GetCommentsCountForPost(postId)).Returns(1000);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _commentService.CreateComment(content, postId));
            Assert.Equal("Error creating comment: Comment limit reached", exception.Message);
        }

        [Fact]
        public void CreateComment_WithValidParentComment_IncrementsLevel()
        {
            // Arrange
            string content = "Test reply";
            int postId = 1;
            int parentCommentId = 1;
            var user = new User(1, "TestUser");
            var post = new Post { Id = postId };
            var parentComment = new Comment(1, "Parent", 1, postId, null, DateTime.Now, 0, 2);

            _mockUserService.Setup(s => s.GetCurrentUser()).Returns(user);
            _mockPostRepository.Setup(r => r.GetPostById(postId)).Returns(post);
            _mockCommentRepository.Setup(r => r.GetCommentsCountForPost(postId)).Returns(0);
            _mockCommentRepository.Setup(r => r.GetCommentById(parentCommentId)).Returns(parentComment);
            _mockCommentRepository.Setup(r => r.CreateComment(It.IsAny<Comment>())).Returns(2);

            // Act
            var result = _commentService.CreateComment(content, postId, parentCommentId);

            // Assert
            _mockCommentRepository.Verify(r => r.CreateComment(It.Is<Comment>(c => 
                c.Content == content && 
                c.PostId == postId && 
                c.UserId == user.UserId &&
                c.Level == parentComment.Level + 1)), Times.Once);
        }

        [Fact]
        public void CreateComment_WithParentComment_ValidatesNestingLevel()
        {
            // Arrange
            string content = "Test reply";
            int postId = 1;
            int parentCommentId = 1;
            var user = new User(1, "TestUser");
            var post = new Post { Id = postId };
            var parentComment = new Comment(1, "Parent", 1, postId, null, DateTime.Now, 0, 5);

            _mockUserService.Setup(s => s.GetCurrentUser()).Returns(user);
            _mockPostRepository.Setup(r => r.GetPostById(postId)).Returns(post);
            _mockCommentRepository.Setup(r => r.GetCommentsCountForPost(postId)).Returns(0);
            _mockCommentRepository.Setup(r => r.GetCommentById(parentCommentId)).Returns(parentComment);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _commentService.CreateComment(content, postId, parentCommentId));
            Assert.Equal("Error creating comment: Comment nesting limit reached", exception.Message);
        }

        [Fact]
        public void CreateComment_WhenParentCommentNotFound_ThrowsException()
        {
            // Arrange
            string content = "Test reply";
            int postId = 1;
            int parentCommentId = 999; // Non-existent parent comment
            var user = new User(1, "TestUser");
            var post = new Post { Id = postId };

            _mockUserService.Setup(s => s.GetCurrentUser()).Returns(user);
            _mockPostRepository.Setup(r => r.GetPostById(postId)).Returns(post);
            _mockCommentRepository.Setup(r => r.GetCommentsCountForPost(postId)).Returns(0);
            _mockCommentRepository.Setup(r => r.GetCommentById(parentCommentId)).Returns((Comment)null);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _commentService.CreateComment(content, postId, parentCommentId));
            Assert.Equal("Error creating comment: Parent comment not found", exception.Message);
        }

        [Fact]
        public void CreateComment_WhenPostNotFound_ThrowsException()
        {
            // Arrange
            string content = "Test comment";
            int postId = 999; // Non-existent post
            var user = new User(1, "TestUser");

            _mockUserService.Setup(s => s.GetCurrentUser()).Returns(user);
            _mockPostRepository.Setup(r => r.GetPostById(postId)).Returns((Post)null);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _commentService.CreateComment(content, postId));
            Assert.Equal("Error creating comment: Post not found", exception.Message);
        }

        [Fact]
        public void DeleteComment_WithValidData_DeletesComment()
        {
            // Arrange
            int commentId = 1;
            int userId = 1;
            var user = new User(userId, "TestUser");

            _mockUserService.Setup(s => s.GetCurrentUser()).Returns(user);
            _mockCommentRepository.Setup(r => r.DeleteComment(commentId)).Returns(true);

            // Act
            var result = _commentService.DeleteComment(commentId, userId);

            // Assert
            Assert.True(result);
            _mockCommentRepository.Verify(r => r.DeleteComment(commentId), Times.Once);
        }

        [Fact]
        public void DeleteComment_WithInvalidData_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => _commentService.DeleteComment(0, 1));
            Assert.Throws<ArgumentException>(() => _commentService.DeleteComment(1, 0));
        }

        [Fact]
        public void DeleteComment_WhenUserNotAuthorized_ThrowsException()
        {
            // Arrange
            int commentId = 1;
            int userId = 1;
            var currentUser = new User(2, "DifferentUser");

            _mockUserService.Setup(s => s.GetCurrentUser()).Returns(currentUser);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _commentService.DeleteComment(commentId, userId));
            Assert.Equal("Error deleting comment with ID 1: User does not have permission to delete this comment", exception.Message);
        }

        [Fact]
        public void LikeComment_WithValidCommentId_IncrementsLikeCount()
        {
            // Arrange
            int commentId = 1;
            _mockCommentRepository.Setup(r => r.IncrementLikeCount(commentId)).Returns(true);

            // Act
            var result = _commentService.LikeComment(commentId);

            // Assert
            Assert.True(result);
            _mockCommentRepository.Verify(r => r.IncrementLikeCount(commentId), Times.Once);
        }

        [Fact]
        public void LikeComment_WithInvalidCommentId_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => _commentService.LikeComment(0));
        }

        [Fact]
        public void LikeComment_WhenRepositoryThrowsException_ThrowsException()
        {
            // Arrange
            int commentId = 1;
            _mockCommentRepository.Setup(r => r.IncrementLikeCount(commentId)).Throws(new Exception("Database error"));

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _commentService.LikeComment(commentId));
            Assert.Contains("Error liking comment", exception.Message);
        }

        #region GetProcessedCommentsByPostId Tests

        [Fact]
        public void GetProcessedCommentsByPostId_WithValidPostId_ReturnsProcessedComments()
        {
            // Arrange
            int postId = 1;
            var comments = new List<Comment>
            {
                new Comment(1, "Top level comment 1", 1, postId, null, DateTime.Now, 0, 1),
                new Comment(2, "Top level comment 2", 2, postId, null, DateTime.Now, 0, 1),
                new Comment(3, "Reply to comment 1", 1, postId, 1, DateTime.Now, 0, 1), // Level will be updated
                new Comment(4, "Reply to reply", 2, postId, 3, DateTime.Now, 0, 1) // Level will be updated
            };

            _mockCommentRepository.Setup(r => r.GetCommentsByPostId(postId)).Returns(comments);
            _mockUserService.Setup(s => s.GetUserById(It.IsAny<int>())).Returns(new User(1, "TestUser"));

            // Act
            var (allComments, topLevelComments, repliesByParentId) = _commentService.GetProcessedCommentsByPostId(postId);

            // Assert
            Assert.Equal(4, allComments.Count);
            Assert.Equal(2, topLevelComments.Count);
            Assert.Equal(2, repliesByParentId.Count);
            
            // Verify that top-level comments have level 1
            foreach (var comment in topLevelComments)
            {
                Assert.Equal(1, comment.Level);
            }
            
            // Verify that replies have correct levels
            Assert.Equal(2, repliesByParentId[1][0].Level); // Reply to comment 1 has level 2
            Assert.Equal(3, repliesByParentId[3][0].Level); // Reply to reply has level 3
        }

        [Fact]
        public void GetProcessedCommentsByPostId_WithNoComments_ReturnsEmptyCollections()
        {
            // Arrange
            int postId = 1;
            var emptyComments = new List<Comment>();

            _mockCommentRepository.Setup(r => r.GetCommentsByPostId(postId)).Returns(emptyComments);

            // Act
            var (allComments, topLevelComments, repliesByParentId) = _commentService.GetProcessedCommentsByPostId(postId);

            // Assert
            Assert.Empty(allComments);
            Assert.Empty(topLevelComments);
            Assert.Empty(repliesByParentId);
        }

        [Fact]
        public void GetProcessedCommentsByPostId_WithNullComments_ReturnsEmptyCollections()
        {
            // Arrange
            int postId = 1;
            
            _mockCommentRepository.Setup(r => r.GetCommentsByPostId(postId)).Returns((List<Comment>)null);

            // Act
            var (allComments, topLevelComments, repliesByParentId) = _commentService.GetProcessedCommentsByPostId(postId);

            // Assert
            Assert.Empty(allComments);
            Assert.Empty(topLevelComments);
            Assert.Empty(repliesByParentId);
        }

        #endregion

        #region FindCommentInHierarchy Tests

        // Define a simple class for testing the generic FindCommentInHierarchy method
        private class TestComment
        {
            public int Id { get; set; }
            public List<TestComment> Replies { get; set; } = new List<TestComment>();
        }

        [Fact]
        public void FindCommentInHierarchy_WhenCommentExistsInTopLevel_ReturnsComment()
        {
            // Arrange
            int targetId = 2;
            var comments = new List<TestComment>
            {
                new TestComment { Id = 1 },
                new TestComment { Id = 2 },
                new TestComment { Id = 3 }
            };

            // Act
            var result = _commentService.FindCommentInHierarchy<TestComment>(
                targetId,
                comments,
                c => c.Replies,
                c => c.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(targetId, result.Id);
        }

        [Fact]
        public void FindCommentInHierarchy_WhenCommentExistsInReplies_ReturnsComment()
        {
            // Arrange
            int targetId = 4;
            var comments = new List<TestComment>
            {
                new TestComment 
                { 
                    Id = 1,
                    Replies = new List<TestComment>
                    {
                        new TestComment { Id = 4 }
                    }
                },
                new TestComment { Id = 2 }
            };

            // Act
            var result = _commentService.FindCommentInHierarchy<TestComment>(
                targetId,
                comments,
                c => c.Replies,
                c => c.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(targetId, result.Id);
        }

        [Fact]
        public void FindCommentInHierarchy_WhenCommentExistsInNestedReplies_ReturnsComment()
        {
            // Arrange
            int targetId = 5;
            var comments = new List<TestComment>
            {
                new TestComment 
                { 
                    Id = 1,
                    Replies = new List<TestComment>
                    {
                        new TestComment 
                        { 
                            Id = 3,
                            Replies = new List<TestComment>
                            {
                                new TestComment { Id = 5 }
                            }
                        }
                    }
                },
                new TestComment { Id = 2 }
            };

            // Act
            var result = _commentService.FindCommentInHierarchy<TestComment>(
                targetId,
                comments,
                c => c.Replies,
                c => c.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(targetId, result.Id);
        }

        [Fact]
        public void FindCommentInHierarchy_WhenCommentDoesNotExist_ReturnsNull()
        {
            // Arrange
            int targetId = 99; // Non-existent ID
            var comments = new List<TestComment>
            {
                new TestComment { Id = 1 },
                new TestComment 
                { 
                    Id = 2,
                    Replies = new List<TestComment>
                    {
                        new TestComment { Id = 3 }
                    }
                }
            };

            // Act
            var result = _commentService.FindCommentInHierarchy<TestComment>(
                targetId,
                comments,
                c => c.Replies,
                c => c.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindCommentInHierarchy_WithNullTopLevelComments_ReturnsNull()
        {
            // Arrange
            int targetId = 1;

            // Act
            var result = _commentService.FindCommentInHierarchy<TestComment>(
                targetId,
                null,
                c => c.Replies,
                c => c.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindCommentInHierarchy_WithNullReplies_HandlesGracefully()
        {
            // Arrange
            int targetId = 3;
            var comments = new List<TestComment>
            {
                new TestComment { Id = 1, Replies = null },
                new TestComment { Id = 2 }
            };

            // Act
            var result = _commentService.FindCommentInHierarchy<TestComment>(
                targetId,
                comments,
                c => c.Replies,
                c => c.Id);

            // Assert
            Assert.Null(result); // Should return null without throwing an exception
        }

        #endregion

        #region CreateReplyWithDuplicateCheck Tests

        [Fact]
        public void CreateReplyWithDuplicateCheck_WithValidDataAndNoDuplicates_CreatesReply()
        {
            // Arrange
            string replyText = "Test reply";
            int postId = 1;
            int parentCommentId = 2;
            var existingComments = new List<Comment>();
            
            // Setup for CreateComment call
            var user = new User(1, "TestUser");
            var post = new Post { Id = postId };
            var parentComment = new Comment(parentCommentId, "Parent", 1, postId, null, DateTime.Now, 0, 1);

            _mockUserService.Setup(s => s.GetCurrentUser()).Returns(user);
            _mockPostRepository.Setup(r => r.GetPostById(postId)).Returns(post);
            _mockCommentRepository.Setup(r => r.GetCommentsCountForPost(postId)).Returns(0);
            _mockCommentRepository.Setup(r => r.GetCommentById(parentCommentId)).Returns(parentComment);
            _mockCommentRepository.Setup(r => r.CreateComment(It.IsAny<Comment>())).Returns(3); // New comment ID

            // Act
            var (success, replySignature) = _commentService.CreateReplyWithDuplicateCheck(
                replyText, postId, parentCommentId, existingComments);

            // Assert
            Assert.True(success);
            Assert.Equal($"{parentCommentId}_{replyText}", replySignature);
            _mockCommentRepository.Verify(r => r.CreateComment(It.Is<Comment>(c => 
                c.Content == replyText && 
                c.PostId == postId && 
                c.ParentCommentId == parentCommentId)), Times.Once);
        }

        [Fact]
        public void CreateReplyWithDuplicateCheck_WithDuplicateInExistingComments_ReturnsFalse()
        {
            // Arrange
            string replyText = "Test reply";
            int postId = 1;
            int parentCommentId = 2;
            
            // Create list with a duplicate comment
            var existingComments = new List<Comment>
            {
                new Comment(3, replyText, 1, postId, parentCommentId, DateTime.Now, 0, 1)
            };

            // Act
            var (success, replySignature) = _commentService.CreateReplyWithDuplicateCheck(
                replyText, postId, parentCommentId, existingComments);

            // Assert
            Assert.False(success);
            Assert.Equal($"{parentCommentId}_{replyText}", replySignature);
            _mockCommentRepository.Verify(r => r.CreateComment(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public void CreateReplyWithDuplicateCheck_WithDuplicateLastProcessedReply_ReturnsFalse()
        {
            // Arrange
            string replyText = "Test reply";
            int postId = 1;
            int parentCommentId = 2;
            var existingComments = new List<Comment>();
            string lastProcessedReplySignature = $"{parentCommentId}_{replyText}";

            // Act
            var (success, replySignature) = _commentService.CreateReplyWithDuplicateCheck(
                replyText, postId, parentCommentId, existingComments, lastProcessedReplySignature);

            // Assert
            Assert.False(success);
            Assert.Equal(lastProcessedReplySignature, replySignature);
            _mockCommentRepository.Verify(r => r.CreateComment(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public void CreateReplyWithDuplicateCheck_WithCaseInsensitiveDuplicate_ReturnsFalse()
        {
            // Arrange
            string replyText = "Test reply";
            int postId = 1;
            int parentCommentId = 2;
            
            // Create list with a case-insensitive duplicate
            var existingComments = new List<Comment>
            {
                new Comment(3, "TEST REPLY", 1, postId, parentCommentId, DateTime.Now, 0, 1)
            };

            // Act
            var (success, replySignature) = _commentService.CreateReplyWithDuplicateCheck(
                replyText, postId, parentCommentId, existingComments);

            // Assert
            Assert.False(success);
            Assert.Equal($"{parentCommentId}_{replyText}", replySignature);
            _mockCommentRepository.Verify(r => r.CreateComment(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public void CreateReplyWithDuplicateCheck_WithNullExistingComments_HandlesGracefully()
        {
            // Arrange
            string replyText = "Test reply";
            int postId = 1;
            int parentCommentId = 2;
            
            // Setup for CreateComment call
            var user = new User(1, "TestUser");
            var post = new Post { Id = postId };
            var parentComment = new Comment(parentCommentId, "Parent", 1, postId, null, DateTime.Now, 0, 1);

            _mockUserService.Setup(s => s.GetCurrentUser()).Returns(user);
            _mockPostRepository.Setup(r => r.GetPostById(postId)).Returns(post);
            _mockCommentRepository.Setup(r => r.GetCommentsCountForPost(postId)).Returns(0);
            _mockCommentRepository.Setup(r => r.GetCommentById(parentCommentId)).Returns(parentComment);
            _mockCommentRepository.Setup(r => r.CreateComment(It.IsAny<Comment>())).Returns(3);

            // Act
            var (success, replySignature) = _commentService.CreateReplyWithDuplicateCheck(
                replyText, postId, parentCommentId, null);

            // Assert
            Assert.True(success);
            _mockCommentRepository.Verify(r => r.CreateComment(It.IsAny<Comment>()), Times.Once);
        }

        #endregion
    }
} 