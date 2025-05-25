using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CoursFlairy.Model.Converters
{
    public class IntToVisibilityConverter : IValueConverter
    {
        public Visibility ZeroValue { get; set; } = Visibility.Collapsed;
        public Visibility NonZeroValue { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? ZeroValue : NonZeroValue;
            }
            return NonZeroValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 