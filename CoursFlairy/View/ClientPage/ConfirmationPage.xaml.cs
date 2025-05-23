using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;

namespace CoursFlairy.View.ClientPage
{
    public partial class ConfirmationPage : Page, INotifyPropertyChanged
    {
        private decimal _amount;
        private ICommand _returnHomeCommand;

        public ConfirmationPage(decimal amount)
        {
            InitializeComponent();
            Amount = amount;
            ReturnHomeCommand = new RelayCommand(async _ => 
            {
                ReturnHome();
                await Task.CompletedTask;
            });
            DataContext = this;
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        public ICommand ReturnHomeCommand
        {
            get => _returnHomeCommand;
            set
            {
                _returnHomeCommand = value;
                OnPropertyChanged(nameof(ReturnHomeCommand));
            }
        }

        private void ReturnHome()
        {
            // Navigate back to the main page or clear navigation stack
            while (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 