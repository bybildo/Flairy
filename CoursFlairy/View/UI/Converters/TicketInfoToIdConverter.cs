using System;
using System.Globalization;
using System.Windows.Data;
using CoursFlairy.Model;

namespace CoursFlairy.View.UI.Converters
{
    public class TicketInfoToIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TicketInfo ticketInfo)
            {
                return ticketInfo.TicketId;
            }
            if (value is int id)
            {
                return id;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // We don't need convert back for this case
            throw new NotImplementedException();
        }
    }
} 