using Xunit;
using Duo.Models;

namespace TestProject1.Models
{
    public class CategoryTests
    {
        [Fact]
        public void Constructor_WithIdAndName_PropertiesAreSet()
        {
            // Arrange
            int expectedId = 1;
            string expectedName = "Technology";

            // Act
            var category = new Category(expectedId, expectedName);

            // Assert
            Assert.Equal(expectedId, category.Id);
            Assert.Equal(expectedName, category.Name);
        }

        [Fact]
        public void Properties_CanBeModified()
        {
            // Arrange
            var category = new Category(1, "OriginalName");
            int newId = 2;
            string newName = "UpdatedName";

            // Act
            category.Id = newId;
            category.Name = newName;

            // Assert
            Assert.Equal(newId, category.Id);
            Assert.Equal(newName, category.Name);
        }

        [Fact]
        public void Name_WhenSetToNull_IsStillNull()
        {
            // Arrange
            var category = new Category(1, "TestCategory");
            
            // Act
            category.Name = null;
            
            // Assert
            Assert.Null(category.Name);
        }

        [Fact]
        public void Name_WhenSetToEmptyString_IsEmptyString()
        {
            // Arrange
            var category = new Category(1, "TestCategory");
            
            // Act
            category.Name = string.Empty;
            
            // Assert
            Assert.Equal(string.Empty, category.Name);
        }
    }
} 