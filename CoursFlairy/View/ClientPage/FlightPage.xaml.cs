using CoursFlairy.Data;
using CoursFlairy.Model;
using CoursFlairy.Model.UI;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace CoursFlairy.View.ClientPage
{
    public partial class FlightClientPage : Page, INotifyPropertyChanged
    {
        private List<FlightStruct> _filteredFlights;
        private FlightStruct _filter;

        public List<FlightStruct> FilteredFlights
        {
            get => _filteredFlights;
            set
            {
                _filteredFlights = value;
                OnPropertyChanged(nameof(FilteredFlights));
            }
        }

        public FlightClientPage(FlightStruct filter)
        {
            InitializeComponent();
            _filter = filter;
            FilteredFlights = new List<FlightStruct>();
            UpdateFilteredFlights();
        }

        public FlightClientPage()
        {
            InitializeComponent();
            FilteredFlights = new List<FlightStruct>();
        }

        private void UpdateFilteredFlights()
        {
            try
            {
                string query = @"
                    SELECT 
                        f.ID as FlightId,
                        f.DTime,
                        DATEADD(MINUTE, r.AmountTime, f.DTime) as ArrivalTime,
                        -- Departure Airport Info
                        depCountry.Name as DepCountry,
                        depAir.City as DepCity,
                        depAir.Name as DepName,
                        depAir.ICAO as DepCode,
                        -- Arrival Airport Info
                        arrCountry.Name as ArrCountry,
                        arrAir.City as ArrCity,
                        arrAir.Name as ArrName,
                        arrAir.ICAO as ArrCode,
                        -- Price Info
                        p.Adult as AdultPrice,
                        -- Seat Info
                        CASE 
                            WHEN @classGroup = 1 THEN pl.Economy
                            WHEN @classGroup = 2 THEN pl.Bussiness
                            WHEN @classGroup = 3 THEN pl.First
                        END as AvailableSeats
                    FROM Flight f
                    INNER JOIN Route r ON f.RouteID = r.ID
                    INNER JOIN Airport depAir ON r.DepartureID = depAir.ID
                    INNER JOIN Airport arrAir ON r.ArrivalID = arrAir.ID
                    INNER JOIN Country depCountry ON depAir.CountryID = depCountry.ID
                    INNER JOIN Country arrCountry ON arrAir.CountryID = arrCountry.ID
                    INNER JOIN Plane pl ON r.PlaneID = pl.ID
                    INNER JOIN Price p ON r.ID = p.RouteID AND p.ClassGroup = @classGroup
                    WHERE f.DTime > GETDATE()";

                List<SqlParameter> parameters = new List<SqlParameter>();

                if (_filter != null)
                {
                    parameters.Add(new SqlParameter("@classGroup", (int)_filter.PersonClasses[0]));
                    parameters.Add(new SqlParameter("@requiredSeats", _filter.PersonClasses.Length));

                    if (_filter.DepartureAirport != null)
                    {
                        if (!string.IsNullOrEmpty(_filter.DepartureAirport.Code))
                        {
                            query += " AND depAir.ICAO = @departureICAO";
                            parameters.Add(new SqlParameter("@departureICAO", _filter.DepartureAirport.Code));
                        }
                        else if (!string.IsNullOrEmpty(_filter.DepartureAirport.City))
                        {
                            query += " AND depAir.City = @departureCity";
                            parameters.Add(new SqlParameter("@departureCity", _filter.DepartureAirport.City));
                        }
                    }

                    if (_filter.ArrivalAirport != null)
                    {
                        if (!string.IsNullOrEmpty(_filter.ArrivalAirport.Code))
                        {
                            query += " AND arrAir.ICAO = @arrivalICAO";
                            parameters.Add(new SqlParameter("@arrivalICAO", _filter.ArrivalAirport.Code));
                        }
                        else if (!string.IsNullOrEmpty(_filter.ArrivalAirport.City))
                        {
                            query += " AND arrAir.City = @arrivalCity";
                            parameters.Add(new SqlParameter("@arrivalCity", _filter.ArrivalAirport.City));
                        }
                    }

                    if (_filter.DateFlight != null && _filter.DateFlight.Count > 0 && _filter.DateFlight[0] != DateTime.MinValue)
                    {
                        var dateConditions = new List<string>();
                        for (int i = 0; i < _filter.DateFlight.Count; i++)
                        {
                            DateTime selectedDate = _filter.DateFlight[i].Date;
                            if (selectedDate >= DateTime.Today)
                            {
                                dateConditions.Add($"CAST(f.DTime AS DATE) = @flightDate{i}");
                                parameters.Add(new SqlParameter($"@flightDate{i}", selectedDate));
                            }
                        }
                        if (dateConditions.Count > 0)
                        {
                            query += " AND (" + string.Join(" OR ", dateConditions) + ")";
                        }
                    }

                    query += @" AND CASE 
                        WHEN @classGroup = 1 THEN pl.Economy
                        WHEN @classGroup = 2 THEN pl.Bussiness
                        WHEN @classGroup = 3 THEN pl.First
                    END >= @requiredSeats";
                }

                query += " ORDER BY f.DTime";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }

                    var flights = new List<FlightStruct>();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var departureAirport = new AirportStruct(
                                reader.IsDBNull(3) ? "" : reader.GetString(3),  // DepCountry
                                reader.IsDBNull(4) ? "" : reader.GetString(4),  // DepCity
                                reader.IsDBNull(5) ? "" : reader.GetString(5),  // DepName
                                reader.IsDBNull(6) ? "" : reader.GetString(6)   // DepCode
                            );

                            var arrivalAirport = new AirportStruct(
                                reader.IsDBNull(7) ? "" : reader.GetString(7),  // ArrCountry
                                reader.IsDBNull(8) ? "" : reader.GetString(8),  // ArrCity
                                reader.IsDBNull(9) ? "" : reader.GetString(9),  // ArrName
                                reader.IsDBNull(10) ? "" : reader.GetString(10)   // ArrCode
                            );

                            var dateFlight = new List<DateTime> {
                                reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1),
                                reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2)
                            };

                            var flight = new FlightStruct(
                                departureAirport,
                                arrivalAirport,
                                dateFlight,
                                _filter.PersonClasses
                            );

                            flight.FlightId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            flight.AdultPrice = reader.IsDBNull(11) ? 0 : Convert.ToDouble(reader.GetDecimal(11));

                            flights.Add(flight);
                        }
                    }

                    FilteredFlights = flights;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження рейсів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void Price_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is FlightStruct flight)
            {
                var parentWindow = Window.GetWindow(this);
                if (parentWindow is MainWindow mainWindow)
                {
                    var selectPage = mainWindow.PageManager.Content as SelectPage;
                    if (selectPage != null)
                    {
                        selectPage.CurentPage = 3;
                        selectPage.PageManager.Navigate(new ChoosingSeatsPage(flight.FlightId, flight.CurrentClass));
                        selectPage.FillPath(3);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
