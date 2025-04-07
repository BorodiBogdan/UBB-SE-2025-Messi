using Moq;
using System;
using System.Collections.Generic;
using Duo.Models;
using Duo.Repositories.Interfaces;
using Duo.Services;
using Xunit;
using Microsoft.Data.SqlClient;

namespace TestProject1.Services
{
    public class CategoryServiceTests
    {
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private CategoryService _categoryService;

        // Test data
        private const string VALID_CATEGORY_NAME = "Valid Category";
        private const string INVALID_CATEGORY_NAME = "";

        public CategoryServiceTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _categoryService = new CategoryService(_mockCategoryRepository.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CategoryService(null));
        }

        #endregion

        #region GetAllCategories Tests

        [Fact]
        public void GetAllCategories_ReturnsAllCategories()
        {
            // Arrange
            var expectedCategories = new List<Category>
            {
                new Category(1, "Category 1"),
                new Category(2, "Category 2"),
                new Category(3, "Category 3")
            };

            _mockCategoryRepository.Setup(repo => repo.GetCategories(It.IsAny<SqlParameter[]>()))
                .Returns(expectedCategories);

            // Act
            var result = _categoryService.GetAllCategories();

            // Assert
            Assert.Equal(expectedCategories.Count, result.Count);
            Assert.Equal(expectedCategories, result);
            _mockCategoryRepository.Verify(repo => repo.GetCategories(It.IsAny<SqlParameter[]>()), Times.Once);
        }

        [Fact]
        public void GetAllCategories_RepositoryThrowsException_ReturnsEmptyList()
        {
            // Arrange
            _mockCategoryRepository.Setup(repo => repo.GetCategories(It.IsAny<SqlParameter[]>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = _categoryService.GetAllCategories();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockCategoryRepository.Verify(repo => repo.GetCategories(It.IsAny<SqlParameter[]>()), Times.Once);
        }

        #endregion

        #region GetCategoryByName Tests

        [Fact]
        public void GetCategoryByName_WithValidName_ReturnsCategory()
        {
            // Arrange
            var expectedCategory = new Category(1, VALID_CATEGORY_NAME);

            _mockCategoryRepository.Setup(repo => repo.GetCategoryByName(VALID_CATEGORY_NAME))
                .Returns(expectedCategory);

            // Act
            var result = _categoryService.GetCategoryByName(VALID_CATEGORY_NAME);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCategory.Id, result.Id);
            Assert.Equal(expectedCategory.Name, result.Name);
            _mockCategoryRepository.Verify(repo => repo.GetCategoryByName(VALID_CATEGORY_NAME), Times.Once);
        }

        [Fact]
        public void GetCategoryByName_WithNonExistentName_ReturnsNull()
        {
            // Arrange
            _mockCategoryRepository.Setup(repo => repo.GetCategoryByName("NonExistentCategory"))
                .Returns((Category)null);

            // Act
            var result = _categoryService.GetCategoryByName("NonExistentCategory");

            // Assert
            Assert.Null(result);
            _mockCategoryRepository.Verify(repo => repo.GetCategoryByName("NonExistentCategory"), Times.Once);
        }

        [Fact]
        public void GetCategoryByName_RepositoryThrowsException_ReturnsNull()
        {
            // Arrange
            _mockCategoryRepository.Setup(repo => repo.GetCategoryByName(VALID_CATEGORY_NAME))
                .Throws(new Exception("Database error"));

            // Act
            var result = _categoryService.GetCategoryByName(VALID_CATEGORY_NAME);

            // Assert
            Assert.Null(result);
            _mockCategoryRepository.Verify(repo => repo.GetCategoryByName(VALID_CATEGORY_NAME), Times.Once);
        }

        [Fact]
        public void GetCategoryByName_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var mockRepo = new Mock<ICategoryRepository>();
            var service = new CategoryService(mockRepo.Object);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => service.GetCategoryByName(string.Empty));
        }

        [Fact]
        public void GetCategoryNames_ReturnsCorrectList()
        {
            // Arrange
            var mockRepo = new Mock<ICategoryRepository>();
            var list = new List<Category>
            {
                new Category(1, "Category1"),
                new Category(2, "Category2")
            };
            
            mockRepo.Setup(x => x.GetCategories(It.IsAny<SqlParameter[]>()))
                   .Returns(list);
            var service = new CategoryService(mockRepo.Object);

            // Act
            var result = service.GetCategoryNames();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("Category1", result);
            Assert.Contains("Category2", result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsValidCategoryName_NullOrEmpty_ReturnsFalse(string name)
        {
            // Arrange
            var mockRepo = new Mock<ICategoryRepository>();
            var service = new CategoryService(mockRepo.Object);

            // Act
            var result = service.IsValidCategoryName(name);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidCategoryName_ExistingCategory_ReturnsTrue()
        {
            // Arrange
            var mockRepo = new Mock<ICategoryRepository>();
            var category = new Category(1, "TestCategory");
            mockRepo.Setup(x => x.GetCategoryByName("TestCategory"))
                   .Returns(category);
            var service = new CategoryService(mockRepo.Object);

            // Act
            var result = service.IsValidCategoryName("TestCategory");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidCategoryName_NonExistentCategory_ReturnsFalse()
        {
            // Arrange
            var mockRepo = new Mock<ICategoryRepository>();
            mockRepo.Setup(x => x.GetCategoryByName("NonExistent"))
                   .Returns((Category)null);
            var service = new CategoryService(mockRepo.Object);

            // Act
            var result = service.IsValidCategoryName("NonExistent");

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}