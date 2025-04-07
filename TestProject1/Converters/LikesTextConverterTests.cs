using Duo.Converters;
using System;
using Xunit;

namespace TestProject1.Converters
{
    public class LikesTextConverterTests
    {
        private readonly LikesTextConverter _converter;

        // Test constants
        private const int ZERO_LIKES = 0;
        private const int SINGLE_LIKE = 1;
        private const int MULTIPLE_LIKES = 42;
        private const string NON_INTEGER_VALUE = "not an integer";
        
        // Expected format strings
        private const string ZERO_LIKES_TEXT = "0 likes";
        private const string SINGLE_LIKE_TEXT = "1 likes";
        private const string MULTIPLE_LIKES_TEXT = "42 likes";

        public LikesTextConverterTests()
        {
            _converter = new LikesTextConverter();
        }

        [Theory]
        [InlineData(ZERO_LIKES, ZERO_LIKES_TEXT)]
        [InlineData(SINGLE_LIKE, SINGLE_LIKE_TEXT)]
        [InlineData(MULTIPLE_LIKES, MULTIPLE_LIKES_TEXT)]
        public void Convert_WhenInteger_ReturnsFormattedString(int likeCount, string expectedText)
        {
            // Act
            var formattedResult = _converter.Convert(likeCount, typeof(string), null, null);

            // Assert
            Assert.Equal(expectedText, formattedResult);
        }

        [Fact]
        public void Convert_WhenNonIntegerValue_ReturnsZeroLikes()
        {
            // Act
            var formattedResult = _converter.Convert(NON_INTEGER_VALUE, typeof(string), null, null);

            // Assert
            Assert.Equal(ZERO_LIKES_TEXT, formattedResult);
        }

        [Fact]
        public void ConvertBack_ThrowsNotImplementedException()
        {
            // Act & Assert
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack(null, typeof(int), null, null));
        }
    }
} 