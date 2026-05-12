using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CoursFlairy.ViewModel.Converter.SearchPage
{
    class PeoplePickerToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                if (text[0] == 'b') return new Thickness(1, 5, 7, -1);
                if (text[0] == 'c') return new Thickness(0, 5, 7, 0);
            }
            return new Thickness(0, 0, 7, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
