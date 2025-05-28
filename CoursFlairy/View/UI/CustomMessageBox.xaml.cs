using System.ComponentModel;
using System.Windows;

namespace CoursFlairy.View.UI
{
    public partial class CustomMessageBox : Window, INotifyPropertyChanged
    {
        private string _message;
        private bool? _dialogResult;

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public CustomMessageBox(string message, bool showConfirmation = false)
        {
            InitializeComponent();
            DataContext = this;
            Message = message;
            
            if (showConfirmation)
            {
                ButtonPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ButtonPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _dialogResult = false;
            Close();
        }

        public new bool? ShowDialog()
        {
            base.ShowDialog();
            return _dialogResult;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 