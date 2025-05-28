using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using static CoursFlairy.ViewModel.ResourceColor;
using System.Windows.Input;
using CoursFlairy.Data;
using CoursFlairy.Model.Enum;
using Microsoft.Data.SqlClient;
using System.Numerics;
using System;
using System.Data;
using System.Windows.Media.Animation;
using System.Windows.Media;
using CoursFlairy.Model;
using static CoursFlairy.View.Bussiness.RoutePage;

namespace CoursFlairy.View.Bussiness
{
    /// <summary>
    /// Interaction logic for FlightPage.xaml
    /// </summary>
    public partial class FlightPage : Page, INotifyPropertyChanged
    {
        private List<FlightStruct> _flightsList = new List<FlightStruct>() {};
        private double _listItemHeight;
        private string _flightName = "";
        private string _searchDate = "";
        private int routeId = -1;
        private DateTime departureTime = DateTime.MinValue;
        private double addGridHeight = 0;
        private bool addGridActive = false;
        private FlightStruct _expandedFlight = null;

        private string _routeName = "";
        private string _routeHint = "";

        #region Властивості
        public List<FlightStruct> Flights { get { return _flightsList; } set { _flightsList = value; OnPropertyChanged(nameof(Flights)); } }
        public string FlightName { get { return _flightName; } set { _flightName = value; UpdateFlights(); OnPropertyChanged("FlightName"); } }
        public string SearchDate { get { return _searchDate; } set { _searchDate = value; UpdateFlights(); OnPropertyChanged("SearchDate"); } }
        public double ListItemHeight { get { return _listItemHeight; } set { _listItemHeight = value; OnPropertyChanged("ListItemHeight"); OnPropertyChanged("AddItemHeight"); } }
        public double AddItemHeight { get { return _listItemHeight * 1.2; } }


        public string RouteHint
        {
            get => _routeHint;
            set
            {
                _routeHint = value;
                if (RouteHint == RouteName && RouteName != "")
                {
                    PlaneBorder.BorderBrush = MainColor100;
                    RouteButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    PlaneBorder.BorderBrush = MainColor20;
                    RouteButton.Visibility = Visibility.Visible;
                }
                OnPropertyChanged(nameof(RouteHint));
            }
        }

        public string RouteName { get => _routeName; set { _routeName = value; RouteHint = ""; RouteHintUpdate(); OnPropertyChanged(nameof(RouteName)); } }
        #endregion

        public FlightPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ListItemHeight = FlightGrid.ActualHeight / 7;
            UpdateFlights();
        }

        #region Методи
        #region MouseDown
        private void DateGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch ((sender as Grid).Name)
            {
                case "DayGrid":
                    DayTb.Focus();
                    break;
                case "MonthGrid":
                    MonthTb.Focus();
                    break;
                case "HourGrid":
                    HourTb.Focus();
                    break;
                case "MinuteGrid":
                    MinuteTb.Focus();
                    break;
            }
        }

        private void AddButtonNotActive_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mainWindow = (App.Current.MainWindow as MainWindow);
            mainWindow.GlobalMessage.Show("Коректно заповніть форму");
        }

        private void AddButtonActive_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AddFlight();
        }

        private void AddFlight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (addGridActive == false)
            {
                var storyboard = new Storyboard();
                storyboard.Children.Add(AddAnimation(FlightGrid.RowDefinitions[1], 0, FlightGrid.ActualHeight / (AddItemHeight * 1.8), 0, 0.12));
                storyboard.Begin();
            }
            else
            {
                var storyboard = new Storyboard();
                storyboard.Children.Add(AddAnimation(FlightGrid.RowDefinitions[1], FlightGrid.ActualHeight / (AddItemHeight * 1.8), 0, 0, 0.15));
                storyboard.Begin();
            }
            addGridActive = !addGridActive;
        }

        private void ClearSearch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FlightName = "";
            SearchDate = "";
        }

        private void FlightItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is FlightStruct flightStruct)
            {
                ToggleFlightExpansion(flightStruct, border);
            }
        }

        private void ToggleFlightExpansion(FlightStruct flightStruct, Border clickedBorder)
        {
            // Find the ListBoxItem container
            var listBoxItem = FindParent<ListBoxItem>(clickedBorder);
            if (listBoxItem == null) return;

            var template = listBoxItem.Template;
            var expandedRow = template.FindName("ExpandedRow", listBoxItem) as RowDefinition;
            var flightDetailsBorder = template.FindName("FlightDetailsBorder", listBoxItem) as Border;

            if (expandedRow == null || flightDetailsBorder == null) return;

            // Collapse all other flights first
            foreach (var flight in Flights)
            {
                if (flight != flightStruct && flight.IsExpanded)
                {
                    var otherItem = FindListBoxItemForFlight(flight);
                    if (otherItem != null)
                    {
                        var otherTemplate = otherItem.Template;
                        var otherExpandedRow = otherTemplate.FindName("ExpandedRow", otherItem) as RowDefinition;
                        var otherDetailsBorder = otherTemplate.FindName("FlightDetailsBorder", otherItem) as Border;
                        if (otherExpandedRow != null && otherDetailsBorder != null)
                        {
                            otherExpandedRow.Height = new GridLength(0);
                            otherDetailsBorder.Opacity = 0;
                            flight.IsExpanded = false;
                        }
                    }
                }
            }

            if (flightStruct.IsExpanded)
            {
                // Collapse current flight
                expandedRow.Height = new GridLength(0);
                flightDetailsBorder.Opacity = 0;
                flightStruct.IsExpanded = false;
                _expandedFlight = null;
            }
            else
            {
                // Expand current flight
                LoadFlightDetails(flightStruct);
                expandedRow.Height = new GridLength(250);
                flightDetailsBorder.Opacity = 1;
                flightStruct.IsExpanded = true;
                _expandedFlight = flightStruct;
            }
        }

        private void LoadFlightDetails(FlightStruct flightStruct)
        {
            try
            {
                string query = @"
                    SELECT f.ID, 
                           dep_airport.ICAO as DepartureICAO, 
                           arr_airport.ICAO as ArrivalICAO, 
                           p.Name as PlaneName,
                           f.Status,
                           (p.Economy + p.Bussiness + p.First) as TotalSeats,
                           ISNULL(tickets_count.TicketsSold, 0) as TicketsSold
                    FROM Flight f
                    LEFT JOIN Route r ON f.RouteID = r.ID
                    LEFT JOIN Plane p ON r.PlaneID = p.ID
                    LEFT JOIN Airport dep_airport ON r.DepartureID = dep_airport.ID
                    LEFT JOIN Airport arr_airport ON r.ArrivalID = arr_airport.ID
                    LEFT JOIN (
                        SELECT FlightID, COUNT(*) as TicketsSold 
                        FROM Ticket 
                        GROUP BY FlightID
                    ) tickets_count ON f.ID = tickets_count.FlightID
                    WHERE f.DTime = @dTime AND r.Name = @routeName AND r.AirlineID = @airlineID";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@dTime", flightStruct.DTime);
                    command.Parameters.AddWithValue("@routeName", flightStruct.RouteName);
                    command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            flightStruct.FlightId = reader.GetInt32(0);
                            flightStruct.DepartureAirport = reader.IsDBNull(1) ? "Не вказано" : reader.GetString(1);
                            flightStruct.ArrivalAirport = reader.IsDBNull(2) ? "Не вказано" : reader.GetString(2);
                            flightStruct.PlaneName = reader.IsDBNull(3) ? "Не призначено" : reader.GetString(3);
                            
                            int status = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                            flightStruct.FlightStatus = GetFlightStatusText(status);
                            
                            flightStruct.TotalSeats = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                            flightStruct.TicketsSold = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження деталей рейсу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetFlightStatusText(int status)
        {
            return status switch
            {
                1 => "Заплановано",
                2 => "Затримано",
                3 => "Скасовано",
                4 => "В польоті",
                5 => "Прибув",
                _ => "Невідомо"
            };
        }

        private ListBoxItem FindListBoxItemForFlight(FlightStruct flight)
        {
            var listBox = FindChild<ListBox>(this);
            if (listBox == null) return null;

            for (int i = 0; i < listBox.Items.Count; i++)
            {
                var item = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (item?.DataContext == flight)
                {
                    return item;
                }
            }
            return null;
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        private static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    return result;
                }
                else
                {
                    var childResult = FindChild<T>(child);
                    if (childResult != null)
                        return childResult;
                }
            }
            return null;
        }
        #endregion

        #region TextChanged
        private void ColorUpdate_TextChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(DayTb.Text, out int day);
            int.TryParse(MonthTb.Text, out int month);
            DateTime date = DateTime.MinValue;

            try
            {
                date = new DateTime(DateTime.Now.Year, month, day);
                BorderAnimation(DayBorder, false);
                BorderAnimation(MonthBorder, false);
            }
            catch
            {
                BorderAnimation(DayBorder, true);
                BorderAnimation(MonthBorder, true);
            }

            if (date > DateTime.MinValue)
            {
                if (date < DateTime.Now)
                    YearTb.Text = $"{DateTime.Now.Year + 1}";
                else
                    YearTb.Text = $"{DateTime.Now.Year}";
            }

            int.TryParse(HourTb.Text, out int hour);
            int.TryParse(MinuteTb.Text, out int minute);
            TimeOnly time = TimeOnly.MinValue;

            if (HourTb.Text.Length > 0 && MinuteTb.Text.Length > 0)
            {
                time = new TimeOnly(hour, minute);
                BorderAnimation(HourBorder, false);
                BorderAnimation(MinuteBorder, false);
            }
            else
            {
                BorderAnimation(HourBorder, true);
                BorderAnimation(MinuteBorder, true);
            }

            if (routeId != -1)
            {
                int year = int.Parse(YearTb.Text);
                date = DateTime.MinValue;
                try
                {
                    if (HourTb.Text.Length == 0 || MinuteTb.Text.Length == 0) throw new Exception();
                    date = new DateTime(year, month, day, hour, minute, 0);
                    departureTime = date;
                    AddButton.Fill = MainColor100;

                    AddActiveGrid.MouseDown -= AddButtonActive_MouseDown;
                    AddActiveGrid.MouseDown -= AddButtonNotActive_MouseDown;

                    AddActiveGrid.MouseDown += AddButtonActive_MouseDown;
                }
                catch
                {
                    AddButton.Fill = MainColor20;

                    AddActiveGrid.MouseDown -= AddButtonActive_MouseDown;
                    AddActiveGrid.MouseDown -= AddButtonNotActive_MouseDown;

                    AddActiveGrid.MouseDown += AddButtonNotActive_MouseDown;
                }
            }
        }
        #endregion  

        #region TextInput
        private void DayTb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string text = DayTb.Text + e.Text;
            if (!int.TryParse(text, out int num)) { e.Handled = true; return; }

            if (num < 0 || num > 31) e.Handled = true;
        }

        private void MonthTb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string text = MonthTb.Text + e.Text;
            if (!int.TryParse(text, out int num)) { e.Handled = true; return; }

            if (num < 0 || num > 12) e.Handled = true;
        }

        private void HourTb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string text = HourTb.Text + e.Text;
            if (!int.TryParse(text, out int num)) { e.Handled = true; return; }

            if (num < 0 || num > 23) e.Handled = true;
        }

        private void MinuteTb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string text = MinuteTb.Text + e.Text;
            if (!int.TryParse(text, out int num)) { e.Handled = true; return; }

            if (num < 0 || num > 60) e.Handled = true;
        }

        private void DateSearch_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string currentText = textBox.Text;
            string newText = currentText + e.Text;

            // Allow only digits and dots
            if (!char.IsDigit(e.Text[0]) && e.Text[0] != '.')
            {
                e.Handled = true;
                return;
            }

            // Auto-format date as user types (dd.mm.yyyy)
            if (char.IsDigit(e.Text[0]))
            {
                int caretIndex = textBox.CaretIndex;
                
                // Auto-add dots after day and month
                if (caretIndex == 2 && currentText.Length == 2 && !currentText.Contains('.'))
                {
                    textBox.Text = currentText + ".";
                    textBox.CaretIndex = 3;
                }
                else if (caretIndex == 5 && currentText.Length == 5 && currentText.Count(c => c == '.') == 1)
                {
                    textBox.Text = currentText + ".";
                    textBox.CaretIndex = 6;
                }
            }

            // Validate date format length
            if (newText.Length > 10)
            {
                e.Handled = true;
            }
        }
        #endregion

        #region KeyControl
        private void KeyControl(object sender, KeyEventArgs e)
        {
            bool isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

            if (isCtrlPressed && (e.Key == Key.C || e.Key == Key.V))
            {
                e.Handled = true;
            }

            if (e.Key == Key.Tab) e.Handled = true;

            if (sender == routenametb)
            {
                if ((e.Key == Key.Tab || e.Key == Key.Enter))
                {
                    RouteName = RouteHint;
                    if (RouteName != "")
                        DayTb.Focus();
                }
                return;
            }

            Dictionary<TextBox, TextBox> textBoxes = new Dictionary<TextBox, TextBox>() { { DayTb, MonthTb }, { MonthTb, HourTb }, { HourTb, MinuteTb } };

            if ((e.Key == Key.Tab || e.Key == Key.Enter) && textBoxes.ContainsKey((TextBox)sender))
            {
                if ((sender as TextBox).Text.Length == 0) return;
                textBoxes[(TextBox)sender].Focus();
            }
        }
        #endregion

        #region База даних
        private void UpdateFlights()
        {
            if (CurrentAccount.id == -1)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show("Ви не авторизовані");
                return;
            }

            string query = @"
                SELECT r.Name, f.DTime, f.ATime, 
                       dep_airport.ICAO as DepartureICAO, 
                       arr_airport.ICAO as ArrivalICAO,
                       (p.Economy + p.Bussiness + p.First) as TotalSeats,
                       ISNULL(tickets_count.TicketsSold, 0) as TicketsSold
                FROM Flight f 
                JOIN Route r ON f.RouteID = r.ID 
                LEFT JOIN Plane p ON r.PlaneID = p.ID
                LEFT JOIN Airport dep_airport ON r.DepartureID = dep_airport.ID
                LEFT JOIN Airport arr_airport ON r.ArrivalID = arr_airport.ID
                LEFT JOIN (
                    SELECT FlightID, COUNT(*) as TicketsSold 
                    FROM Ticket 
                    GROUP BY FlightID
                ) tickets_count ON f.ID = tickets_count.FlightID
                WHERE r.AirlineID = @airlineID";

            if (!string.IsNullOrWhiteSpace(FlightName))
            {
                query += " AND r.Name LIKE @name";
            }

            if (!string.IsNullOrWhiteSpace(SearchDate))
            {
                query += " AND CAST(f.DTime AS DATE) = @searchDate";
            }

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);
                if (!string.IsNullOrWhiteSpace(FlightName))
                {
                    command.Parameters.AddWithValue("@name", $"%{FlightName}%");
                }

                if (!string.IsNullOrWhiteSpace(SearchDate))
                {
                    if (DateTime.TryParseExact(SearchDate, new string[] { "dd.MM.yyyy", "dd.MM.yy", "dd/MM/yyyy", "dd/MM/yy", "yyyy-MM-dd" }, 
                        System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime searchDateTime))
                    {
                        command.Parameters.AddWithValue("@searchDate", searchDateTime.Date);
                    }
                    else
                    {
                        // If date parsing fails, search will return no results
                        command.Parameters.AddWithValue("@searchDate", DateTime.MinValue);
                    }
                }

                try
                {
                    List<FlightStruct> result = new List<FlightStruct>();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var flight = new FlightStruct(reader.GetString(0), reader.GetDateTime(1), reader.GetDateTime(2));
                            flight.DepartureAirport = reader.IsDBNull(3) ? "Не вказано" : reader.GetString(3);
                            flight.ArrivalAirport = reader.IsDBNull(4) ? "Не вказано" : reader.GetString(4);
                            flight.TotalSeats = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                            flight.TicketsSold = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                            result.Add(flight);
                        }
                    }

                    Flights = result;
                  
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void AddFlight()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            if (DataBase.GetConnection().State != System.Data.ConnectionState.Open)
            {
                mainWindow.GlobalMessage.Show("Помилка з'єднання з базою даних");
                return;
            }

            if (CurrentAccount.id == -1 || CurrentAccount.accountType != AccountType.Bussines)
            {
                mainWindow.GlobalMessage.Show("Помилка авторизації");
                return;
            }

            if (routeId == -1)
            {
                mainWindow.GlobalMessage.Show("Оберіть коректний маршрут");
                return;
            }

            if (departureTime == DateTime.MinValue)
            {
                mainWindow.GlobalMessage.Show("Оберіть коректний час відправлення");
                return;
            }

            string checkQuery = @"SELECT COUNT(*) FROM [dbo].[Flight] WHERE RouteID = @routeId AND DTime BETWEEN DATEADD(MINUTE, -60, @dTime) AND DATEADD(MINUTE, 60, @dTime)";

            using (SqlCommand checkCommand = new SqlCommand(checkQuery, DataBase.GetConnection()))
            {
                checkCommand.Parameters.AddWithValue("@routeId", routeId);
                checkCommand.Parameters.AddWithValue("@dTime", departureTime);

                int existingFlights = (int)checkCommand.ExecuteScalar();

                if (existingFlights > 0)
                {
                    mainWindow.GlobalMessage.Show("Відправлення за таким часом і маршрутом вже існує");
                    return;
                }
            }

            string query = @"INSERT INTO [dbo].[Flight] ([RouteID], [DTime], [Status]) 
                VALUES (@routeId, @dTime, @status)";

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@routeId", routeId);
                command.Parameters.AddWithValue("@dTime", departureTime);
                command.Parameters.AddWithValue("@status", 1);

                try
                {
                    command.ExecuteNonQuery();
                    mainWindow.GlobalMessage.Show("Відправлення успішно додано");
                    RouteName = "";
                    RouteHint = "";
                    DayTb.Text = "";
                    MonthTb.Text = "";
                    HourTb.Text = "";
                    MinuteTb.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RouteHintUpdate()
        {
            if (DataBase.GetConnection().State != System.Data.ConnectionState.Open)
                return;

            if (CurrentAccount.id == -1 || CurrentAccount.accountType != AccountType.Bussines) return;

            if (RouteName == "")
            {
                RouteHint = "";
                RouteButton.Visibility = Visibility.Hidden;
                return;
            }

            string query = @"SELECT TOP 1 Name, ID FROM Route WHERE AirlineID = @airlineID AND Name LIKE @routeName";

            string routeHint = "$";
            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@routeName", $"{RouteName}%");
                command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);

                try
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string read = reader.GetString(0);
                            routeId = reader.GetInt32(1);
                            routeHint = RouteName + read.Substring(RouteName.Length);
                        }
                        else
                        {
                            RouteButton.Visibility = Visibility.Hidden;
                            routeId = -1;
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            if (routeHint != "$") RouteHint = routeHint;

            if (RouteHint == RouteName) RouteButton.Visibility = Visibility.Hidden;
            else routeId = -1;
        }
        #endregion

        #region Scroll
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (mouseEntered)
            {
                return;
            }

            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
        }

        private bool mouseEntered = false;
        private void ScrollViewer_MouseEnter(object sender, MouseEventArgs e)
        {
            mouseEntered = true;
        }

        private void ScrollViewer_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseEntered = false;
        }

        private void ScrollViewer_PreviewHorizontalMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta != 0)
            {
                var scrollViewer = sender as ScrollViewer;
                if (scrollViewer != null)
                {
                    if (e.Delta > 0)
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 5);
                    }
                    else
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 5);
                    }

                    e.Handled = true;
                }
            }
        }
        #endregion

        #region Анімація
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

        private GridLengthAnimation AddAnimation(ColumnDefinition column, double from, double to, double startTime, double duration)
        {
            var animation = new GridLengthAnimation
            {
                From = new GridLength(from, GridUnitType.Star),
                To = new GridLength(to, GridUnitType.Star),
                Duration = TimeSpan.FromSeconds(duration),
                BeginTime = TimeSpan.FromSeconds(startTime),
                FillBehavior = FillBehavior.HoldEnd
            };

            Storyboard.SetTarget(animation, column);
            Storyboard.SetTargetProperty(animation, new PropertyPath(ColumnDefinition.WidthProperty));
            return animation;
        }

        private GridLengthAnimation AddAnimation(RowDefinition row, double from, double to, double startTime, double duration)
        {
            var animation = new GridLengthAnimation
            {
                From = new GridLength(from, GridUnitType.Star),
                To = new GridLength(to, GridUnitType.Star),
                Duration = TimeSpan.FromSeconds(duration),
                BeginTime = TimeSpan.FromSeconds(startTime),
                FillBehavior = FillBehavior.HoldEnd
            };

            Storyboard.SetTarget(animation, row);
            Storyboard.SetTargetProperty(animation, new PropertyPath(RowDefinition.HeightProperty));
            return animation;
        }
        #endregion
        #endregion

        public class FlightStruct : INotifyPropertyChanged
        {
            private string _routeName;
            private DateTime _dTime;
            private DateTime _aTime;
            private bool _isExpanded = false;
            private int _flightId;
            private string _departureAirport;
            private string _arrivalAirport;
            private string _planeName;
            private int _ticketsSold = 0;
            private int _totalSeats = 0;
            private string _flightStatus;
            private decimal _economyPrice = 0;
            private decimal _businessPrice = 0;
            private decimal _firstPrice = 0;

            public string RouteName { get => _routeName; set { _routeName = value; OnPropertyChanged(nameof(RouteName)); } }
            public DateTime DTime { get => _dTime; set { _dTime = value; OnPropertyChanged(nameof(DTime)); OnPropertyChanged(nameof(RouteDisplayText)); OnPropertyChanged(nameof(DateDisplayText)); OnPropertyChanged(nameof(TimeDisplayText)); } }
            public DateTime ATime { get => _aTime; set { _aTime = value; OnPropertyChanged(nameof(ATime)); OnPropertyChanged(nameof(ArrivalTimeDisplayText)); } }
            public bool IsExpanded { get => _isExpanded; set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); } }
            public int FlightId { get => _flightId; set { _flightId = value; OnPropertyChanged(nameof(FlightId)); } }
            public string DepartureAirport { get => _departureAirport; set { _departureAirport = value; OnPropertyChanged(nameof(DepartureAirport)); OnPropertyChanged(nameof(RouteDisplayText)); } }
            public string ArrivalAirport { get => _arrivalAirport; set { _arrivalAirport = value; OnPropertyChanged(nameof(ArrivalAirport)); OnPropertyChanged(nameof(RouteDisplayText)); } }
            public string PlaneName { get => _planeName; set { _planeName = value; OnPropertyChanged(nameof(PlaneName)); } }
            public int TicketsSold { get => _ticketsSold; set { _ticketsSold = value; OnPropertyChanged(nameof(TicketsSold)); OnPropertyChanged(nameof(TicketsInfo)); } }
            public int TotalSeats { get => _totalSeats; set { _totalSeats = value; OnPropertyChanged(nameof(TotalSeats)); OnPropertyChanged(nameof(TicketsInfo)); } }
            public string FlightStatus { get => _flightStatus; set { _flightStatus = value; OnPropertyChanged(nameof(FlightStatus)); } }
            public decimal EconomyPrice { get => _economyPrice; set { _economyPrice = value; OnPropertyChanged(nameof(EconomyPrice)); } }
            public decimal BusinessPrice { get => _businessPrice; set { _businessPrice = value; OnPropertyChanged(nameof(BusinessPrice)); } }
            public decimal FirstPrice { get => _firstPrice; set { _firstPrice = value; OnPropertyChanged(nameof(FirstPrice)); } }

            // Display properties
            public string RouteDisplayText => $"{DepartureAirport} → {ArrivalAirport}";
            public string DateDisplayText => DTime.ToString("dd.MM.yyyy");
            public string TimeDisplayText => DTime.ToString("HH:mm");
            public string ArrivalTimeDisplayText => ATime.ToString("HH:mm");
            public string TicketsInfo => $"{TicketsSold}/{TotalSeats}";

            public FlightStruct() { }
            public FlightStruct(string routeName, DateTime dTime, DateTime aTime)
            {
                RouteName = routeName;
                DTime = dTime;
                ATime = aTime;
            }

            #region PropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
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
