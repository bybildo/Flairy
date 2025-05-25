using System;
using System.Collections.Generic;
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
using CoursFlairy.Model;
using CoursFlairy.Model.Enum;
using CoursFlairy.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Media.Media3D;
using System.Data.Common;
using System.Collections.ObjectModel;

namespace CoursFlairy.View.ClientPage
{
    /// <summary>
    /// Interaction logic for ChoosingSeats.xaml
    /// </summary>
    public partial class ChoosingSeatsPage : Page, INotifyPropertyChanged
    {
        private List<List<PlaneSeats>> _planeStructsList;
        private double _constructorColumns = 0;
        private double _constructorRows = 0;
        private int _flightId;
        private Classes _currentClass;
        private ObservableCollection<PlaneSeats> _selectedSeats;

        // New properties for flight details
        private string _departureCity;
        private string _arrivalCity;
        private string _departureIcao;
        private string _arrivalIcao;
        private string _airlineName;
        private int _economySeats;
        private int _businessSeats;
        private int _firstClassSeats;
        private decimal _economyPrice;
        private decimal _businessPrice;
        private decimal _firstClassPrice;
        private DateTime _departureTime;
        private DateTime _arrivalTime;

        public ObservableCollection<PlaneSeats> SelectedSeats
        {
            get => _selectedSeats;
            set
            {
                _selectedSeats = value;
                OnPropertyChanged(nameof(SelectedSeats));
                OnPropertyChanged(nameof(HasSelectedSeats));
            }
        }

        public bool HasSelectedSeats => SelectedSeats?.Count > 0;

        public Classes CurrentClass
        {
            get => _currentClass; set
            {
                _currentClass = value;
                pathec.Stroke = new SolidColorBrush(Colors.Transparent);
                pathbi.Stroke = new SolidColorBrush(Colors.Transparent);
                pathfi.Stroke = new SolidColorBrush(Colors.Transparent);
                switch (_currentClass)
                {
                    case Classes.Econom:
                        pathec.Stroke = pathec.Fill; break;
                    case Classes.Bussiness:
                        pathbi.Stroke = pathbi.Fill; break;
                    case Classes.First:
                        pathfi.Stroke = pathfi.Fill; break;
                }
                OnPropertyChanged(nameof(CurrentClass));
            }
        }

        public string DepartureCity { get => _departureCity; set { _departureCity = value; OnPropertyChanged(nameof(DepartureCity)); } }
        public string ArrivalCity { get => _arrivalCity; set { _arrivalCity = value; OnPropertyChanged(nameof(ArrivalCity)); } }
        public string DepartureIcao { get => _departureIcao; set { _departureIcao = value; OnPropertyChanged(nameof(DepartureIcao)); } }
        public string ArrivalIcao { get => _arrivalIcao; set { _arrivalIcao = value; OnPropertyChanged(nameof(ArrivalIcao)); } }
        public string AirlineName { get => _airlineName; set { _airlineName = value; OnPropertyChanged(nameof(AirlineName)); } }
        public int EconomySeats { get => _economySeats; set { _economySeats = value; OnPropertyChanged(nameof(EconomySeats)); } }
        public int BusinessSeats { get => _businessSeats; set { _businessSeats = value; OnPropertyChanged(nameof(BusinessSeats)); } }
        public int FirstClassSeats { get => _firstClassSeats; set { _firstClassSeats = value; OnPropertyChanged(nameof(FirstClassSeats)); } }
        public decimal EconomyPrice { get => _economyPrice; set { _economyPrice = value; OnPropertyChanged(nameof(EconomyPrice)); } }
        public decimal BusinessPrice { get => _businessPrice; set { _businessPrice = value; OnPropertyChanged(nameof(BusinessPrice)); } }
        public decimal FirstClassPrice { get => _firstClassPrice; set { _firstClassPrice = value; OnPropertyChanged(nameof(FirstClassPrice)); } }
        public DateTime DepartureTime { get => _departureTime; set { _departureTime = value; OnPropertyChanged(nameof(DepartureTime)); } }
        public DateTime ArrivalTime { get => _arrivalTime; set { _arrivalTime = value; OnPropertyChanged(nameof(ArrivalTime)); } }

        public string DepartureTimeString => DepartureTime.ToString("HH:mm");
        public string ArrivalTimeString => ArrivalTime.ToString("HH:mm");
        public string DepartureDateString => DepartureTime.ToString("dd.MM.yyyy");
        public string ArrivalDateString => ArrivalTime.ToString("dd.MM.yyyy");

        public double ConstructorWidth { get { try { return PlaneStructsList[1][0].WidthElement * ConstructorColumns + 2; } catch { return 0; } } }
        public double ConstructorWidthItem { get { try { return PlaneStructsList[1][0].WidthElement * ConstructorColumns; } catch { return 0; } } }
        public double ConstructorColumns { get => _constructorColumns; set { _constructorColumns = value; OnPropertyChanged(nameof(ConstructorColumns)); } }
        public double ConstructorRows { get => _constructorRows; set { _constructorRows = value; OnPropertyChanged(nameof(ConstructorRows)); } }

        public List<List<PlaneSeats>> PlaneStructsList
        {
            get => _planeStructsList;
            set
            {
                _planeStructsList = value;
                OnPropertyChanged(nameof(PlaneStructsList));
            }
        }

        public ChoosingSeatsPage(int flightId, Classes currentClass)
        {
            InitializeComponent();
            _flightId = flightId;
            CurrentClass = currentClass;
            _planeStructsList = new List<List<PlaneSeats>>();
            _selectedSeats = new ObservableCollection<PlaneSeats>();
            LoadFlightDetails();
            LoadPlaneScheme();

            Loaded += (s, e) =>
            {
                var parent = FindParent<SelectPage>(this);
                if (parent != null)
                {
                    parent.flightId = flightId;
                    parent.currentClass = currentClass;
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

        public ChoosingSeatsPage()
        {
            InitializeComponent();
            _planeStructsList = new List<List<PlaneSeats>>();
            _selectedSeats = new ObservableCollection<PlaneSeats>();
            _flightId = 2007;
            CurrentClass = Classes.Econom;
            LoadFlightDetails();
            LoadPlaneScheme();
        }

        private void LoadFlightDetails()
        {
            try
            {
                string query = @"
                    SELECT 
                        depAir.City as DepCity,
                        arrAir.City as ArrCity,
                        depAir.ICAO as DepICAO,
                        arrAir.ICAO as ArrICAO,
                        al.Name as AirlineName,
                        pl.Economy as EconomySeats,
                        pl.Bussiness as BusinessSeats,
                        pl.First as FirstClassSeats,
                        pe.Adult as EconomyPrice,
                        pb.Adult as BusinessPrice,
                        pf.Adult as FirstClassPrice,
                        f.DTime,
                        DATEADD(MINUTE, r.AmountTime, f.DTime) as ArrivalTime
                    FROM Flight f
                    JOIN Route r ON f.RouteID = r.ID
                    JOIN Airport depAir ON r.DepartureID = depAir.ID
                    JOIN Airport arrAir ON r.ArrivalID = arrAir.ID
                    JOIN Plane pl ON r.PlaneID = pl.ID
                    JOIN Airline al ON pl.AirlineID = al.ID
                    LEFT JOIN Price pe ON r.ID = pe.RouteID AND pe.ClassGroup = 1
                    LEFT JOIN Price pb ON r.ID = pb.RouteID AND pb.ClassGroup = 2
                    LEFT JOIN Price pf ON r.ID = pf.RouteID AND pf.ClassGroup = 3
                    WHERE f.ID = @flightId";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@flightId", _flightId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DepartureCity = reader.GetString(0);
                            ArrivalCity = reader.GetString(1);
                            DepartureIcao = reader.GetString(2);
                            ArrivalIcao = reader.GetString(3);
                            AirlineName = reader.GetString(4);
                            EconomySeats = reader.GetInt32(5);
                            BusinessSeats = reader.GetInt32(6);
                            FirstClassSeats = reader.GetInt32(7);

                            // Handle nullable prices
                            EconomyPrice = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
                            BusinessPrice = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                            FirstClassPrice = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);

                            DepartureTime = reader.GetDateTime(11);
                            ArrivalTime = reader.GetDateTime(12);

                            // Update UI visibility based on available seats
                            UpdateClassVisibility();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження деталей рейсу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateClassVisibility()
        {
            // Get the parent window to access the XAML elements
            var parentWindow = Window.GetWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
                // Find the class icons in the XAML
                var economyIcon = mainWindow.FindName("pathec") as Path;
                var businessIcon = mainWindow.FindName("pathbi") as Path;
                var firstClassIcon = mainWindow.FindName("pathfi") as Path;

                if (economyIcon != null)
                    economyIcon.Visibility = EconomySeats > 0 ? Visibility.Visible : Visibility.Collapsed;
                if (businessIcon != null)
                    businessIcon.Visibility = BusinessSeats > 0 ? Visibility.Visible : Visibility.Collapsed;
                if (firstClassIcon != null)
                    firstClassIcon.Visibility = FirstClassSeats > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void LoadPlaneScheme()
        {
            try
            {
                string query = @"
                    SELECT p.SeatScheme 
                    FROM Flight f
                    JOIN Route r ON f.RouteID = r.ID
                    JOIN Plane p ON r.PlaneID = p.ID
                    WHERE f.ID = @flightId";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@flightId", _flightId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string scheme = reader.GetString(0);
                            ParsePlaneScheme(scheme);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження схеми літака: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ParsePlaneScheme(string scheme)
        {
            var planeSeatsList = new List<List<PlaneSeats>>();

            var rows = scheme.Split('|');
            ConstructorRows = rows.Length;
            ConstructorColumns = rows[0].Split(',').Length;

            bool firstRow = true;
            foreach (var row in rows)
            {
                var seatDetails = row.Split(',');

                var rowSeats = new List<PlaneSeats>();

                foreach (var seat in seatDetails)
                {
                    var seatInfo = seat.Split('-');
                    var letter = seatInfo[0];
                    var seatNumber = int.Parse(seatInfo[1]);
                    var brush = (PlaneBrushes)Enum.Parse(typeof(PlaneBrushes), seatInfo[2], true);

                    var widthA = this.ActualWidth;
                    double SeatSize = 60 - ((ConstructorColumns - 1) / 109.0) * 20;
                    PlaneSeats planeSeat = new PlaneSeats(brush, letter.ToString(), seatNumber, SeatSize, _currentClass);

                    if (firstRow)
                    {
                        planeSeat.PathAngleTransform = new RotateTransform { Angle = 270 };
                    }
                    rowSeats.Add(planeSeat);
                }
                firstRow = false;

                planeSeatsList.Add(rowSeats);
            }
            PlaneStructsList = planeSeatsList;
        }

        private void FilteredButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FilteredButtonOverlay.Opacity = 1;
            RandomButtonOverlay.Opacity = 0;
            FilteredButtonText.Foreground = Brushes.White;
            RandomButtonText.Foreground = (Brush)FindResource("MainColor100");

            foreach (var row in PlaneStructsList)
            {
                foreach (var seat in row)
                {
                    seat.SetClass(_currentClass);
                }
            }

            OnPropertyChanged(nameof(PlaneStructsList));
        }

        private void RandomButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FilteredButtonOverlay.Opacity = 0;
            RandomButtonOverlay.Opacity = 1;
            FilteredButtonText.Foreground = (Brush)FindResource("MainColor100");
            RandomButtonText.Foreground = Brushes.White;

            foreach (var row in PlaneStructsList)
            {
                foreach (var seat in row)
                {
                    seat.SetClass(Classes.None);
                }
            }

            OnPropertyChanged(nameof(PlaneStructsList));
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void EconomyPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FilteredButton_MouseDown(null, null);
            CurrentClass = Classes.Econom;

            foreach (var row in PlaneStructsList)
            {
                foreach (var seat in row)
                {
                    seat.SetClass(Classes.Econom);
                }
            }
        }

        private void BusinessPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FilteredButton_MouseDown(null, null);
            CurrentClass = Classes.Bussiness;

            foreach (var row in PlaneStructsList)
            {
                foreach (var seat in row)
                {
                    seat.SetClass(Classes.Bussiness);
                }
            }
        }

        private void FirstPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FilteredButton_MouseDown(null, null);
            CurrentClass = Classes.First;

            foreach (var row in PlaneStructsList)
            {
                foreach (var seat in row)
                {
                    seat.SetClass(Classes.First);
                }
            }
        }

        private void Seat_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as Grid;
            if (grid?.DataContext is PlaneSeats seat)
            {
                seat.ToggleSelection();
                if (seat.IsSelected)
                {
                    if (!SelectedSeats.Contains(seat))
                    {
                        SelectedSeats.Add(seat);
                        OnPropertyChanged(nameof(HasSelectedSeats));
                    }
                }
                else
                {
                    SelectedSeats.Remove(seat);
                    OnPropertyChanged(nameof(HasSelectedSeats));
                }
            }
        }

        private void SelectedSeat_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.DataContext is PlaneSeats seat)
            {
                seat.ToggleSelection();
                SelectedSeats.Remove(seat);
                OnPropertyChanged(nameof(HasSelectedSeats));
            }
        }

        private void SelectButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedSeats.Count > 0)
            {
                var selectedClasses = SelectedSeats.Select(s => s.GetClass()).ToList();
                var passengerDataPage = new PassengerDataPage(_flightId, selectedClasses);
                var selectPage = FindParent<SelectPage>(this);
                if (selectPage != null)
                {
                    selectPage.SelectedSeatsCode = new List<string>();
                    selectPage.SelectedSeats = new List<string>();
                    foreach (var seat in SelectedSeats)
                    {
                        selectPage.SelectedSeats.Add(seat.GetSeat());
                        selectPage.SelectedSeatsCode.Add(seat.ToString());
                    }

                    selectPage.CurentPage = 4;
                    selectPage.PageManager.Navigate(passengerDataPage);
                    selectPage.FillPath(4);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть хоча б одне місце", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
