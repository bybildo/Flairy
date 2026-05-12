using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CoursFlairy.ViewModel.UI
{
    class PeoplePickerPlusButtonColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int.TryParse(value as string, out int num);
            return (num < 99) ? ResourceColor.MainColor100 : ResourceColor.MainColor10;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
