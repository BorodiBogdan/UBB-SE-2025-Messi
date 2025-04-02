using Duo.Data;
using Duo.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

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
}