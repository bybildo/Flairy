using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoursFlairy.Data;
using CoursFlairy.Model;
using CoursFlairy.Model.Enum;
using Microsoft.Data.SqlClient;
using CoursFlairy.View.UI;
using static CoursFlairy.ViewModel.ResourceColor;

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
        private string _email = "";
        private decimal _totalSum;
        private bool _isUserLoggedIn = false;
        private bool _showQuickFillButtons = false;
        private bool _hasUsedQuickFill = false;
        private PassengerInfo _quickFilledPassenger = null;

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

        public bool ShowQuickFillButtons
        {
            get => _showQuickFillButtons;
            set
            {
                _showQuickFillButtons = value;
                OnPropertyChanged(nameof(ShowQuickFillButtons));
            }
        }

        public ObservableCollection<PassengerInfo> Passengers
        {
            get => _passengers;
            set
            {
                _passengers = value;
                OnPropertyChanged(nameof(Passengers));
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public double TotalSum
        {
            get => (double)_totalSum;
            set
            {
                _totalSum = (decimal)value;
                OnPropertyChanged(nameof(TotalSum));
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
                LoadUserEmail();
                var parent = FindParent<SelectPage>(this);
                if (parent != null)
                {
                    parent.flightId = flightId;
                    parent.passengerClasses = _passengerClasses;
                    parent.Prices = Passengers.Select(p => p.CurrentPrice).ToList();
                }
            };
        }

        private void LoadUserEmail()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (CurrentAccount.id != -1 && CurrentAccount.accountType == AccountType.User)
            {
                try
                {
                    var connection = DataBase.GetConnection();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Email, Citizenship FROM [User] WHERE ID = @UserId";
                    command.Parameters.AddWithValue("@UserId", CurrentAccount.id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Завантажуємо email
                            if (!reader.IsDBNull(0))
                            {
                                Email = reader.GetString(0);
                                _isUserLoggedIn = true;
                                
                                if (email != null)
                                {
                                    email.IsEnabled = false;
                                    email.Text = Email;
                                }
                            }

                            // Перевіряємо наявність громадянства
                            bool hasCitizenship = !reader.IsDBNull(1);
                            ShowQuickFillButtons = hasCitizenship;
                        }
                        else
                        {
                            ShowQuickFillButtons = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (mainWindow != null)
                    {
                        mainWindow.GlobalMessage.Show($"Помилка завантаження email: {ex.Message}");
                    }
                    ShowQuickFillButtons = false;
                }
            }
            else
            {
                ShowQuickFillButtons = false;
            }
        }

        private void LoadFlightPrices()
        {
            try
            {
                var connection = Data.DataBase.GetConnection();
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
                    LEFT JOIN Price pe ON r.ID = pe.RouteID AND pe.ClassGroup = 1
                    LEFT JOIN Price pb ON r.ID = pb.RouteID AND pb.ClassGroup = 2
                    LEFT JOIN Price pf ON r.ID = pf.RouteID AND pf.ClassGroup = 3
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
                            reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),  // Adult
                            reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),  // Child
                            reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),  // Baby
                            reader.IsDBNull(9) ? 0 : reader.GetDecimal(9)   // Pensioner
                        );
                        prices[Classes.Bussiness] = new PriceCategories(
                            reader.IsDBNull(10) ? 0 : reader.GetDecimal(10), // Adult
                            reader.IsDBNull(11) ? 0 : reader.GetDecimal(11), // Child
                            reader.IsDBNull(12) ? 0 : reader.GetDecimal(12), // Baby
                            reader.IsDBNull(13) ? 0 : reader.GetDecimal(13)  // Pensioner
                        );
                        prices[Classes.First] = new PriceCategories(
                            reader.IsDBNull(14) ? 0 : reader.GetDecimal(14), // Adult
                            reader.IsDBNull(15) ? 0 : reader.GetDecimal(15), // Child
                            reader.IsDBNull(16) ? 0 : reader.GetDecimal(16), // Baby
                            reader.IsDBNull(17) ? 0 : reader.GetDecimal(17)  // Pensioner
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
            catch (SqlException ex)
            {
                MessageBox.Show($"Помилка бази даних при завантаженні цін: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                passenger.PropertyChanged += Passenger_PropertyChanged;
                Passengers.Add(passenger);
            }
            UpdateTotalSum();
        }

        private void Passenger_UserPassportDataChanged(object sender, EventArgs e)
        {
            if (sender is PassengerInfo passenger)
            {
                passenger.RecalculatePrice();
            }
        }

        private void Passenger_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PassengerInfo.CurrentPrice))
            {
                UpdateTotalSum();
            }
        }

        private void UpdateTotalSum()
        {
            TotalSum = Passengers.Sum(p => (double)p.CurrentPrice);
            
            // Также обновляем список цен в SelectPage
            var selectPage = FindParent<SelectPage>(this);
            if (selectPage != null && Passengers != null)
            {
                selectPage.Prices = Passengers.Select(p => p.CurrentPrice).ToList();
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

        private const string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        private void emailAnimation(object sender, TextChangedEventArgs e)
        {
            if (_isUserLoggedIn) return;

            if (sender is TextBox emailTextBox)
            {
                bool isValid = Regex.IsMatch(emailTextBox.Text, emailPattern);
                if (emailBorder.BorderBrush is not SolidColorBrush brush || brush.IsFrozen)
                    emailBorder.BorderBrush = brush = new SolidColorBrush(MainColor20.Color);

                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(isValid ? MainColor100.Color : MainColor20.Color, TimeSpan.FromSeconds(0.15)));
                Email = emailTextBox.Text;
            }
        }

        private void BorderAnimation(Border border, bool start = true)
        {
            if (border.BorderBrush is not SolidColorBrush brush || brush.IsFrozen)
                border.BorderBrush = brush = new SolidColorBrush(MainColor20.Color);

            if (!start)
                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor100.Color, TimeSpan.FromSeconds(0.15)));
            else
                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor20.Color, TimeSpan.FromSeconds(0.15)));
        }

        private void PaymentButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            var selectPage = FindParent<SelectPage>(this);

            selectPage.Clients = new List<PassportInfo>();
            bool allPassengersValid = true;
            foreach (var passenger in Passengers)
            {
                if (passenger.PassportData.Validation() == State.unsuccessful)
                {
                    allPassengersValid = false;
                    break;
                }
                else
                {
                    if (selectPage != null)
                    {
                        selectPage.Clients.Add(passenger.PassportData.GetPassportInfo());
                    }
                }
            }

            if (!allPassengersValid)
            {
                mainWindow.GlobalMessage.Show("Будь ласка, заповніть коректно дані всіх пасажирів", 3);
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                mainWindow.GlobalMessage.Show("Будь ласка, введіть email", 3);
                return;
            }

            if (!Regex.IsMatch(Email, emailPattern))
            {
                mainWindow.GlobalMessage.Show("Будь ласка, введіть коректний email", 3);
                return;
            }

            if (selectPage != null)
            {
                selectPage.Email = Email;
                selectPage.Prices = Passengers.Select(p => p.CurrentPrice).ToList();
                selectPage.CurentPage = 5;
                selectPage.PageManager.Navigate(new PaymentPage(TotalSum));
                selectPage.FillPath(5);
            }
        }

        private void QuickFillButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            
            // Перевіряємо чи користувач авторизований
            if (CurrentAccount.id == -1 || CurrentAccount.accountType != AccountType.User)
            {
                mainWindow.GlobalMessage.Show("Для швидкого заповнення потрібно війти в акаунт", 3);
                return;
            }

            // Отримуємо PassengerInfo з Tag кнопки
            if (sender is Button button && button.Tag is PassengerInfo passengerInfo)
            {
                LoadUserDataForPassenger(passengerInfo);
                _hasUsedQuickFill = true;
                _quickFilledPassenger = passengerInfo;
                
                // Змінюємо текст кнопки на "Скасувати заповнення"
                button.Content = "Скасувати заповнення";
                button.Click -= QuickFillButton_Click;
                button.Click += CancelQuickFill_Click;
                
                // Блокуємо поля для редагування
                if (passengerInfo.PassportData != null)
                {
                    passengerInfo.PassportData.SetReadOnly(true);
                    passengerInfo.PassportData.UpdateColor();
                }

                // Знаходимо всі кнопки швидкого заповнення та приховуємо їх
                var parentWindow = Window.GetWindow(button);
                if (parentWindow != null)
                {
                    var allButtons = FindVisualChildren<Button>(parentWindow)
                        .Where(b => b != button && b.Name == "QuickFillButton");
                    
                    foreach (var otherButton in allButtons)
                    {
                        otherButton.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void CancelQuickFill_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            
            if (sender is Button button && button.Tag is PassengerInfo passengerInfo)
            {
                // Очищаємо дані
                if (passengerInfo.PassportData != null)
                {
                    passengerInfo.PassportData.ClearData();
                    passengerInfo.PassportData.SetReadOnly(false);
                }
                
                // Скидаємо стан
                _hasUsedQuickFill = false;
                _quickFilledPassenger = null;
                
                // Повертаємо початковий стан кнопки
                button.Content = "Заповнити моїми даними";
                button.Click -= CancelQuickFill_Click;
                button.Click += QuickFillButton_Click;
                
                // Показуємо всі кнопки швидкого заповнення
                var parentWindow = Window.GetWindow(button);
                if (parentWindow != null)
                {
                    var allButtons = FindVisualChildren<Button>(parentWindow)
                        .Where(b => b.Name == "QuickFillButton");
                    
                    foreach (var quickFillButton in allButtons)
                    {
                        quickFillButton.Visibility = Visibility.Visible;
                    }
                }
                
                mainWindow.GlobalMessage.Show("Швидке заповнення скасовано", 2);
            }
        }

        private void LoadUserDataForPassenger(PassengerInfo passengerInfo)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            
            try
            {
                var connection = DataBase.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT u.Gender, u.Name, u.Surname, u.BirthDate, u.Passport, u.PassportDate, u.Citizenship, c.Name as CountryName
                    FROM [User] u
                    LEFT JOIN Country c ON u.Citizenship = c.ID
                    WHERE u.ID = @UserId";
                command.Parameters.AddWithValue("@UserId", CurrentAccount.id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Перевіряємо чи є громадянство
                        if (reader.IsDBNull(6)) // Citizenship
                        {
                            mainWindow.GlobalMessage.Show("У вашому профілі не вказано громадянство. Будь ласка, заповніть дані у профілі", 3);
                            return;
                        }

                        var passportData = passengerInfo.PassportData;
                        if (passportData != null)
                        {
                            // Заповнюємо стать
                            if (!reader.IsDBNull(0))
                            {
                                passportData.gender = (Gender)reader.GetInt32(0);
                            }

                            // Заповнюємо ім'я
                            if (!reader.IsDBNull(1))
                            {
                                passportData.Namee = reader.GetString(1);                              
                            }

                            // Заповнюємо прізвище
                            if (!reader.IsDBNull(2))
                            {
                                passportData.Surname = reader.GetString(2);
                            }

                            // Заповнюємо дату народження
                            if (!reader.IsDBNull(3))
                            {
                                var birthDate = reader.GetDateTime(3);
                                passportData.PersonalDay = birthDate.Day.ToString("00");
                                passportData.PersonalMonth = birthDate.Month.ToString("00");
                                passportData.PersonalYear = birthDate.Year.ToString();
                            }

                            // Заповнюємо номер паспорта
                            if (!reader.IsDBNull(4))
                            {
                                passportData.Passport = reader.GetString(4);
                            }

                            // Заповнюємо дату закінчення паспорта
                            if (!reader.IsDBNull(5))
                            {
                                var passportDate = reader.GetDateTime(5);
                                passportData.PassportDay = passportDate.Day.ToString("00");
                                passportData.PassportMonth = passportDate.Month.ToString("00");
                                passportData.PassportYear = passportDate.Year.ToString();
                            }

                            // Заповнюємо громадянство
                            if (!reader.IsDBNull(7))
                            {
                                passportData.Citizenship = reader.GetString(7);
                                passportData.CitizenshipHint = reader.GetString(7);
                                passportData.CitizenshipUpdate();
                            }

                            passportData.UpdateColor();
                            mainWindow.GlobalMessage.Show("Дані успішно заповнено", 2);
                        }
                    }
                    else
                    {
                        mainWindow.GlobalMessage.Show("Не вдалося завантажити дані користувача", 3);
                    }
                }
            }
            catch (SqlException ex)
            {
                mainWindow.GlobalMessage.Show($"Помилка бази даних: {ex.Message}", 3);
            }
            catch (Exception ex)
            {
                mainWindow.GlobalMessage.Show($"Помилка завантаження даних: {ex.Message}", 3);
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

            public string FormattedPrice => $"Ціна: {CurrentPrice:N0} грн";

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

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                
                if (child is T t)
                    yield return t;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }
}
