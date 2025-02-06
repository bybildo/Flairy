using System.Windows;
using System.Windows.Media;

namespace CoursFlairy.ViewModel
{
    static class ResourceColor
    {
        public static readonly SolidColorBrush Black = (SolidColorBrush)Application.Current.Resources["Black"];
        public static readonly SolidColorBrush White = (SolidColorBrush)Application.Current.Resources["White"];
        public static readonly SolidColorBrush MainColor100 = (SolidColorBrush)Application.Current.Resources["MainColor100"];
        public static readonly SolidColorBrush MainColor50 = (SolidColorBrush)Application.Current.Resources["MainColor50"];
        public static readonly SolidColorBrush MainColor30 = (SolidColorBrush)Application.Current.Resources["MainColor30"];
        public static readonly SolidColorBrush MainColor20 = (SolidColorBrush)Application.Current.Resources["MainColor20"];
        public static readonly SolidColorBrush MainColor10 = (SolidColorBrush)Application.Current.Resources["MainColor10"];
        public static readonly SolidColorBrush HighlightingSelectionMainWindow = (SolidColorBrush)Application.Current.Resources["HighlightingSelectionMainWindow"];
        public static readonly SolidColorBrush HintColor = (SolidColorBrush)Application.Current.Resources["HintColor"];
        public static readonly SolidColorBrush DatePickerGray = (SolidColorBrush)Application.Current.Resources["DatePickerGray"];
        public static readonly SolidColorBrush DatePickerAdditionalBackground = (SolidColorBrush)Application.Current.Resources["DatePickerAdditionalBackground"];
    }
}
