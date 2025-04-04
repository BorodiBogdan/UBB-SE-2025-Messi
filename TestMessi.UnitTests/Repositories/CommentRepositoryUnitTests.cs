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
        _dataLinkMock = new MockDatabaseConnection();
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
}