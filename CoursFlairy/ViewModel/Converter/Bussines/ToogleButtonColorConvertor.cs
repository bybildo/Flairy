using CoursFlairy.View.UI;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CoursFlairy.ViewModel.Converter.Bussines
{
    public class ToogleButtonColorConvertor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return Brushes.Transparent;

            var height1 = (GridLength)values[0];
            var height2 = (GridLength)values[1];

            if (height1.Value < height2.Value)
                return Brushes.LimeGreen;
            return Brushes.OrangeRed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
