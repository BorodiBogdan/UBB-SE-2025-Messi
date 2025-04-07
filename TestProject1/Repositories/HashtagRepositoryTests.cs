using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Xunit;
using Duo.Repositories;
using Duo.Data;
using Duo.Models;
using Duo.Repositories.Interfaces;

namespace TestProject1.Repositories
{
    public class HashtagRepositoryTests : IDisposable
    {
        private readonly MockDatabaseConnectionHashtagRepository _dataLinkMock;
        private readonly HashtagRepository _repository;

        public HashtagRepositoryTests()
        {
            _dataLinkMock = new MockDatabaseConnectionHashtagRepository();
            _repository = new HashtagRepository(_dataLinkMock);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }

        [Fact]
        public void GetHashtagByText_ValidText_ReturnsHashtag()
        {
            // Act
            var result = _repository.GetHashtagByText("test");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("test", result.Tag);
        }

        [Fact]
        public void GetHashtagByText_EmptyText_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<Exception>(() => _repository.GetHashtagByText(""));
        }

        [Fact]
        public void GetHashtagByText_NoResults_ReturnsNull()
        {
            // Act
            var result = _repository.GetHashtagByText("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetHashtagByText_SqlError_ReturnsNull()
        {
            // Act
            var result = _repository.GetHashtagByText("error");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void CreateHashtag_ValidTag_ReturnsNewHashtag()
        {
            // Act
            var result = _repository.CreateHashtag("newtag");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("newtag", result.Tag);
        }

        [Fact]
        public void CreateHashtag_EmptyTag_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<Exception>(() => _repository.CreateHashtag(""));
        }

        [Fact]
        public void CreateHashtag_SqlError_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _repository.CreateHashtag("error"));
            Assert.Contains("Error - CreateHashtag:", exception.Message);
        }

        [Fact]
        public void GetHashtagsByPostId_ValidPostId_ReturnsHashtags()
        {
            // Act
            var result = _repository.GetHashtagsByPostId(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("test", result[0].Tag);
            Assert.Equal("tag1", result[1].Tag);
            Assert.Equal("tag2", result[2].Tag);
        }

        [Fact]
        public void GetHashtagsByPostId_InvalidPostId_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<Exception>(() => _repository.GetHashtagsByPostId(0));
        }

        [Fact]
        public void GetHashtagsByPostId_SqlError_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _repository.GetHashtagsByPostId(404));
            Assert.Contains("Error - GetHashtagsByPostId:", exception.Message);
        }

        [Fact]
        public void AddHashtagToPost_ValidIds_ReturnsTrue()
        {
            // Act
            var result = _repository.AddHashtagToPost(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AddHashtagToPost_InvalidPostId_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<Exception>(() => _repository.AddHashtagToPost(0, 1));
        }

        [Fact]
        public void AddHashtagToPost_SqlError_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _repository.AddHashtagToPost(404, 1));
            Assert.Contains("Error - AddHashtagToPost:", exception.Message);
        }

        [Fact]
        public void AddHashtagToPost_InvalidHashtagId_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _repository.AddHashtagToPost(1, 404));
            Assert.Contains("Error - AddHashtagToPost:", exception.Message);
        }

        [Fact]
        public void RemoveHashtagFromPost_ValidIds_ReturnsTrue()
        {
            // Act
            var result = _repository.RemoveHashtagFromPost(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void RemoveHashtagFromPost_InvalidPostId_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<Exception>(() => _repository.RemoveHashtagFromPost(0, 1));
        }

        [Fact]
        public void RemoveHashtagFromPost_SqlError_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _repository.RemoveHashtagFromPost(404, 1));
            Assert.Contains("Error - RemoveHashtagFromPost:", exception.Message);
        }

        [Fact]
        public void RemoveHashtagFromPost_InvalidHashtagId_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _repository.RemoveHashtagFromPost(1, 404));
            Assert.Contains("Error - RemoveHashtagFromPost:", exception.Message);
        }

        [Fact]
        public void GetHashtagByName_ValidName_ReturnsHashtag()
        {
            // Act
            var result = _repository.GetHashtagByName("test");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("test", result.Tag);
        }

        [Fact]
        public void GetAllHashtags_ReturnsAllHashtags()
        {
            // Act
            var result = _repository.GetAllHashtags();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count);
            Assert.Equal("test", result[0].Tag);
            Assert.Equal("tag1", result[1].Tag);
            Assert.Equal("tag2", result[2].Tag);
            Assert.Equal("category1", result[3].Tag);
            Assert.Equal("category2", result[4].Tag);
        }

        [Fact]
        public void GetAllHashtags_SqlError_ThrowsException()
        {
            // Arrange
            _dataLinkMock.SetShouldThrowSqlException(true);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _repository.GetAllHashtags());
            Assert.Contains("Error - GetAllHashtags:", exception.Message);
        }

        [Fact]
        public void GetHashtagsByCategory_ValidCategoryId_ReturnsHashtags()
        {
            // Act
            var result = _repository.GetHashtagsByCategory(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("category1", result[0].Tag);
            Assert.Equal("category2", result[1].Tag);
        }

        [Fact]
        public void GetHashtagsByCategory_InvalidCategoryId_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<Exception>(() => _repository.GetHashtagsByCategory(0));
        }

        [Fact]
        public void GetHashtagsByCategory_SqlError_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _repository.GetHashtagsByCategory(404));
            Assert.Contains("Error - GetHashtagsByCategory:", exception.Message);
        }
    }
}