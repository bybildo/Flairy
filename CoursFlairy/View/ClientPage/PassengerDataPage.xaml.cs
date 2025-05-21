using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoursFlairy.Data;
using CoursFlairy.Model.Enum;
using Microsoft.Data.SqlClient;
using CoursFlairy.View.UI;

namespace CoursFlairy.View.ClientPage
{
    /// <summary>
    /// Interaction logic for PassengerDataPage.xaml
    /// </summary>
    public partial class PassengerDataPage : Page, INotifyPropertyChanged
    {
        private int _flightId;
        private List<Classes> _passengerClasses;
        private ObservableCollection<PassengerInfo> _passengers;
        private decimal _economyPrice;
        private decimal _businessPrice;
        private decimal _firstClassPrice;
        private DateTime _departureTime;
        private DateTime _arrivalTime;
        private string _departureCity;
        private string _arrivalCity;
        private string _departureIcao;
        private string _arrivalIcao;

        public string DepartureCity { get => _departureCity; set { _departureCity = value; OnPropertyChanged(nameof(DepartureCity)); } }
        public string ArrivalCity { get => _arrivalCity; set { _arrivalCity = value; OnPropertyChanged(nameof(ArrivalCity)); } }
        public string DepartureIcao { get => _departureIcao; set { _departureIcao = value; OnPropertyChanged(nameof(DepartureIcao)); } }
        public string ArrivalIcao { get => _arrivalIcao; set { _arrivalIcao = value; OnPropertyChanged(nameof(ArrivalIcao)); } }
        public DateTime DepartureTime { get => _departureTime; set { _departureTime = value; OnPropertyChanged(nameof(DepartureTime)); } }
        public DateTime ArrivalTime { get => _arrivalTime; set { _arrivalTime = value; OnPropertyChanged(nameof(ArrivalTime)); } }

        public string DepartureTimeString => DepartureTime.ToString("HH:mm");
        public string ArrivalTimeString => ArrivalTime.ToString("HH:mm");
        public string DepartureDateString => DepartureTime.ToString("dd.MM.yyyy");
        public string ArrivalDateString => ArrivalTime.ToString("dd.MM.yyyy");

        public ObservableCollection<PassengerInfo> Passengers
        {
            get => _passengers;
            set
            {
                _passengers = value;
                OnPropertyChanged(nameof(Passengers));
            }
        }

        public PassengerDataPage(int flightId, List<Classes> passengerClasses)
        {
            InitializeComponent();
            _flightId = flightId;
            _passengerClasses = passengerClasses;
            DataContext = this;
            InitializePassengers();
            LoadFlightPrices();

            Loaded += (s, e) =>
            {
                var parent = FindParent<SelectPage>(this);
                if (parent != null)
                {
                    parent.flightId = flightId;
                    parent.passengerClasses = _passengerClasses;
                }
            };
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                child = VisualTreeHelper.GetParent(child);
                if (child is T parent)
                {
                    return parent;
                }
            }
            return null;
        }

        private void LoadFlightPrices()
        {
            try
            {
                using (var connection = Data.DataBase.GetConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT 
                            f.DTime,
                            DATEADD(MINUTE, r.AmountTime, f.DTime) as ArrivalTime,
                            depAir.City as DepCity,
                            arrAir.City as ArrCity,
                            depAir.ICAO as DepICAO,
                            arrAir.ICAO as ArrICAO,
                            pe.Adult as EconomyAdult,
                            pe.Child as EconomyChild,
                            pe.Baby as EconomyBaby,
                            pe.Pensioner as EconomyPensioner,
                            pb.Adult as BusinessAdult,
                            pb.Child as BusinessChild,
                            pb.Baby as BusinessBaby,
                            pb.Pensioner as BusinessPensioner,
                            pf.Adult as FirstAdult,
                            pf.Child as FirstChild,
                            pf.Baby as FirstBaby,
                            pf.Pensioner as FirstPensioner
                        FROM Flight f
                        JOIN Route r ON f.RouteID = r.ID
                        JOIN Airport depAir ON r.DepartureID = depAir.ID
                        JOIN Airport arrAir ON r.ArrivalID = arrAir.ID
                        JOIN Price pe ON r.ID = pe.RouteID AND pe.ClassGroup = 1
                        JOIN Price pb ON r.ID = pb.RouteID AND pb.ClassGroup = 2
                        JOIN Price pf ON r.ID = pf.RouteID AND pf.ClassGroup = 3
                        WHERE f.ID = @flightId";

                    command.Parameters.AddWithValue("@flightId", _flightId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DepartureTime = reader.GetDateTime(0);
                            ArrivalTime = reader.GetDateTime(1);
                            DepartureCity = reader.GetString(2);
                            ArrivalCity = reader.GetString(3);
                            DepartureIcao = reader.GetString(4);
                            ArrivalIcao = reader.GetString(5);

                            var prices = new Dictionary<Classes, PriceCategories>();
                            prices[Classes.Econom] = new PriceCategories(
                                reader.GetDecimal(6),  // Adult
                                reader.GetDecimal(7),  // Child
                                reader.GetDecimal(8),  // Baby
                                reader.GetDecimal(9)   // Pensioner
                            );
                            prices[Classes.Bussiness] = new PriceCategories(
                                reader.GetDecimal(10), // Adult
                                reader.GetDecimal(11), // Child
                                reader.GetDecimal(12), // Baby
                                reader.GetDecimal(13)  // Pensioner
                            );
                            prices[Classes.First] = new PriceCategories(
                                reader.GetDecimal(14), // Adult
                                reader.GetDecimal(15), // Child
                                reader.GetDecimal(16), // Baby
                                reader.GetDecimal(17)  // Pensioner
                            );

                            foreach (var passenger in Passengers)
                            {
                                if (prices.TryGetValue(passenger.PassengerClass, out var categoryPrices))
                                {
                                    passenger.SetPrices(categoryPrices);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження цін: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializePassengers()
        {
            Passengers = new ObservableCollection<PassengerInfo>();
            for (int i = 0; i < _passengerClasses.Count; i++)
            {
                var passenger = new PassengerInfo
                {
                    PassengerNumber = i + 1,
                    PassengerClass = _passengerClasses[i]
                };
                passenger.UserPassportDataChanged += Passenger_UserPassportDataChanged;
                Passengers.Add(passenger);
            }
        }

        private void Passenger_UserPassportDataChanged(object sender, EventArgs e)
        {
            if (sender is PassengerInfo passenger)
            {
                passenger.RecalculatePrice();
            }
        }

        private void UserPassportData_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is UserPassportData passportData && 
                passportData.DataContext is PassengerInfo passengerInfo)
            {
                passengerInfo.PassportData = passportData;
            }
        }

        public class PriceCategories
        {
            public decimal Adult { get; }
            public decimal Child { get; }
            public decimal Baby { get; }
            public decimal Pensioner { get; }

            public PriceCategories(decimal adult, decimal child, decimal baby, decimal pensioner)
            {
                Adult = adult;
                Child = child;
                Baby = baby;
                Pensioner = pensioner;
            }
        }

        public class PassengerInfo : INotifyPropertyChanged
        {
            private int _passengerNumber;
            private Classes _passengerClass;
            private DateTime? _birthDate;
            private decimal _currentPrice;
            private PriceCategories _prices;
            private UserPassportData _passportData;

            public event EventHandler UserPassportDataChanged;
            public event PropertyChangedEventHandler PropertyChanged;

            public int PassengerNumber
            {
                get => _passengerNumber;
                set
                {
                    _passengerNumber = value;
                    OnPropertyChanged(nameof(PassengerNumber));
                }
            }

            public Classes PassengerClass
            {
                get => _passengerClass;
                set
                {
                    _passengerClass = value;
                    OnPropertyChanged(nameof(PassengerClass));
                }
            }

            public decimal CurrentPrice
            {
                get => _currentPrice;
                private set
                {
                    _currentPrice = value;
                    OnPropertyChanged(nameof(CurrentPrice));
                    OnPropertyChanged(nameof(FormattedPrice));
                }
            }

            public string FormattedPrice => $"Ціна: {Convert.ToDouble(CurrentPrice)} грн";

            public UserPassportData PassportData
            {
                get => _passportData;
                set
                {
                    if (_passportData != null)
                    {
                        _passportData.BirthDateChanged -= PassportData_BirthDateChanged;
                    }
                    _passportData = value;
                    if (_passportData != null)
                    {
                        _passportData.BirthDateChanged += PassportData_BirthDateChanged;
                    }
                }
            }

            private void PassportData_BirthDateChanged(object sender, DateTime? birthDate)
            {
                _birthDate = birthDate;
                UserPassportDataChanged?.Invoke(this, EventArgs.Empty);
            }

            public void SetPrices(PriceCategories prices)
            {
                _prices = prices;
                RecalculatePrice();
            }

            public void RecalculatePrice()
            {
                if (_prices == null)
                {
                    CurrentPrice = 0;
                    return;
                }

                if (!_birthDate.HasValue)
                {
                    CurrentPrice = _prices.Adult;
                    return;
                }

                var age = CalculateAge(_birthDate.Value);
                CurrentPrice = age switch
                {
                    < 2 => _prices.Baby,
                    < 18 => _prices.Child,
                    >= 60 => _prices.Pensioner,
                    _ => _prices.Adult
                };
            }

            private int CalculateAge(DateTime birthDate)
            {
                var today = DateTime.Today;
                var age = today.Year - birthDate.Year;
                if (birthDate.Date > today.AddYears(-age))
                    age--;
                return age;
            }

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
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
