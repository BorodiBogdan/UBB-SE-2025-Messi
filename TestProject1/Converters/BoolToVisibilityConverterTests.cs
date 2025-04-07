using Microsoft.UI.Xaml;
using Duo.Converters;
using System;
using Xunit;

namespace TestMessi.Converters
{
    public class BoolToVisibilityConverterTests
    {
        private readonly BoolToVisibilityConverter _converter;
        private const string NON_VALID_STRING = "invalid input";

        public BoolToVisibilityConverterTests()
        {
            _converter = new BoolToVisibilityConverter();
        }


        [Fact]
        public void Convert_True_ReturnsVisible()
        {
            // Arrange
            var converter = new BoolToVisibilityConverter();
            var value = true;
            // Act
            var result = converter.Convert(value, null, null, null);
            // Assert
            Assert.Equal(Visibility.Visible, result);
        }
        [Fact]
        public void Convert_False_ReturnsCollapsed()
        {
            // Arrange
            var converter = new BoolToVisibilityConverter();
            var value = false;
            // Act
            var result = converter.Convert(value, null, null, null);
            // Assert
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void ConvertBack_Collapsed_ReturnsFalse()
        {
            // Arrange
            var value = Visibility.Collapsed;
            // Act
            var result = _converter.ConvertBack(value, null, null, null);
            // Assert
            Assert.False((bool)result);
        }

        [Fact]
        public void ConvertBack_Visible_ReturnsTrue()
        {
            // Arrange
            var value = Visibility.Visible;

            // Act
            var result = _converter.ConvertBack(value, null, null, null);

            // Assert
            Assert.True((bool)result);
        }

        [Fact]
        public void ConvertBack_InvalidType_ReturnsFalse()
        {
            // Act
            var result = _converter.ConvertBack(NON_VALID_STRING, null, null, null);

            // Assert
            Assert.False((bool)result);
        }
    }
}
