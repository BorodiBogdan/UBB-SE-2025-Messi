using System;
using Xunit;
using Duo.Models;

namespace TestProject1.Models
{
    public class CommentTests
    {

        [Fact]
        public void LikeCount_Getter_ReturnsCorrectValue()
        {
            // Arrange
            int expectedLikeCount = 5;
            var comment = new Comment(1, "Test", 1, 1, null, DateTime.Now, expectedLikeCount, 1);

            // Act
            var actualLikeCount = comment.LikeCount;

            // Assert
            Assert.Equal(expectedLikeCount, actualLikeCount);
        }

        [Fact]
        public void CreatedAt_Getter_ReturnsCorrectValue()
        {
            // Arrange
            DateTime expectedCreatedAt = DateTime.Now;
            var comment = new Comment(1, "Test", 1, 1, null, expectedCreatedAt, 0, 1);

            // Act
            var actualCreatedAt = comment.CreatedAt;

            // Assert
            Assert.Equal(expectedCreatedAt, actualCreatedAt);
        }
        [Fact]
        public void TestIncrementLikeCount_IncrementsLikeCount()
        {
            // Arrange
            var comment = new Comment(1, "Test", 1, 1, null, DateTime.Now, 0, 1);
            int expectedLikeCount = comment.LikeCount + 1;
            // Act
            comment.IncrementLikeCount();
            // Assert
            Assert.Equal(expectedLikeCount, comment.LikeCount);
        }
    }
} 