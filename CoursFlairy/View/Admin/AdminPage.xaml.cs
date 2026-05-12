using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using CoursFlairy.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoursFlairy.View.Admin
{
    public partial class AdminPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<AirportInfo> _airports;
        private ObservableCollection<CountryInfo> _countries;
        private string _newAirportName;
        private string _newAirportCity;
        private string _newAirportICAO;
        private CountryInfo _selectedCountry;

        public ObservableCollection<AirportInfo> Airports
        {
            get => _airports;
            set
            {
                _airports = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CountryInfo> Countries
        {
            get => _countries;
            set
            {
                _countries = value;
                OnPropertyChanged();
            }
        }

        public CountryInfo SelectedCountry
        {
            get => _selectedCountry;
            set
            {
                _selectedCountry = value;
                OnPropertyChanged();
            }
        }

        public string NewAirportName
        {
            get => _newAirportName;
            set
            {
                _newAirportName = value;
                OnPropertyChanged();
            }
        }

        public string NewAirportCity
        {
            get => _newAirportCity;
            set
            {
                _newAirportCity = value;
                OnPropertyChanged();
            }
        }

        public string NewAirportICAO
        {
            get => _newAirportICAO;
            set
            {
                _newAirportICAO = value;
                OnPropertyChanged();
            }
        }

        public AdminPage()
        {
            InitializeComponent();
            DataContext = this;
            Airports = new ObservableCollection<AirportInfo>();
            Countries = new ObservableCollection<CountryInfo>();
            LoadCountries();
            LoadAirports();
        }

        private void LoadCountries()
        {
            try
            {
                Countries.Clear();
                var connection = DataBase.GetConnection();
                string query = "SELECT ID, Name FROM Country ORDER BY Name";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Countries.Add(new CountryInfo
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження країн: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAirports()
        {
            try
            {
                Airports.Clear();
                var connection = DataBase.GetConnection();
                string query = @"
                    SELECT 
                        a.ID,
                        CASE 
                            WHEN a.Name IS NULL OR LTRIM(RTRIM(a.Name)) = '' THEN a.City + ' Airport'
                            ELSE a.Name 
                        END as Name,
                        a.City,
                        a.ICAO,
                        ISNULL(c.Name, N'Невідомо') as CountryName
                    FROM Airport a
                    LEFT JOIN Country c ON a.CountryID = c.ID
                    ORDER BY a.Name";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            var id = reader.GetInt32(0);
                            var name = !reader.IsDBNull(1) ? reader.GetString(1).Trim() : "Невідомий аеропорт";
                            var city = !reader.IsDBNull(2) ? reader.GetString(2).Trim() : "";
                            var icao = !reader.IsDBNull(3) ? reader.GetString(3).Trim() : "";
                            var country = !reader.IsDBNull(4) ? reader.GetString(4).Trim() : "Невідомо";

                            var airport = new AirportInfo
                            {
                                Id = id,
                                Name = name,
                                City = city,
                                ICAO = icao,
                                Country = country
                            };

                            Airports.Add(airport);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Помилка читання даних аеропорту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }

                if (Airports.Count == 0)
                {
                    MessageBox.Show("Не знайдено жодного аеропорту", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження аеропортів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddAirport_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewAirportName) || 
                string.IsNullOrWhiteSpace(NewAirportCity) || 
                string.IsNullOrWhiteSpace(NewAirportICAO) || 
                SelectedCountry == null)
            {
                MessageBox.Show("Будь ласка, заповніть всі поля", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var connection = DataBase.GetConnection();
                
                // Create airport
                string query = @"
                    INSERT INTO Airport (Name, City, ICAO, CountryID) 
                    VALUES (@name, @city, @icao, @countryId)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", NewAirportName);
                    command.Parameters.AddWithValue("@city", NewAirportCity);
                    command.Parameters.AddWithValue("@icao", NewAirportICAO);
                    command.Parameters.AddWithValue("@countryId", SelectedCountry.Id);

                    command.ExecuteNonQuery();
                }

                // Clear inputs
                NewAirportName = "";
                NewAirportCity = "";
                NewAirportICAO = "";
                SelectedCountry = null;

                // Reload airports
                LoadAirports();

                MessageBox.Show("Аеропорт успішно додано", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка додавання аеропорту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteAirport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int airportId)
            {
                try
                {
                    var connection = DataBase.GetConnection();

                    // Перевіряємо чи є прив'язані маршрути
                    string checkRoutesQuery = @"
                        SELECT COUNT(*) 
                        FROM Route 
                        WHERE DepartureID = @airportId 
                           OR ArrivalID = @airportId";

                    using (var command = new SqlCommand(checkRoutesQuery, connection))
                    {
                        command.Parameters.AddWithValue("@airportId", airportId);
                        int routeCount = (int)command.ExecuteScalar();

                        if (routeCount > 0)
                        {
                            MessageBox.Show("Неможливо видалити аеропорт, оскільки він використовується в маршрутах",
                                          "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Якщо маршрутів немає, видаляємо аеропорт
                    var result = MessageBox.Show("Ви впевнені, що хочете видалити цей аеропорт?",
                                               "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        string deleteQuery = "DELETE FROM Airport WHERE ID = @airportId";
                        using (var command = new SqlCommand(deleteQuery, connection))
                        {
                            command.Parameters.AddWithValue("@airportId", airportId);
                            command.ExecuteNonQuery();
                        }

                        LoadAirports(); // Оновлюємо список
                        MessageBox.Show("Аеропорт успішно видалено", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка видалення аеропорту: {ex.Message}", 
                                  "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            
            // Скидаємо поточний акаунт
            CurrentAccount.Set(Model.Enum.AccountType.None, -1);
            
            // Переходимо на сторінку пошуку
            mainWindow.PageManager.Navigate(new SearchPage());
            mainWindow.AccountUI.ShowLogin();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AirportInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string ICAO { get; set; }
        public string Country { get; set; }
    }

    public class CountryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
} 