using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Duo.Converters
{
    public class IntegerToVisibilityConverter : IValueConverter
    {
        // Constants for visibility threshold
        private const int VISIBILITY_THRESHOLD = 0;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Check if value is an integer
            if (value is int integerValue)
            {
                // Return Visible if value is greater than threshold, otherwise Collapsed
                return integerValue > VISIBILITY_THRESHOLD ? Visibility.Visible : Visibility.Collapsed;
            }
            
            // Return Collapsed for non-integer values
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 