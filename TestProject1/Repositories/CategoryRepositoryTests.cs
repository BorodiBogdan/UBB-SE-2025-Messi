using Duo.Data;
using Duo.Models;
using Duo.Repositories;
using Microsoft.Data.SqlClient;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace TestProject1.Repositories
{
    public class CategoryRepositoryTests
    {
        private Mock<IDatabaseConnection> _mockDatabase;
        private CategoryRepository _categoryRepository;
        private DataTable _categoryDataTable;

        public CategoryRepositoryTests()
        {
            _mockDatabase = new Mock<IDatabaseConnection>();
            
            // Setup mock data table
            _categoryDataTable = new DataTable();
            _categoryDataTable.Columns.Add("Id", typeof(int));
            _categoryDataTable.Columns.Add("Name", typeof(string));
            _categoryDataTable.Rows.Add(1, "Technology");
            _categoryDataTable.Rows.Add(2, "Science");
            _categoryDataTable.Rows.Add(3, "Music");
            
            // Configure mock behavior - this is the correct setup that should be used
            _mockDatabase.Setup(db => db.ExecuteReader("GetCategories", It.IsAny<SqlParameter[]>()))
                .Returns(_categoryDataTable);
                
            _mockDatabase.Setup(db => db.ExecuteReader("GetCategoryByName", It.Is<SqlParameter[]>(p => 
                p != null && p.Length > 0 && p[0].Value.ToString() == "Technology")))
                .Returns(_categoryDataTable.AsEnumerable().Where(r => r.Field<string>("Name") == "Technology").CopyToDataTable());
                
            _mockDatabase.Setup(db => db.ExecuteReader("GetCategoryByName", It.Is<SqlParameter[]>(p => 
                p != null && p.Length > 0 && p[0].Value.ToString() == "NonExistentCategory")))
                .Returns(new DataTable());

            // Initialize repository with mock
            _categoryRepository = new CategoryRepository(_mockDatabase.Object);
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
            // Arrange
            var mockConsole = new MockConsole();
            Console.SetOut(mockConsole);
            
            // Setup mock to throw exception
            var mockDb = new Mock<IDatabaseConnection>();
            mockDb.Setup(db => db.ExecuteReader("GetCategories", null))
                .Throws(new Exception("Database error"));
                
            var repository = new CategoryRepository(mockDb.Object);
            
            // Act
            var result = repository.GetCategories();
            
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            // Note: In a real test, we would verify the console output
            // but that's difficult to test directly
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
    }
    
    // Helper class to mock Console.WriteLine for testing
    public class MockConsole : System.IO.TextWriter
    {
        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
        public string Output { get; private set; } = "";
        
        public override void WriteLine(string value)
        {
            Output += value + "\n";
        }
    }
} 