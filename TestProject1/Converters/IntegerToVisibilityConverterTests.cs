using Microsoft.UI.Xaml;
using Duo.Converters;
using System;
using Xunit;

namespace TestProject1.Converters
{
    public class IntegerToVisibilityConverterTests
    {
        private readonly IntegerToVisibilityConverter _converter;

        // Test constants
        private const int POSITIVE_VALUE = 1;
        private const int LARGE_POSITIVE_VALUE = 100;
        private const int ZERO_VALUE = 0;
        private const int NEGATIVE_VALUE = -1;
        private const string NON_INTEGER_VALUE = "not an integer";

        public IntegerToVisibilityConverterTests()
        {
            _converter = new IntegerToVisibilityConverter();
        }

        [Theory]
        [InlineData(POSITIVE_VALUE)]
        [InlineData(LARGE_POSITIVE_VALUE)]
        public void Convert_WhenPositiveInteger_ReturnsVisible(int inputValue)
        {
            // Act
            var visibilityResult = _converter.Convert(inputValue, typeof(Visibility), null, null);

            // Assert
            Assert.Equal(Visibility.Visible, visibilityResult);
        }

        [Theory]
        [InlineData(ZERO_VALUE)]
        [InlineData(NEGATIVE_VALUE)]
        public void Convert_WhenZeroOrNegativeInteger_ReturnsCollapsed(int inputValue)
        {
            // Act
            var visibilityResult = _converter.Convert(inputValue, typeof(Visibility), null, null);

            // Assert
            Assert.Equal(Visibility.Collapsed, visibilityResult);
        }

        [Fact]
        public void Convert_WhenNonIntegerValue_ReturnsCollapsed()
        {
            // Act
            var visibilityResult = _converter.Convert(NON_INTEGER_VALUE, typeof(Visibility), null, null);

            // Assert
            Assert.Equal(Visibility.Collapsed, visibilityResult);
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