using System.Globalization;
using System.Windows.Data;

namespace CoursFlairy.ViewModel.UI
{
    class DatePickerBorderMonthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value + 10;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
