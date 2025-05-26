using System.Globalization;
using System.Windows.Data;

namespace CoursFlairy.ViewModel.Converter.User
{
    public class PartOfGridHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double gridHeight)
            {
                return gridHeight / 14;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 