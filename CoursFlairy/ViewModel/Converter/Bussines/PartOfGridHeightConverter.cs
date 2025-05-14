using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;

namespace CoursFlairy.ViewModel.Converter.Bussines
{
    class PartOfGridHeightConverter : IValueConverter
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
