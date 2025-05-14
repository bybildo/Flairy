using CoursFlairy.Data;
using CoursFlairy.Model;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            public string RouteName { get => _routeName; set { _routeName = value; OnPropertyChanged(nameof(RouteName)); } }
            public int AmountTime { get => _amountTime; set { _amountTime = value; OnPropertyChanged(nameof(AmountTime)); } }
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

            public RouteStruct(string name, int time)
            {
                RouteName = name;
                AmountTime = time;
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
