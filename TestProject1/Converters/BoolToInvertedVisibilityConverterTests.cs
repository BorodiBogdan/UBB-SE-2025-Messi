using Microsoft.UI.Xaml;
using Duo.Converters;
using System;
using Xunit;

namespace TestMessi.Converters
{
    public class BoolToInvertedVisibilityConverterTests
    {
        private readonly BoolToInvertedVisibilityConverter _converter;
        private const string VALID_STRING = "hello";
        private const int NOT_BOOL_OR_STRING = 123;

        public BoolToInvertedVisibilityConverterTests()
        {
            _converter = new BoolToInvertedVisibilityConverter();
        }

        [Fact]
        public void Convert_BoolFalse_ReturnsVisible()
        {
            var result = _converter.Convert(false, null, null, null);
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void Convert_BoolTrue_ReturnsCollapsed()
        {
            var result = _converter.Convert(true, null, null, null);
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_StringEmpty_ReturnsCollapsed()
        {
            var result = _converter.Convert("", null, null, null);
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_StringNonEmpty_ReturnsVisible()
        {
            var result = _converter.Convert(VALID_STRING, null, null, null);
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void Convert_InvalidType_ReturnsCollapsed()
        {
            var result = _converter.Convert(NOT_BOOL_OR_STRING, null, null, null); // not bool or string
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void ConvertBack_ToBool_Collapsed_ReturnsTrue()
        {
            var result = _converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, null);
            Assert.True((bool)result);
        }

        [Fact]
        public void ConvertBack_ToBool_Visible_ReturnsFalse()
        {
            var result = _converter.ConvertBack(Visibility.Visible, typeof(bool), null, null);
            Assert.False((bool)result);
        }

        [Fact]
        public void ConvertBack_ToString_Visible_ReturnsTrueString()
        {
            var result = _converter.ConvertBack(Visibility.Visible, typeof(string), null, null);
            Assert.Equal("True", result);
        }

        [Fact]
        public void ConvertBack_ToString_Collapsed_ReturnsEmptyString()
        {
            var result = _converter.ConvertBack(Visibility.Collapsed, typeof(string), null, null);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ConvertBack_InvalidTargetType_ReturnsNull()
        {
            var result = _converter.ConvertBack(Visibility.Visible, typeof(int), null, null);
            Assert.Null(result);
        }

    }
}
