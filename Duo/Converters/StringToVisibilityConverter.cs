using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Duo.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string inputString)
            {
                return string.IsNullOrWhiteSpace(inputString) ? Visibility.Collapsed : Visibility.Visible;
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 