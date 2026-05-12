using System;
using System.ComponentModel;
using CoursFlairy.Model.Enum;

namespace CoursFlairy.Model
{
    public class TicketInfo : INotifyPropertyChanged
    {
        private int _ticketId;
        private string _passengerName;
        private string _passengerSurname;
        private string _departureCity;
        private string _arrivalCity;
        private string _departureIcao;
        private string _arrivalIcao;
        private DateTime _departureTime;
        private DateTime _arrivalTime;
        private string _seat;
        private string _seatCode;
        private Classes _class;
        private decimal _price;
        private string _airlineName;
        private bool _baggage;
        private string _gate;
        private DateTime _addDate;
        private string _passport;

        public int TicketId 
        { 
            get => _ticketId; 
            set { _ticketId = value; OnPropertyChanged(nameof(TicketId)); } 
        }

        public string PassengerName 
        { 
            get => _passengerName; 
            set { _passengerName = value; OnPropertyChanged(nameof(PassengerName)); OnPropertyChanged(nameof(FullName)); } 
        }

        public string PassengerSurname 
        { 
            get => _passengerSurname; 
            set { _passengerSurname = value; OnPropertyChanged(nameof(PassengerSurname)); OnPropertyChanged(nameof(FullName)); } 
        }

        public string FullName => $"{PassengerName} {PassengerSurname}";

        public string DepartureCity 
        { 
            get => _departureCity; 
            set { _departureCity = value; OnPropertyChanged(nameof(DepartureCity)); } 
        }

        public string ArrivalCity 
        { 
            get => _arrivalCity; 
            set { _arrivalCity = value; OnPropertyChanged(nameof(ArrivalCity)); } 
        }

        public string DepartureIcao 
        { 
            get => _departureIcao; 
            set { _departureIcao = value; OnPropertyChanged(nameof(DepartureIcao)); } 
        }

        public string ArrivalIcao 
        { 
            get => _arrivalIcao; 
            set { _arrivalIcao = value; OnPropertyChanged(nameof(ArrivalIcao)); } 
        }

        public DateTime DepartureTime 
        { 
            get => _departureTime; 
            set { _departureTime = value; OnPropertyChanged(nameof(DepartureTime)); OnPropertyChanged(nameof(DepartureTimeString)); OnPropertyChanged(nameof(DepartureDateString)); } 
        }

        public DateTime ArrivalTime 
        { 
            get => _arrivalTime; 
            set { _arrivalTime = value; OnPropertyChanged(nameof(ArrivalTime)); OnPropertyChanged(nameof(ArrivalTimeString)); OnPropertyChanged(nameof(ArrivalDateString)); } 
        }

        public string DepartureTimeString => DepartureTime.ToString("HH:mm");
        public string DepartureDateString => DepartureTime.ToString("dd/MM/yyyy");
        public string ArrivalTimeString => ArrivalTime.ToString("HH:mm");
        public string ArrivalDateString => ArrivalTime.ToString("dd/MM/yyyy");

        public string Seat 
        { 
            get => _seat; 
            set { _seat = value; OnPropertyChanged(nameof(Seat)); } 
        }

        public string SeatCode 
        { 
            get => _seatCode; 
            set { _seatCode = value; OnPropertyChanged(nameof(SeatCode)); } 
        }

        public Classes Class 
        { 
            get => _class; 
            set { _class = value; OnPropertyChanged(nameof(Class)); OnPropertyChanged(nameof(ClassDisplay)); } 
        }

        public string ClassDisplay 
        { 
            get 
            {
                return Class switch
                {
                    Classes.Econom => "Economy",
                    Classes.Bussiness => "Business",
                    Classes.First => "First Class",
                    _ => "Economy"
                };
            }
        }

        public decimal Price 
        { 
            get => _price; 
            set { _price = value; OnPropertyChanged(nameof(Price)); OnPropertyChanged(nameof(PriceDisplay)); } 
        }

        public string PriceDisplay => $"{Price:F2} ₴";

        public string AirlineName 
        { 
            get => _airlineName; 
            set { _airlineName = value; OnPropertyChanged(nameof(AirlineName)); } 
        }

        public string Gate 
        { 
            get => _gate; 
            set { _gate = value; OnPropertyChanged(nameof(Gate)); } 
        }

        public DateTime AddDate 
        { 
            get => _addDate; 
            set { _addDate = value; OnPropertyChanged(nameof(AddDate)); } 
        }

        public string Passport 
        { 
            get => _passport; 
            set { _passport = value; OnPropertyChanged(nameof(Passport)); } 
        }

        public string FlightNumber => $"FL {TicketId:D4}";

        public bool Baggage 
        { 
            get => _baggage; 
            set { _baggage = value; OnPropertyChanged(nameof(Baggage)); OnPropertyChanged(nameof(BaggageDisplay)); } 
        }

        public string BaggageDisplay => Baggage ? "Included" : "Not included";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 