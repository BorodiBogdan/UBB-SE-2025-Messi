using Duo.Data;
using Duo.Models;
using Duo.Repositories;
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
        public void GetCommentByPostId_ReturnsComment()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var comment = commentRepository.GetCommentById(1);
            Assert.NotNull(comment);
            Assert.Equal(1, comment.Id);
            Assert.IsType<Comment>(comment);
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
        public void DeleteComment_ReturnsBoolean()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var result = commentRepository.DeleteComment(1);
            Assert.True(result);
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
        public void IncrementLikeCount_ReturnsBoolean()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var result = commentRepository.IncrementLikeCount(1);
            Assert.True(result);
        }

        [Fact]
        public void GetCommentsCountForPost_ReturnsInt()
        {
            var commentRepository = new CommentRepository(_dataLinkMock);
            var result = commentRepository.GetCommentsCountForPost(1);
            Assert.IsType<int>(result);
            Assert.Equal(3, result);
        }
    }
} 