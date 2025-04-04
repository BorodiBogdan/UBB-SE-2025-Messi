using Duo.Data;
using Duo.Models;
using Duo.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

[TestClass]
public class CommentRepositoryUnitTests
{
    private IDatabaseConnection _dataLinkMock;

    [TestInitialize]
    public void Initialize()
    {
        _dataLinkMock = new MockDatabaseConnectionCommentRepository();
    }

    [TestMethod]
    public void TestGetCommentRepository_CorrectlyInstanciated_ReturnsInstance()
    {
        var commentRepository = new CommentRepository(_dataLinkMock);
        Assert.IsNotNull(commentRepository);
    }
    [TestMethod]
    public void TestGetCommentByPostId_ReturnsComment()
    {
        var commentRepository = new CommentRepository(_dataLinkMock);
        var comment = commentRepository.GetCommentById(1);
        Assert.IsNotNull(comment);
        Assert.AreEqual(1, comment.Id);
        Assert.IsInstanceOfType(comment, typeof(Comment));
    }
    [TestMethod]
    public void TestGetCommentsById_ReturnsListComments()
    {
        var commentRepository = new CommentRepository(_dataLinkMock);
        var comments = commentRepository.GetCommentsByPostId(1);
        Assert.IsNotNull(comments);
        Assert.IsInstanceOfType(comments, typeof(List<Comment>));
        Assert.AreEqual(3, comments.Count);
    }
    [TestMethod]
    public void TestCreateComment_ReturnsInt()
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
        Assert.IsInstanceOfType(result, typeof(int));
    }
    [TestMethod]
    public void TestDeleteComment_ReturnsBoolean()
    {
        var commentRepository = new CommentRepository(_dataLinkMock);
        var result = commentRepository.DeleteComment(1);
        Assert.IsTrue(result);
    }
    [TestMethod]
    public void TestGetRepliesByCommentId_ReturnsListComments()
    {
        var commentRepository = new CommentRepository(_dataLinkMock);
        var replies = commentRepository.GetRepliesByCommentId(1);
        Assert.IsNotNull(replies);
        Assert.IsInstanceOfType(replies, typeof(List<Comment>));
        Assert.AreEqual(3, replies.Count);
    }
    [TestMethod]
    public void TestIncrementLikeCount_ReturnsBoolean()
    {
        var commentRepository = new CommentRepository(_dataLinkMock);
        var result = commentRepository.IncrementLikeCount(1);
        Assert.IsTrue(result);
    }
    [TestMethod]
    public void TestGetCommentsCountForPost_ReturnsInt()
    {
        var commentRepository = new CommentRepository(_dataLinkMock);
        var result = commentRepository.GetCommentsCountForPost(1);
        Assert.IsInstanceOfType(result, typeof(int));
        Assert.AreEqual(3, result);
    }
}