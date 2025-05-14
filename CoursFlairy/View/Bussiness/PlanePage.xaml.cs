using CoursFlairy.Data;
using CoursFlairy.Model;
using CoursFlairy.Model.Enum;
using CoursFlairy.Model.UI;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CoursFlairy.View.Bussiness
{
    /// <summary>
    /// Interaction logic for PlanePage.xaml
    /// </summary>
    public partial class PlanePage : Page, INotifyPropertyChanged
    {
        private List<PlaneStruct> _planes = new List<PlaneStruct>();
        private string _planeName = "";
        private double _listItemHeight;

        public string PlaneName { get => _planeName; set { _planeName = value; OnPropertyChanged(nameof(PlaneName)); UpdatePlanes(); } }
        public List<PlaneStruct> Planes { get => _planes; set { _planes = value; OnPropertyChanged(nameof(Planes)); } }
        public double ListItemHeight { get => _listItemHeight; set { _listItemHeight = value; OnPropertyChanged(nameof(ListItemHeight)); } }

        public PlanePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ListItemHeight = PlanesGrid.ActualHeight / 7;
            UpdatePlanes();
        }

        private void AddPlane_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new AddPlanePage());
        }

        private void UpdatePlanes()
        {
            if (CurrentAccount.id == -1)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show("Ви не авторизовані");
                return;
            }

            string query = @"SELECT Name, Economy, Bussiness, First FROM Plane WHERE AirlineID = @airlineID AND Name LIKE @name";
            if (string.IsNullOrWhiteSpace(PlaneName))
            {
                query = @"SELECT Name, Economy, Bussiness, First FROM Plane WHERE AirlineID = @airlineID";
            }

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);
                if (!string.IsNullOrWhiteSpace(PlaneName))
                {
                    command.Parameters.AddWithValue("@name", $"%{PlaneName}%");
                }

                try
                {
                    List<PlaneStruct> result = new List<PlaneStruct>();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new PlaneStruct(reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3)));
                        }
                    }

                    Planes = result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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

        private void Seat_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid gridSeat = sender as Grid;
            Grid gridGlobal = ((Grid)gridSeat.Parent).Parent as Grid;

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[1], 0, 1, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[2], 0, 0.3, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[3], 0, 1, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[4], 0, 0.3, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[5], 0, 1, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[6], 0, 0.3, 0, 0.12));

            storyboard.Children.Add(AddAnimation(gridGlobal.ColumnDefinitions[1], 23, 20.5, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridGlobal.ColumnDefinitions[3], 1.4, 4, 0, 0.12));
            storyboard.Begin();
        }

        private void Seat_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid gridSeat = sender as Grid;
            Grid gridGlobal = ((Grid)gridSeat.Parent).Parent as Grid;

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[1], 1, 0, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[2], 0.3, 0, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[3], 1, 0, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[4], 0.3, 0, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[5], 1, 0, 0, 0.12));
            storyboard.Children.Add(AddAnimation(gridSeat.ColumnDefinitions[6], 0.3, 0, 0, 0.12));

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

        public class PlaneStruct : INotifyPropertyChanged
        {
            private string _planeName;
            private int _economySeats, _businessSeats, _firstSeats = 0;
            public string PlaneName { get => _planeName; set { _planeName = value; OnPropertyChanged(nameof(AllSeats)); OnPropertyChanged(nameof(PlaneName)); } }
            public object EconomySeats { get => _economySeats.ToString(); set { _economySeats = (int)value; OnPropertyChanged(nameof(AllSeats)); OnPropertyChanged(nameof(EconomySeats)); } }
            public object BusinessSeats { get => _businessSeats.ToString(); set { _businessSeats = (int)value; OnPropertyChanged(nameof(AllSeats)); OnPropertyChanged(nameof(BusinessSeats)); } }
            public object FirstSeats { get => _firstSeats.ToString(); set { _firstSeats = (int)value; OnPropertyChanged(nameof(AllSeats)); OnPropertyChanged(nameof(FirstSeats)); } }
            public string AllSeats { get => (_economySeats + _businessSeats + _firstSeats).ToString(); }

            public PlaneStruct(string planeName, int economySeats, int businessSeats, int firstSeats)
            {
                PlaneName = planeName;
                EconomySeats = economySeats;
                BusinessSeats = businessSeats;
                FirstSeats = firstSeats;
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
