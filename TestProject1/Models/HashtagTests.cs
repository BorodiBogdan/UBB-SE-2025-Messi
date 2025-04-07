using Xunit;
using Duo.Models;

namespace TestProject1.Models
{
    public class HashtagTests
    {
        [Fact]
        public void Constructor_WithIdAndTag_PropertiesAreSet()
        {
            // Arrange
            int expectedId = 1;
            string expectedTag = "Technology";

            // Act
            var hashtag = new Hashtag(expectedId, expectedTag);

            // Assert
            Assert.Equal(expectedId, hashtag.Id);
            Assert.Equal(expectedTag, hashtag.Tag);
            Assert.Equal(expectedTag, hashtag.Name); // Name property returns Tag value
        }

        [Fact]
        public void Id_CanBeModified()
        {
            // Arrange
            var hashtag = new Hashtag(1, "OriginalTag");
            int newId = 2;

            // Act
            hashtag.Id = newId;

            // Assert
            Assert.Equal(newId, hashtag.Id);
        }

        [Fact]
        public void Tag_CanBeModified()
        {
            // Arrange
            var hashtag = new Hashtag(1, "OriginalTag");
            string newTag = "UpdatedTag";

            // Act
            hashtag.Tag = newTag;

            // Assert
            Assert.Equal(newTag, hashtag.Tag);
            Assert.Equal(newTag, hashtag.Name); // Name property reflects Tag changes
        }

        [Fact]
        public void Name_CanBeModified()
        {
            // Arrange
            var hashtag = new Hashtag(1, "OriginalTag");
            string newName = "UpdatedName";

            // Act
            hashtag.Name = newName;

            // Assert
            Assert.Equal(newName, hashtag.Name);
            Assert.Equal(newName, hashtag.Tag); // Tag property reflects Name changes
        }

        [Fact]
        public void Tag_WhenSetToNull_IsStillNull()
        {
            // Arrange
            var hashtag = new Hashtag(1, "TestTag");
            
            // Act
            hashtag.Tag = null;
            
            // Assert
            Assert.Null(hashtag.Tag);
            Assert.Null(hashtag.Name);
        }

        [Fact]
        public void Tag_WhenSetToEmptyString_IsEmptyString()
        {
            // Arrange
            var hashtag = new Hashtag(1, "TestTag");
            
            // Act
            hashtag.Tag = string.Empty;
            
            // Assert
            Assert.Equal(string.Empty, hashtag.Tag);
            Assert.Equal(string.Empty, hashtag.Name);
        }

        [Fact]
        public void Name_WhenSetToNull_IsStillNull()
        {
            // Arrange
            var hashtag = new Hashtag(1, "TestTag");
            
            // Act
            hashtag.Name = null;
            
            // Assert
            Assert.Null(hashtag.Name);
            Assert.Null(hashtag.Tag);
        }

        [Fact]
        public void Name_WhenSetToEmptyString_IsEmptyString()
        {
            // Arrange
            var hashtag = new Hashtag(1, "TestTag");
            
            // Act
            hashtag.Name = string.Empty;
            
            // Assert
            Assert.Equal(string.Empty, hashtag.Name);
            Assert.Equal(string.Empty, hashtag.Tag);
        }

        [Fact]
        public void TagAndName_AreAlwaysSynchronized()
        {
            // Arrange
            var hashtag = new Hashtag(1, "InitialTag");

            // Act & Assert
            // Test Tag changes reflect in Name
            hashtag.Tag = "NewTag";
            Assert.Equal("NewTag", hashtag.Name);

            // Test Name changes reflect in Tag
            hashtag.Name = "AnotherTag";
            Assert.Equal("AnotherTag", hashtag.Tag);

            // Test null values
            hashtag.Tag = null;
            Assert.Null(hashtag.Name);

            hashtag.Name = "RestoredTag";
            Assert.Equal("RestoredTag", hashtag.Tag);
        }
    }
}