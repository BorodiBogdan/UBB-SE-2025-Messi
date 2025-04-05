using Duo.Data;
using Duo.Models;
using Duo.Repositories;
using Microsoft.Data.SqlClient;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace TestMessi.Repositories
{
    public class CommentRepositoryTests
    {
        private IDatabaseConnection _dataLinkMock;

        public CommentRepositoryTests()
        {
            // This is executed before each test (like TestInitialize in MSTest)
            _dataLinkMock = new MockDatabaseConnectionCommentRepository();
        }

        [Fact]
        public void GetCommentRepository_CorrectlyInstanciated_ReturnsInstance()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.NotNull(commentRepository);
        }
        [Fact]
        public void GetCommentRepository_NullInstanciated_ThrowsError()
        {
            Assert.Throws<ArgumentNullException>(() => new CommentRepository(null));
        }

        [Fact]
        public void GetCommentByPostId_ReturnsComment()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var comment = commentRepository.GetCommentById(1);
            Assert.NotNull(comment);
            Assert.Equal(1, comment.Id);
            Assert.IsType<Comment>(comment);
        }
        [Fact]
        public void void_GetCommentById_ThrowsExceptionAfterSqlFailure()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => commentRepository.GetCommentById(404));
        }
        [Fact]
        public void GetCommentById_ThrowsException()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<System.Exception>(() => commentRepository.GetCommentById(40));
        }
        [Fact]
        public void GetCommentByPostId_ThrowsException()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<System.ArgumentException>(() => commentRepository.GetCommentById(0));
        }
        [Fact]
        public void GetCommentsById_ReturnsListComments()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var comments = commentRepository.GetCommentsByPostId(1);
            Assert.NotNull(comments);
            Assert.IsType<List<Comment>>(comments);
            Assert.Equal(3, comments.Count);
        }
        [Fact]
        public void GetCommentsById_ThrowsException()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<System.ArgumentException>(() => commentRepository.GetCommentsByPostId(0));
        }
        [Fact]
        public void GetCommentsById_ThrowsNotFoundException()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => commentRepository.GetCommentsByPostId(20));
        }
        [Fact]
        public void GetCommentsById_ThrowsExceptionAfterSqlFailure()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => commentRepository.GetCommentsByPostId(404));
        }
        [Fact]
        public void CreateComment_ReturnsInt()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var comment = new Comment
            {
                Content = "Test comment",
                UserId = 1,
                PostId = 1,
                ParentCommentId = null,
                CreatedAt = DateTime.Now,
                LikeCount = 0,
                Level = 1
            };
            var result = commentRepository.CreateComment(comment);
            Assert.IsType<int>(result);
        }
        [Fact]
        public void CreateComment_ThrowsException()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);

            var comment = new Comment
            {
                Content = "Test comment",
                UserId = 404,
                PostId = 1,
                ParentCommentId = null,
                CreatedAt = DateTime.Now,
                LikeCount = 0,
                Level = 1
            };

            Assert.Throws<Exception>(() => commentRepository.CreateComment(comment));
        }
        [Fact]
        public void DeleteComment_ReturnsBoolean()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var result = commentRepository.DeleteComment(1);
            Assert.True(result);
        }
        [Fact]
        public void DeleteComment_ThrowsExceptionAfterSqlFailure()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => commentRepository.DeleteComment(404));
        }
        [Fact]
        public void DeleteComment_ThrowsException()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<ArgumentException>(() => commentRepository.DeleteComment(0));
        }

        [Fact]
        public void GetRepliesByCommentId_ReturnsListComments()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var replies = commentRepository.GetRepliesByCommentId(1);
            Assert.NotNull(replies);
            Assert.IsType<List<Comment>>(replies);
            Assert.Equal(3, replies.Count);
        }
        [Fact]
        public void GetRepliesByCommentId_ThrowsException()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<System.ArgumentException>(() => commentRepository.GetRepliesByCommentId(0));
        }
        [Fact]
        public void GetRepliesByCommentId_ThrowsExceptionAfterSqlFailure()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => commentRepository.GetRepliesByCommentId(404));
        }

        [Fact]
        public void IncrementLikeCount_ReturnsBoolean()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var result = commentRepository.IncrementLikeCount(1);
            Assert.True(result);
        }
        [Fact]
        public void IncrementLikeCount_ThrowsExceptionAfterSqlFailure()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => commentRepository.IncrementLikeCount(404));
        }
        [Fact]
        public void IncrementLikeCount_ThrowsException()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<System.ArgumentException>(() => commentRepository.IncrementLikeCount(0));
        }

        [Fact]
        public void GetCommentsCountForPost_ReturnsInt()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var result = commentRepository.GetCommentsCountForPost(1);
            Assert.IsType<int>(result);
            Assert.Equal(3, result);
        }
        [Fact]
        public void GetCommentsCountForPost_ThrowsExceptionAfterSqlFailure()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<Exception>(() => commentRepository.GetCommentsCountForPost(404));
        }
        [Fact]
        public void GetCommentsCountForPost_ThrowsException()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<System.ArgumentException>(() => commentRepository.GetCommentsCountForPost(0));
        }
        [Fact]
        public void CreateComment_ThrowsArgumentNullException()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            Assert.Throws<ArgumentNullException>(() => commentRepository.CreateComment(null));
        }

        [Fact]
        public void CreateComment_ThrowsArgumentException_WhenContentIsEmpty()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var comment = new Comment
            {
                Content = "",
                UserId = 1,
                PostId = 1,
                ParentCommentId = null,
                CreatedAt = DateTime.Now,
                LikeCount = 0,
                Level = 1
            };
            Assert.Throws<ArgumentException>(() => commentRepository.CreateComment(comment));
        }

        [Fact]
        public void CreateComment_ThrowsArgumentException_WhenUserIdIsInvalid()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var comment = new Comment
            {
                Content = "Test comment",
                UserId = 0,
                PostId = 1,
                ParentCommentId = null,
                CreatedAt = DateTime.Now,
                LikeCount = 0,
                Level = 1
            };
            Assert.Throws<ArgumentException>(() => commentRepository.CreateComment(comment));
        }

        [Fact]
        public void CreateComment_ThrowsArgumentException_WhenPostIdIsInvalid()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var comment = new Comment
            {
                Content = "Test comment",
                UserId = 1,
                PostId = 0,
                ParentCommentId = null,
                CreatedAt = DateTime.Now,
                LikeCount = 0,
                Level = 1
            };
            Assert.Throws<ArgumentException>(() => commentRepository.CreateComment(comment));
        }

    }
} 