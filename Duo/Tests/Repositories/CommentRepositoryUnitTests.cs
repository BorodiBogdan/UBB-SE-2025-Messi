using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using Duo.Data;
using Duo.Models;
using Duo.Repositories;
using Duo.Repositories.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Duo.Tests.Repositories
{
    [TestClass]
    public class CommentRepositoryUnitTests
    {
        private Mock<DataLink>? _dataLink;

        [TestInitialize]
        public void initialize()
        {
            _dataLink = new Mock<DataLink>();
        }

        [TestMethod]
        public void TestGetCommentRepository_CorrectlyInstanciated_ReturnsInstance()
        {
            var commentRepository = new CommentRepository(_dataLink.Object); 

            Assert.IsNotNull(commentRepository);
        }
    }
}
