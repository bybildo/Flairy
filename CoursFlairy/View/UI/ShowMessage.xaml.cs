using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static CoursFlairy.ViewModel.ResourceColor;

namespace CoursFlairy.View.UI
{
    public partial class ShowMessage : UserControl, INotifyPropertyChanged
    {
        private string _message = "";

        public string Message
        {
            get { return _message; }
            set { _message = value; OnPropertyChanged(nameof(Message)); }
        }

        public ShowMessage()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty BorderBackgroundProperty =
         DependencyProperty.Register(
             "BorderBackground",                      
             typeof(SolidColorBrush),                 
             typeof(ShowMessage),                     
             new PropertyMetadata(White));

        public SolidColorBrush BorderBackground
        {
            get { return (SolidColorBrush)GetValue(BorderBackgroundProperty); }
            set { SetValue(BorderBackgroundProperty, value); }
        }

        public void Show(string message, double displaySeconds = 4)
        {
            if (border.Visibility == System.Windows.Visibility.Visible) return;

            Message = message;

            border.Visibility = System.Windows.Visibility.Visible;
            var fadeInAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4)
            };
            border.BeginAnimation(Border.OpacityProperty, fadeInAnimation);

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(displaySeconds)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();

                var fadeOutAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.4)
                };

                fadeOutAnimation.Completed += (s2, e2) =>
                {
                    border.Visibility = System.Windows.Visibility.Collapsed;
                };

                border.BeginAnimation(Border.OpacityProperty, fadeOutAnimation);
            };
            timer.Start();
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
