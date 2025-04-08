using Duo.Data;
using Duo.Models;
using Duo.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace TestProject1.Repositories
{
    public class CategoryRepositoryTests
    {
        private MockDatabaseConnectionCategoryRepository _mockDatabase;
        private CategoryRepository _categoryRepository;

        public CategoryRepositoryTests()
        {
            // Use the custom mock implementation instead of Moq
            _mockDatabase = new MockDatabaseConnectionCategoryRepository();
            
            // Initialize repository with mock
            _categoryRepository = new CategoryRepository(_mockDatabase);
        }

        [Fact]
        public void Constructor_WithNullConnection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CategoryRepository(null));
        }

        [Fact]
        public void GetCategories_ReturnsListOfCategories()
        {
            // Act
            var result = _categoryRepository.GetCategories();
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("Technology", result[0].Name);
            Assert.Equal(2, result[1].Id);
            Assert.Equal("Science", result[1].Name);
            Assert.Equal(3, result[2].Id);
            Assert.Equal("Music", result[2].Name);
        }
        
        [Fact]
        public void GetCategories_DatabaseException_LogsErrorAndReturnsEmptyList()
        {
            // Create a database exception test parameter
            var exceptionTestParam = new SqlParameter("DatabaseExceptionTest", true);
            
            // Act
            var result = _categoryRepository.GetCategories(new SqlParameter[] { exceptionTestParam });
            
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetCategoryByName_WithValidName_ReturnsCategoryObject()
        {
            // Arrange
            string categoryName = "Technology";
            
            // Act
            var result = _categoryRepository.GetCategoryByName(categoryName);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(categoryName, result.Name);
        }

        [Fact]
        public void GetCategoryByName_WithNonExistentCategory_ThrowsException()
        {
            // Arrange
            string categoryName = "NonExistentCategory";
            
            // Act & Assert
            Assert.Throws<Exception>(() => _categoryRepository.GetCategoryByName(categoryName));
        }

        [Fact]
        public void MockDatabaseConnectionCategoryRepository_CloseConnection_DoesNotThrow()
        {
            // Arrange
            var mockConnection = new MockDatabaseConnectionCategoryRepository();
            
            // Act & Assert - no exception should be thrown
            mockConnection.CloseConnection();
            Assert.True(true);
        }
        
        [Fact]
        public void MockDatabaseConnectionCategoryRepository_OpenConnection_DoesNotThrow()
        {
            // Arrange
            var mockConnection = new MockDatabaseConnectionCategoryRepository();
            
            // Act & Assert - no exception should be thrown
            mockConnection.OpenConnection();
            Assert.True(true);
        }
        
        [Fact]
        public void MockDatabaseConnectionCategoryRepository_ExecuteNonQuery_ThrowsNotImplementedException()
        {
            // Arrange
            var mockConnection = new MockDatabaseConnectionCategoryRepository();
            
            // Act & Assert
            Assert.Throws<NotImplementedException>(() => mockConnection.ExecuteNonQuery("AnyProcedure"));
        }
        
        [Fact]
        public void MockDatabaseConnectionCategoryRepository_ExecuteReader_GetCategoryByName_NullParameters_ThrowsException()
        {
            // Arrange
            var mockConnection = new MockDatabaseConnectionCategoryRepository();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mockConnection.ExecuteReader("GetCategoryByName", null));
        }
        
        [Fact]
        public void MockDatabaseConnectionCategoryRepository_ExecuteReader_GetCategoryByName_EmptyParameters_ThrowsException()
        {
            // Arrange
            var mockConnection = new MockDatabaseConnectionCategoryRepository();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mockConnection.ExecuteReader("GetCategoryByName", new SqlParameter[] { }));
        }
        
        [Fact]
        public void MockDatabaseConnectionCategoryRepository_ExecuteReader_GetCategoryByName_EmptyName_ThrowsException()
        {
            // Arrange
            var mockConnection = new MockDatabaseConnectionCategoryRepository();
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", "")
            };
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => mockConnection.ExecuteReader("GetCategoryByName", parameters));
        }
        
        [Fact]
        public void MockDatabaseConnectionCategoryRepository_ExecuteReader_GetCategoryByName_NoMatchingFilters_ReturnsEmptyTable()
        {
            // Arrange
            var mockConnection = new MockDatabaseConnectionCategoryRepository();
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Name", "UnknownCategory") // Not in predefined conditions
            };
            
            // Act
            var result = mockConnection.ExecuteReader("GetCategoryByName", parameters);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Rows.Count);
        }
        
        [Fact]
        public void MockDatabaseConnectionCategoryRepository_ExecuteReader_UnsupportedProcedure_ThrowsNotImplementedException()
        {
            // Arrange
            var mockConnection = new MockDatabaseConnectionCategoryRepository();
            
            // Act & Assert
            Assert.Throws<NotImplementedException>(() => mockConnection.ExecuteReader("UnsupportedProcedure"));
        }
        
        [Fact]
        public void MockDatabaseConnectionCategoryRepository_ExecuteScalar_ThrowsNotImplementedException()
        {
            // Arrange
            var mockConnection = new MockDatabaseConnectionCategoryRepository();
            
            // Act & Assert
            Assert.Throws<NotImplementedException>(() => mockConnection.ExecuteScalar<int>("AnyProcedure"));
        }
    }
} 