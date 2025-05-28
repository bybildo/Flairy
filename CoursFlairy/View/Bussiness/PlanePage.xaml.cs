using CoursFlairy.Data;
using CoursFlairy.Model;
using CoursFlairy.Model.Enum;
using CoursFlairy.Model.UI;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private PlaneStruct _expandedPlane = null;

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

        private void PlaneItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is PlaneStruct planeStruct)
            {
                TogglePlaneExpansion(planeStruct, border);
            }
        }

        private void TogglePlaneExpansion(PlaneStruct planeStruct, Border clickedBorder)
        {
            // Find the ListBoxItem container
            var listBoxItem = FindParent<ListBoxItem>(clickedBorder);
            if (listBoxItem == null) return;

            var template = listBoxItem.Template;
            var expandedRow = template.FindName("ExpandedRow", listBoxItem) as RowDefinition;
            var cabinLayoutBorder = template.FindName("CabinLayoutBorder", listBoxItem) as Border;

            if (expandedRow == null || cabinLayoutBorder == null) return;

            // Collapse all other planes first
            foreach (var plane in Planes)
            {
                if (plane != planeStruct && plane.IsExpanded)
                {
                    var otherItem = FindListBoxItemForPlane(plane);
                    if (otherItem != null)
                    {
                        var otherTemplate = otherItem.Template;
                        var otherExpandedRow = otherTemplate.FindName("ExpandedRow", otherItem) as RowDefinition;
                        var otherCabinBorder = otherTemplate.FindName("CabinLayoutBorder", otherItem) as Border;
                        if (otherExpandedRow != null && otherCabinBorder != null)
                        {
                            otherExpandedRow.Height = new GridLength(0);
                            otherCabinBorder.Opacity = 0;
                            plane.IsExpanded = false;
                        }
                    }
                }
            }

            if (planeStruct.IsExpanded)
            {
                // Collapse current plane - ПРОСТО ЗАКРИТИ
                expandedRow.Height = new GridLength(0);
                cabinLayoutBorder.Opacity = 0;
                planeStruct.IsExpanded = false;
                _expandedPlane = null;
            }
            else
            {
                // Expand current plane - ПРОСТО ВІДКРИТИ
                GenerateCabinLayout(planeStruct);
                expandedRow.Height = new GridLength(300);
                cabinLayoutBorder.Opacity = 1;
                planeStruct.IsExpanded = true;
                _expandedPlane = planeStruct;
            }
        }

        private void GenerateCabinLayout(PlaneStruct planeStruct)
        {
            try
            {
                // Get the actual plane scheme from database (same as ChoosingSeatsPage)
                string query = @"
                    SELECT p.SeatScheme 
                    FROM Plane p
                    WHERE p.Name = @planeName AND p.AirlineID = @airlineID";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@planeName", planeStruct.PlaneName);
                    command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string scheme = reader.GetString(0);
                            ParsePlaneScheme(planeStruct, scheme);
                        }
                        else
                        {
                            // Fallback - generate basic scheme if no scheme found
                            GenerateBasicScheme(planeStruct);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback - generate basic scheme if error occurs
                GenerateBasicScheme(planeStruct);
            }
        }

        private void ParsePlaneScheme(PlaneStruct planeStruct, string scheme)
        {
            var planeSeatsList = new List<List<PlaneSeats>>();

            var rows = scheme.Split('|');
            double constructorRows = rows.Length;
            double constructorColumns = rows[0].Split(',').Length;

            // Set cabin dimensions
            planeStruct.CabinColumns = (int)constructorColumns;
            planeStruct.CabinWidth = constructorColumns * 60 + 2; // Same calculation as ChoosingSeatsPage

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

                    double seatSize = 60 - ((constructorColumns - 1) / 109.0) * 20; // Same as ChoosingSeatsPage
                    PlaneSeats planeSeat = new PlaneSeats(brush, letter.ToString(), seatNumber, seatSize, Classes.None);

                    if (firstRow)
                    {
                        planeSeat.PathAngleTransform = new RotateTransform { Angle = 270 };
                    }
                    rowSeats.Add(planeSeat);
                }
                firstRow = false;
                planeSeatsList.Add(rowSeats);
            }

            // Convert to our structure
            var cabinLayout = new ObservableCollection<ObservableCollection<PlaneSeats>>();
            foreach (var row in planeSeatsList)
            {
                var observableRow = new ObservableCollection<PlaneSeats>(row);
                cabinLayout.Add(observableRow);
            }

            planeStruct.CabinLayout = cabinLayout;
        }

        private void GenerateBasicScheme(PlaneStruct planeStruct)
        {
            // Fallback - generate basic scheme based on seat counts
            var economySeats = int.Parse(planeStruct.EconomySeats.ToString());
            var businessSeats = int.Parse(planeStruct.BusinessSeats.ToString());
            var firstSeats = int.Parse(planeStruct.FirstSeats.ToString());

            var cabinLayout = new ObservableCollection<ObservableCollection<PlaneSeats>>();
            
            int columns = 6; // Standard configuration
            planeStruct.CabinColumns = columns;
            planeStruct.CabinWidth = columns * 60 + 2;

            char[] seatLetters = { 'A', 'B', 'C', 'D', 'E', 'F' };
            int rowNumber = 1;

            // First class section (2 seats per row: A + F only)
            if (firstSeats > 0)
            {
                int firstRows = (int)Math.Ceiling((double)firstSeats / 2);
                for (int row = 0; row < firstRows; row++)
                {
                    var rowSeats = new ObservableCollection<PlaneSeats>();
                    int seatsInThisRow = Math.Min(2, firstSeats - (row * 2));
                    
                    for (int col = 0; col < columns; col++)
                    {
                        if ((col == 0 || col == 5) && (col < seatsInThisRow || (col == 5 && seatsInThisRow == 2)))
                        {
                            rowSeats.Add(new PlaneSeats(PlaneBrushes.first, seatLetters[col].ToString(), rowNumber, 60, Classes.None));
                        }
                        else
                        {
                            rowSeats.Add(new PlaneSeats(PlaneBrushes.empty, "", 0, 60, Classes.None));
                        }
                    }
                    cabinLayout.Add(rowSeats);
                    rowNumber++;
                }
            }

            // Business class section (4 seats per row: A, B, E, F)
            if (businessSeats > 0)
            {
                int businessRows = (int)Math.Ceiling((double)businessSeats / 4);
                for (int row = 0; row < businessRows; row++)
                {
                    var rowSeats = new ObservableCollection<PlaneSeats>();
                    int seatsInThisRow = Math.Min(4, businessSeats - (row * 4));
                    int seatIndex = 0;
                    
                    for (int col = 0; col < columns; col++)
                    {
                        if ((col == 0 || col == 1 || col == 4 || col == 5) && seatIndex < seatsInThisRow)
                        {
                            rowSeats.Add(new PlaneSeats(PlaneBrushes.bussiness, seatLetters[col].ToString(), rowNumber, 60, Classes.None));
                            seatIndex++;
                        }
                        else
                        {
                            rowSeats.Add(new PlaneSeats(PlaneBrushes.empty, "", 0, 60, Classes.None));
                        }
                    }
                    cabinLayout.Add(rowSeats);
                    rowNumber++;
                }
            }

            // Economy class section (6 seats per row: A, B, C, D, E, F)
            if (economySeats > 0)
            {
                int economyRows = (int)Math.Ceiling((double)economySeats / 6);
                for (int row = 0; row < economyRows; row++)
                {
                    var rowSeats = new ObservableCollection<PlaneSeats>();
                    int seatsInThisRow = Math.Min(6, economySeats - (row * 6));
                    
                    for (int col = 0; col < columns; col++)
                    {
                        if (col < seatsInThisRow)
                        {
                            rowSeats.Add(new PlaneSeats(PlaneBrushes.econom, seatLetters[col].ToString(), rowNumber, 60, Classes.None));
                        }
                        else
                        {
                            rowSeats.Add(new PlaneSeats(PlaneBrushes.empty, "", 0, 60, Classes.None));
                        }
                    }
                    cabinLayout.Add(rowSeats);
                    rowNumber++;
                }
            }

            planeStruct.CabinLayout = cabinLayout;
        }

        private ListBoxItem FindListBoxItemForPlane(PlaneStruct plane)
        {
            var listBox = FindChild<ListBox>(this);
            if (listBox == null) return null;

            for (int i = 0; i < listBox.Items.Count; i++)
            {
                var item = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (item?.DataContext == plane)
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

        private void CabinScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                // Check if Ctrl is pressed for horizontal scrolling
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta / 3);
                }
                else
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                }
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
            private ObservableCollection<ObservableCollection<PlaneSeats>> _cabinLayout;
            private int _cabinColumns;
            private double _cabinWidth;
            private bool _isExpanded = false;

            public string PlaneName { get => _planeName; set { _planeName = value; OnPropertyChanged(nameof(AllSeats)); OnPropertyChanged(nameof(PlaneName)); } }
            public object EconomySeats { get => _economySeats.ToString(); set { _economySeats = (int)value; OnPropertyChanged(nameof(AllSeats)); OnPropertyChanged(nameof(EconomySeats)); } }
            public object BusinessSeats { get => _businessSeats.ToString(); set { _businessSeats = (int)value; OnPropertyChanged(nameof(AllSeats)); OnPropertyChanged(nameof(BusinessSeats)); } }
            public object FirstSeats { get => _firstSeats.ToString(); set { _firstSeats = (int)value; OnPropertyChanged(nameof(AllSeats)); OnPropertyChanged(nameof(FirstSeats)); } }
            public string AllSeats { get => (_economySeats + _businessSeats + _firstSeats).ToString(); }
            public ObservableCollection<ObservableCollection<PlaneSeats>> CabinLayout 
            { 
                get => _cabinLayout; 
                set { _cabinLayout = value; OnPropertyChanged(nameof(CabinLayout)); } 
            }
            public int CabinColumns 
            { 
                get => _cabinColumns; 
                set { _cabinColumns = value; OnPropertyChanged(nameof(CabinColumns)); } 
            }
            public double CabinWidth 
            { 
                get => _cabinWidth; 
                set { _cabinWidth = value; OnPropertyChanged(nameof(CabinWidth)); } 
            }
            public bool IsExpanded 
            { 
                get => _isExpanded; 
                set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); } 
            }

            public PlaneStruct(string planeName, int economySeats, int businessSeats, int firstSeats)
            {
                PlaneName = planeName;
                EconomySeats = economySeats;
                BusinessSeats = businessSeats;
                FirstSeats = firstSeats;
                _cabinLayout = new ObservableCollection<ObservableCollection<PlaneSeats>>();
            }

            public event PropertyChangedEventHandler PropertyChanged;

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
