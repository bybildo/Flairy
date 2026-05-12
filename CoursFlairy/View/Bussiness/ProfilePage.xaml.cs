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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoursFlairy.Data;
using CoursFlairy.Model;
using CoursFlairy.Model.Enum;
using Microsoft.Data.SqlClient;
using static CoursFlairy.ViewModel.ResourceColor;

namespace CoursFlairy.View.Bussiness
{
    /// <summary>
    /// Interaction logic for ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page, INotifyPropertyChanged
    {
        private CompanyStats _stats = new CompanyStats();
        private bool _isLoading = true;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                LoadingOverlay.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ProfilePage()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += ProfilePage_Loaded;
        }

        private async void ProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCompanyData();
        }

        private async Task LoadCompanyData()
        {
            IsLoading = true;
            
            try
            {
                await Task.Run(() => LoadCompanyInfo());
                await Task.Run(() => LoadStatistics());
                
                // Simulate a brief loading time for better UX
                await Task.Delay(500);
                
                UpdateUI();
            }
            catch (Exception ex)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.GlobalMessage.Show($"Помилка завантаження даних: {ex.Message}", 3);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadCompanyInfo()
        {
            try
            {
                var connection = DataBase.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT a.Name, a.Email, c.Name as CountryName
                    FROM Airline a
                    LEFT JOIN Country c ON a.CountryID = c.ID
                    WHERE a.ID = @AirlineId";
                command.Parameters.AddWithValue("@AirlineId", CurrentAccount.id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        _stats.CompanyName = reader.IsDBNull(0) ? "Невідома компанія" : reader.GetString(0);
                        _stats.Email = reader.IsDBNull(1) ? "Не вказано" : reader.GetString(1);
                        _stats.Country = reader.IsDBNull(2) ? "Не вказано" : reader.GetString(2);
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.GlobalMessage.Show($"Помилка завантаження інформації про компанію: {ex.Message}", 3);
                });
            }
        }

        private void LoadStatistics()
        {
            try
            {
                var connection = DataBase.GetConnection();
                
                // Load aircraft count and seats
                LoadAircraftStats(connection);
                
                // Load route count
                LoadRouteStats(connection);
                
                // Load flight statistics
                LoadFlightStats(connection);
                
                // Load monthly statistics
                LoadMonthlyStats(connection);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.GlobalMessage.Show($"Помилка завантаження статистики: {ex.Message}", 3);
                });
            }
        }

        private void LoadAircraftStats(SqlConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    COUNT(*) as TotalAircraft,
                    ISNULL(SUM(Economy), 0) as TotalEconomySeats,
                    ISNULL(SUM(Bussiness), 0) as TotalBusinessSeats,
                    ISNULL(SUM(First), 0) as TotalFirstSeats
                FROM Plane 
                WHERE AirlineID = @AirlineId";
            command.Parameters.AddWithValue("@AirlineId", CurrentAccount.id);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    _stats.TotalAircraft = reader.GetInt32(0);
                    _stats.EconomySeats = reader.GetInt32(1);
                    _stats.BusinessSeats = reader.GetInt32(2);
                    _stats.FirstClassSeats = reader.GetInt32(3);
                }
            }
        }

        private void LoadRouteStats(SqlConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) as TotalRoutes
                FROM Route 
                WHERE AirlineID = @AirlineId";
            command.Parameters.AddWithValue("@AirlineId", CurrentAccount.id);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    _stats.TotalRoutes = reader.GetInt32(0);
                }
            }
        }

        private void LoadFlightStats(SqlConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    COUNT(*) as TotalFlights,
                    COUNT(CASE WHEN s.Name IN ('Scheduled', 'In progress') THEN 1 END) as ActiveFlights,
                    COUNT(CASE WHEN s.Name = 'Completed' THEN 1 END) as CompletedFlights,
                    COUNT(CASE WHEN s.Name = 'Cancelled' THEN 1 END) as CancelledFlights
                FROM Flight f
                JOIN Route r ON f.RouteID = r.ID
                LEFT JOIN Enum_Status s ON f.ID = s.ID
                WHERE r.AirlineID = @AirlineId";
            command.Parameters.AddWithValue("@AirlineId", CurrentAccount.id);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    _stats.TotalFlights = reader.GetInt32(0);
                    _stats.ActiveFlights = reader.GetInt32(1);
                    _stats.CompletedFlights = reader.GetInt32(2);
                    _stats.CancelledFlights = reader.GetInt32(3);
                    
                    // Calculate success rate
                    var totalCompleted = _stats.CompletedFlights + _stats.CancelledFlights;
                    if (totalCompleted > 0)
                    {
                        _stats.SuccessRate = (double)_stats.CompletedFlights / totalCompleted * 100;
                    }
                }
            }
        }

        private void LoadMonthlyStats(SqlConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    COUNT(DISTINCT f.ID) as MonthlyFlights,
                    COUNT(t.ID) as MonthlyPassengers
                FROM Flight f
                JOIN Route r ON f.RouteID = r.ID
                LEFT JOIN Ticket t ON f.ID = t.FlightID
                WHERE r.AirlineID = @AirlineId 
                    AND f.DTime >= DATEADD(MONTH, -1, GETDATE())
                    AND f.DTime <= GETDATE()";
            command.Parameters.AddWithValue("@AirlineId", CurrentAccount.id);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    _stats.MonthlyFlights = reader.GetInt32(0);
                    _stats.MonthlyPassengers = reader.GetInt32(1);
                }
            }
        }

        private void UpdateUI()
        {
            Dispatcher.Invoke(() =>
            {
                // Update company info
                CompanyNameText.Text = _stats.CompanyName;
                CompanyNameDetail.Text = _stats.CompanyName;
                CompanyEmailText.Text = _stats.Email;
                CompanyCountryText.Text = _stats.Country;

                // Update main stats
                TotalAircraftText.Text = _stats.TotalAircraft.ToString();
                TotalRoutesText.Text = _stats.TotalRoutes.ToString();
                TotalFlightsText.Text = _stats.TotalFlights.ToString();
                ActiveFlightsText.Text = _stats.ActiveFlights.ToString();

                // Update performance stats
                CompletedFlightsText.Text = _stats.CompletedFlights.ToString();
                CancelledFlightsText.Text = _stats.CancelledFlights.ToString();
                SuccessRateText.Text = $"{_stats.SuccessRate:F1}%";

                // Update progress bars
                var totalFinished = _stats.CompletedFlights + _stats.CancelledFlights;
                if (totalFinished > 0)
                {
                    CompletedFlightsProgress.Value = (double)_stats.CompletedFlights / totalFinished * 100;
                    CancelledFlightsProgress.Value = (double)_stats.CancelledFlights / totalFinished * 100;
                }

                // Update monthly stats
                MonthlyFlightsText.Text = _stats.MonthlyFlights.ToString();
                MonthlyPassengersText.Text = _stats.MonthlyPassengers.ToString();

                // Animate the statistics
                AnimateStatistics();
            });
        }

        private void AnimateStatistics()
        {
            // Animate the main stat cards
            AnimateCard(TotalAircraftText, 0, _stats.TotalAircraft);
            AnimateCard(TotalRoutesText, 0, _stats.TotalRoutes, 0.2);
            AnimateCard(TotalFlightsText, 0, _stats.TotalFlights, 0.4);
            AnimateCard(ActiveFlightsText, 0, _stats.ActiveFlights, 0.6);
        }

        private void AnimateCard(TextBlock textBlock, int from, int to, double delay = 0)
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            var startTime = DateTime.Now.AddSeconds(delay);
            var duration = TimeSpan.FromSeconds(1.5);
            var range = to - from;
            
            timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            timer.Tick += (s, e) =>
            {
                var elapsed = DateTime.Now - startTime;
                if (elapsed < TimeSpan.Zero)
                    return;
                    
                if (elapsed >= duration)
                {
                    textBlock.Text = to.ToString();
                    timer.Stop();
                }
                else
                {
                    var progress = elapsed.TotalSeconds / duration.TotalSeconds;
                    var easeProgress = 1 - Math.Pow(1 - progress, 3); // Ease out cubic
                    var current = from + (int)(range * easeProgress);
                    textBlock.Text = current.ToString();
                }
            };
            timer.Start();
        }

        private void AddAircraftButton_Click(object sender, RoutedEventArgs e)
        {
            var parentControl = FindParent<BussinessControlPage>(this);
            if (parentControl != null)
            {
                parentControl.PageManager.Navigate(new AddPlanePage());
            }
        }

        private void AddFlightButton_Click(object sender, RoutedEventArgs e)
        {
            var parentControl = FindParent<BussinessControlPage>(this);
            if (parentControl != null)
            {
                parentControl.PageManager.Navigate(new FlightPage());
            }
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

        public class CompanyStats
        {
            public string CompanyName { get; set; } = "Завантаження...";
            public string Email { get; set; } = "Завантаження...";
            public string Country { get; set; } = "Завантаження...";
            public int TotalAircraft { get; set; } = 0;
            public int TotalRoutes { get; set; } = 0;
            public int TotalFlights { get; set; } = 0;
            public int ActiveFlights { get; set; } = 0;
            public int CompletedFlights { get; set; } = 0;
            public int CancelledFlights { get; set; } = 0;
            public double SuccessRate { get; set; } = 0;
            public int EconomySeats { get; set; } = 0;
            public int BusinessSeats { get; set; } = 0;
            public int FirstClassSeats { get; set; } = 0;
            public int MonthlyFlights { get; set; } = 0;
            public int MonthlyPassengers { get; set; } = 0;
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
