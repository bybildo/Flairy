using CoursFlairy.Data;
using CoursFlairy.Model;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CoursFlairy.View.Bussiness
{
    /// <summary>
    /// Interaction logic for RoutePage.xaml
    /// </summary>
    public partial class RoutePage : Page, INotifyPropertyChanged
    {
        private List<RouteStruct> _routes = new List<RouteStruct>();
        private string _routeName;
        private double _listItemHeight;
        private RouteStruct _expandedRoute = null;

        public string RouteName { get => _routeName; set { _routeName = value; UpdateRoute(); OnPropertyChanged(nameof(RouteName)); } }
        public List<RouteStruct> Routes { get => _routes; set { _routes = value; OnPropertyChanged(nameof(Routes)); } }
        public double ListItemHeight { get => _listItemHeight; set { _listItemHeight = value; OnPropertyChanged(nameof(ListItemHeight)); } }

        public RoutePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ListItemHeight = RouteGrid.ActualHeight / 7;
            UpdateRoute();
        }

        private void AddRoute_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new AddRoutePage());
        }

        private void RouteItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is RouteStruct routeStruct)
            {
                ToggleRouteExpansion(routeStruct, border);
            }
        }

        private void ToggleRouteExpansion(RouteStruct routeStruct, Border clickedBorder)
        {
            // Find the ListBoxItem container
            var listBoxItem = FindParent<ListBoxItem>(clickedBorder);
            if (listBoxItem == null) return;

            var template = listBoxItem.Template;
            var expandedRow = template.FindName("ExpandedRow", listBoxItem) as RowDefinition;
            var routeDetailsBorder = template.FindName("RouteDetailsBorder", listBoxItem) as Border;

            if (expandedRow == null || routeDetailsBorder == null) return;

            // Collapse all other routes first
            foreach (var route in Routes)
            {
                if (route != routeStruct && route.IsExpanded)
                {
                    var otherItem = FindListBoxItemForRoute(route);
                    if (otherItem != null)
                    {
                        var otherTemplate = otherItem.Template;
                        var otherExpandedRow = otherTemplate.FindName("ExpandedRow", otherItem) as RowDefinition;
                        var otherDetailsBorder = otherTemplate.FindName("RouteDetailsBorder", otherItem) as Border;
                        if (otherExpandedRow != null && otherDetailsBorder != null)
                        {
                            otherExpandedRow.Height = new GridLength(0);
                            otherDetailsBorder.Opacity = 0;
                            route.IsExpanded = false;
                        }
                    }
                }
            }

            if (routeStruct.IsExpanded)
            {
                // Collapse current route
                expandedRow.Height = new GridLength(0);
                routeDetailsBorder.Opacity = 0;
                routeStruct.IsExpanded = false;
                _expandedRoute = null;
            }
            else
            {
                // Expand current route
                LoadRouteDetails(routeStruct);
                expandedRow.Height = new GridLength(200);
                routeDetailsBorder.Opacity = 1;
                routeStruct.IsExpanded = true;
                _expandedRoute = routeStruct;
            }
        }

        private void LoadRouteDetails(RouteStruct routeStruct)
        {
            try
            {
                string query = @"
                    SELECT r.ID, 
                           dep_airport.ICAO as DepartureICAO, 
                           arr_airport.ICAO as ArrivalICAO, 
                           p.Name as PlaneName
                    FROM Route r
                    LEFT JOIN Plane p ON r.PlaneID = p.ID
                    LEFT JOIN Airport dep_airport ON r.DepartureID = dep_airport.ID
                    LEFT JOIN Airport arr_airport ON r.ArrivalID = arr_airport.ID
                    WHERE r.Name = @routeName AND r.AirlineID = @airlineID";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@routeName", routeStruct.RouteName);
                    command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            routeStruct.RouteId = reader.GetInt32(0);
                            routeStruct.DepartureAirport = reader.IsDBNull(1) ? "Не вказано" : reader.GetString(1);
                            routeStruct.ArrivalAirport = reader.IsDBNull(2) ? "Не вказано" : reader.GetString(2);
                            routeStruct.PlaneName = reader.IsDBNull(3) ? "Не призначено" : reader.GetString(3);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження деталей маршруту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ListBoxItem FindListBoxItemForRoute(RouteStruct route)
        {
            var listBox = FindChild<ListBox>(this);
            if (listBox == null) return null;

            for (int i = 0; i < listBox.Items.Count; i++)
            {
                var item = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (item?.DataContext == route)
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

        private void UpdateRoute()
        {
            if (CurrentAccount.id == -1)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show("Ви не авторизовані");
                return;
            }

            string query = @"SELECT Name, AmountTime FROM Route WHERE AirlineID = @airlineID AND Name LIKE @name";
            if (string.IsNullOrWhiteSpace(RouteName))
            {
                query = @"SELECT Name, AmountTime FROM Route WHERE AirlineID = @airlineID";
            }

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);
                if (!string.IsNullOrWhiteSpace(RouteName))
                {
                    command.Parameters.AddWithValue("@name", $"%{RouteName}%");
                }

                try
                {
                    List<RouteStruct> result = new List<RouteStruct>();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new RouteStruct(reader.GetString(0), reader.GetInt32(1)));
                        }
                    }

                    Routes = result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid gridSeat = sender as Grid;
            Grid gridGlobal = ((Grid)gridSeat.Parent).Parent as Grid;

            Storyboard storyboard = new Storyboard();

            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[0], 0, 4, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[1], 1, 0, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridGlobal.ColumnDefinitions[1], 23, 20.5, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridGlobal.ColumnDefinitions[3], 1.4, 4, 0, 0.12));
            storyboard.Begin();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid gridSeat = sender as Grid;
            Grid gridGlobal = ((Grid)gridSeat.Parent).Parent as Grid;

            Storyboard storyboard = new Storyboard();

            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[0], 4, 0, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[1], 0, 1, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridGlobal.ColumnDefinitions[1], 20.5, 23, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridGlobal.ColumnDefinitions[3], 4, 1.4, 0, 0.12));
            storyboard.Begin();
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

        public class RouteStruct : INotifyPropertyChanged
        {
            private string _routeName;
            private int _amountTime;
            private bool _isExpanded = false;
            private string _departureAirport;
            private string _arrivalAirport;
            private string _planeName;
            private int _routeId;

            public string RouteName { get => _routeName; set { _routeName = value; OnPropertyChanged(nameof(RouteName)); } }
            public int AmountTime { get => _amountTime; set { _amountTime = value; OnPropertyChanged(nameof(AmountTime)); OnPropertyChanged(nameof(TimeString)); } }
            public bool IsExpanded { get => _isExpanded; set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); } }
            public string DepartureAirport { get => _departureAirport; set { _departureAirport = value; OnPropertyChanged(nameof(DepartureAirport)); } }
            public string ArrivalAirport { get => _arrivalAirport; set { _arrivalAirport = value; OnPropertyChanged(nameof(ArrivalAirport)); } }
            public string PlaneName { get => _planeName; set { _planeName = value; OnPropertyChanged(nameof(PlaneName)); } }
            public int RouteId { get => _routeId; set { _routeId = value; OnPropertyChanged(nameof(RouteId)); } }

            public string TimeString
            {
                get
                {
                    int hours = AmountTime / 60;
                    int minutes = AmountTime % 60;
                    if (hours == 0)
                    {
                        return $"{minutes} хв";
                    }
                    if (minutes == 0)
                    {
                        return $"{hours} год";
                    }
                    return $"{hours} год {minutes} хв";
                }
            }

            public RouteStruct(string routeName, int amountTime)
            {
                RouteName = routeName;
                AmountTime = amountTime;
            }

            public RouteStruct() { }

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
