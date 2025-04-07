using Microsoft.UI.Xaml;
using Duo.Converters;
using System;
using Xunit;

namespace TestProject1.Converters
{
    public class StringToVisibilityConverterTests
    {
        private readonly StringToVisibilityConverter _converter;

        // Test constants
        private const string NON_EMPTY_STRING = "test";
        private const string PADDED_STRING = " test ";
        private const string SINGLE_CHAR_STRING = "a";
        private const string EMPTY_STRING = "";
        private const string SINGLE_SPACE = " ";
        private const string MULTIPLE_SPACES = "   ";
        private const int NON_STRING_VALUE = 42;

        public StringToVisibilityConverterTests()
        {
            _converter = new StringToVisibilityConverter();
        }

        [Theory]
        [InlineData(NON_EMPTY_STRING)]
        [InlineData(PADDED_STRING)]
        [InlineData(SINGLE_CHAR_STRING)]
        public void Convert_WhenNonEmptyString_ReturnsVisible(string inputString)
        {
            // Act
            var visibilityResult = _converter.Convert(inputString, typeof(Visibility), null, null);

            // Assert
            Assert.Equal(Visibility.Visible, visibilityResult);
        }

        [Theory]
        [InlineData(EMPTY_STRING)]
        [InlineData(SINGLE_SPACE)]
        [InlineData(null)]
        [InlineData(MULTIPLE_SPACES)]
        public void Convert_WhenEmptyOrWhitespaceString_ReturnsCollapsed(string inputString)
        {
            // Act
            var visibilityResult = _converter.Convert(inputString, typeof(Visibility), null, null);

            // Assert
            Assert.Equal(Visibility.Collapsed, visibilityResult);
        }

        [Fact]
        public void Convert_WhenNonStringValue_ReturnsCollapsed()
        {
            // Act
            var visibilityResult = _converter.Convert(NON_STRING_VALUE, typeof(Visibility), null, null);

            // Assert
            Assert.Equal(Visibility.Collapsed, visibilityResult);
        }

        [Fact]
        public void ConvertBack_ThrowsNotImplementedException()
        {
            // Act & Assert
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack(null, typeof(string), null, null));
        }
    }
} 