using System;
using System.ComponentModel;
using CoursFlairy.View.UI;

namespace CoursFlairy.Model
{
    public class PassengerInfo : INotifyPropertyChanged
    {
        private int _passengerNumber;
        public int PassengerNumber
        {
            get => _passengerNumber;
            set
            {
                _passengerNumber = value;
                OnPropertyChanged(nameof(PassengerNumber));
            }
        }

        private string _passengerClass;
        public string PassengerClass
        {
            get => _passengerClass;
            set
            {
                _passengerClass = value;
                OnPropertyChanged(nameof(PassengerClass));
            }
        }

        private decimal _currentPrice;
        public decimal CurrentPrice
        {
            get => _currentPrice;
            set
            {
                _currentPrice = value;
                OnPropertyChanged(nameof(CurrentPrice));
                OnPropertyChanged(nameof(FormattedPrice));
            }
        }

        public string FormattedPrice => $"{CurrentPrice:C}";

        private UserPassportData _passportData;
        public UserPassportData PassportData
        {
            get => _passportData;
            set
            {
                _passportData = value;
                OnPropertyChanged(nameof(PassportData));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 