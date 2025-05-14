using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using static CoursFlairy.ViewModel.ResourceColor;

namespace CoursFlairy.ViewModel.Converter.SearchPage
{
    class PeoplePickerToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            char lastsymbol = '1';
            if (value is string text)
            {
                lastsymbol = text.Last();
            }

            switch (lastsymbol)
            {
                default:
                    return EconomColor;
                case '2':
                    return BusinessColor;
                case '3':
                    return FirstColor;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
