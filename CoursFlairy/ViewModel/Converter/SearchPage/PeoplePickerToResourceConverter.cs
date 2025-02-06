using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CoursFlairy.ViewModel.Converter.SearchPage
{
    class PeoplePickerToResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string resourceKey)
            {
                return Application.Current.TryFindResource(resourceKey);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
