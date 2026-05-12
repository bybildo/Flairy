using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace CoursFlairy.ViewModel.UI
{ 
    class PeoplePickerRightBorderRoundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new CornerRadius(0, (double)value / 2, (double)value / 2, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
