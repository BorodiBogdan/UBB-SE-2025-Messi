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
    }
} 