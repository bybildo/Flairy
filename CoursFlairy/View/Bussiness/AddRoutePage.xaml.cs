using CoursFlairy.Model;
using CoursFlairy.Model.Enum;
using static CoursFlairy.ViewModel.ResourceColor;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using CoursFlairy.Data;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Threading;

namespace CoursFlairy.View.Bussiness
{
    /// <summary>
    /// Interaction logic for AddRoutePage.xaml
    /// </summary>
    public partial class AddRoutePage : Page, INotifyPropertyChanged
    {
        private Dictionary<Classes, ClassesPrice> _classesPricesDictionary = new Dictionary<Classes, ClassesPrice>();

        private bool handLuggage = false;
        private bool bigLuggage = false;

        private bool planeEntered = false;
        private ClassesPrice _classesPrice = new ClassesPrice();

        private string _routeName = "";
        private string _planeName = "";
        private string _planeHint = "";
        private double _seatButtonSize = 0;
        private int PlaneID = -1;
        private int _time = 0;
        private Classes _curentClass = Classes.Suitcase;

        #region Властивості
        public string PlaneHint
        {
            get => _planeHint;
            set
            {
                _planeHint = value;
                if (PlaneHint == PlaneName && PlaneName != "")
                {
                    PlaneBorder.BorderBrush = MainColor100;
                    UpGrid3 = true;
                    PlaneButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    PlaneBorder.BorderBrush = MainColor20;
                    UpGrid3 = false;
                    PlaneButton.Visibility = Visibility.Visible;
                }
                OnPropertyChanged(nameof(PlaneHint));
            }
        }

        public ClassesPrice ClassesPrices
        {
            get
            {
                if (CurentClass == Classes.Suitcase)
                {
                    ToSuitcase(true);
                    if (_classesPricesDictionary.ContainsKey(CurentClass))
                        return _classesPricesDictionary[CurentClass];
                    else
                        return new ClassesPrice();
                }

                ToSuitcase(false);
                if (_classesPricesDictionary.ContainsKey(CurentClass))
                    return _classesPricesDictionary[CurentClass];
                else
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow.GlobalMessage.Show("Помилка класу");
                    return new ClassesPrice();
                }
            }

            set
            {
                if (value != null)
                {
                    _classesPricesDictionary[CurentClass] = value;
                }
                else
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow.GlobalMessage.Show("Помилка: не можна встановити null значення");
                }
            }
        }

        public Classes CurentClass
        {
            get => _curentClass;
            set
            {
                SetBackgroundColor(_curentClass);
                _curentClass = value;
                SetBackgroundColor(_curentClass);
                OnPropertyChanged(nameof(ClassesPrices));
            }
        }

        public int Time
        {
            get => _time; set
            {
                if (value > 0)
                {
                    _time = value;
                    AddRouteButton.Visibility = Visibility.Visible;
                }
                else
                {
                    _time = 0;
                    AddRouteButton.Visibility = Visibility.Collapsed;
                }
                if (_time > 1440) _time = 1440;
                OnPropertyChanged("Time");
                OnPropertyChanged("TimeString");
            }
        }
        public string TimeString
        {
            get
            {
                int hours = _time / 60;
                int minutes = _time % 60;
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

        public string RouteName { get => _routeName; set { _routeName = value; OnPropertyChanged("RouteName"); } }
        public string PlaneName { get => _planeName; set { _planeName = value; PlaneHint = ""; PlaneHintUpdate(); OnPropertyChanged("PlaneName"); } }
        public double SeatButtonSize { get => _seatButtonSize; set { _seatButtonSize = value; OnPropertyChanged("SeatButtonSize"); OnPropertyChanged("BaggagePrice"); } }
        public double BaggagePrice { get => _seatButtonSize * 0.5; }
        private bool UpGrid3 { get => planeEntered; set { planeEntered = value; UpdateUpMenu(); } }
        #endregion

        public AddRoutePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AirportPicker.ToRouteGrid();
        }

        #region Методи

        #region MouseDown
        private void AddRouteEnds_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataBase.GetConnection().State != System.Data.ConnectionState.Open)
                return;

            if (CurrentAccount.id == -1 || CurrentAccount.accountType != AccountType.Bussines) return;

            string query = @"INSERT INTO [dbo].[Route] ([Name], [AirlineID], [PlaneID], [DepartureID], [ArrivalID], [AmountTime], [BackpackIncluded]) VALUES (@name, @airlineID, @planeID, @depID, @arrID, @time, @backpack)";

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@name", RouteName);
                command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);
                command.Parameters.AddWithValue("@planeID", PlaneID);
                command.Parameters.AddWithValue("@depID", AirportPicker.DepartureAirport.Id);
                command.Parameters.AddWithValue("@arrID", AirportPicker.ArrivalAirport.Id);
                command.Parameters.AddWithValue("@time", Time);
                command.Parameters.AddWithValue("@backpack", handLuggage);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            int routeID = 0;
            query = @"SELECT TOP 1 ID FROM [dbo].[Route] ORDER BY ID DESC;";

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                try
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        routeID = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (routeID == 0)
            {
                MessageBox.Show("Помилка зчитування ID маршруту", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (_classesPricesDictionary.ContainsKey(Classes.Econom))
            {
                query = @"INSERT INTO [dbo].[Price] ([RouteID], [Pensioner], [Adult], [Child], [Baby], [BaggageIncluded], [Baggage], [ClassGroup] ) VALUES (@routeID, @pensioner, @adult, @child, @baby, @baggageIncluded, @baggage, @classGroup)";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@routeID", routeID);
                    command.Parameters.AddWithValue("@pensioner", _classesPricesDictionary[Classes.Econom].PensionerMoney);
                    command.Parameters.AddWithValue("@adult", _classesPricesDictionary[Classes.Econom].AdultMoney);
                    command.Parameters.AddWithValue("@child", _classesPricesDictionary[Classes.Econom].ChildMoney);
                    command.Parameters.AddWithValue("@baby", _classesPricesDictionary[Classes.Econom].BabyMoney);
                    command.Parameters.AddWithValue("@baggageIncluded", bigLuggage);
                    command.Parameters.AddWithValue("@baggage", _classesPricesDictionary[Classes.Suitcase][Classes.Econom]);
                    command.Parameters.AddWithValue("@classGroup", (int)Classes.Econom);

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            if (_classesPricesDictionary.ContainsKey(Classes.Bussiness))
            {
                query = @"INSERT INTO [dbo].[Price] ([RouteID], [Pensioner], [Adult], [Child], [Baby], [BaggageIncluded], [Baggage], [ClassGroup] ) VALUES (@routeID, @pensioner, @adult, @child, @baby, @baggageIncluded, @baggage, @classGroup)";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@routeID", routeID);
                    command.Parameters.AddWithValue("@pensioner", _classesPricesDictionary[Classes.Bussiness].PensionerMoney);
                    command.Parameters.AddWithValue("@adult", _classesPricesDictionary[Classes.Bussiness].AdultMoney);
                    command.Parameters.AddWithValue("@child", _classesPricesDictionary[Classes.Bussiness].ChildMoney);
                    command.Parameters.AddWithValue("@baby", _classesPricesDictionary[Classes.Bussiness].BabyMoney);
                    command.Parameters.AddWithValue("@baggageIncluded", bigLuggage);
                    command.Parameters.AddWithValue("@baggage", _classesPricesDictionary[Classes.Suitcase][Classes.Bussiness]);
                    command.Parameters.AddWithValue("@classGroup", (int)Classes.Bussiness);

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            if (_classesPricesDictionary.ContainsKey(Classes.First))
            {
                query = @"INSERT INTO [dbo].[Price] ([RouteID], [Pensioner], [Adult], [Child], [Baby], [BaggageIncluded], [Baggage], [ClassGroup] ) VALUES (@routeID, @pensioner, @adult, @child, @baby, @baggageIncluded, @baggage, @classGroup)";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@routeID", routeID);
                    command.Parameters.AddWithValue("@pensioner", _classesPricesDictionary[Classes.First].PensionerMoney);
                    command.Parameters.AddWithValue("@adult", _classesPricesDictionary[Classes.First].AdultMoney);
                    command.Parameters.AddWithValue("@child", _classesPricesDictionary[Classes.First].ChildMoney);
                    command.Parameters.AddWithValue("@baby", _classesPricesDictionary[Classes.First].BabyMoney);
                    command.Parameters.AddWithValue("@baggageIncluded", bigLuggage);
                    command.Parameters.AddWithValue("@baggage", _classesPricesDictionary[Classes.Suitcase][Classes.First]);
                    command.Parameters.AddWithValue("@classGroup", (int)Classes.First);

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            this.NavigationService.Navigate(new RoutePage());
        }

        private void BackAddRouteEnd_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            EndAddRouteGrid.Visibility = Visibility.Collapsed;
        }

        private void AddRoute_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (RouteName == "")
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show("Введіть назву маршруту");
                return;
            }

            if (AirportPicker.DepartureAirport == null || AirportPicker.ArrivalAirport == null)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show("Оберіть аеропорти");
                return;
            }

            if (AirportPicker.DepartureAirport == AirportPicker.ArrivalAirport)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show("Аеропорти повинні бути різні");
                return;
            }

            if (CheckRouteNameAvailability())
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show("У вашій колекції вже є маршрут з таким іменем");
                return;
            }

            Page_MouseDown(null, null);
            Keyboard.ClearFocus();
            EndAddRouteGrid.Visibility = Visibility.Visible;
            List<Classes> lst = new List<Classes> { Classes.Econom, Classes.Bussiness, Classes.First };

            int row = 1;
            ResultTableGrid.Children.Clear();
            for (int i = 0; i < lst.Count; i++)
            {
                if (_classesPricesDictionary.ContainsKey(lst[i]))
                {
                    var info = _classesPricesDictionary[lst[i]];
                    for (int j = 0; j < 4; j++)
                    {
                        string money = info[j];
                        TextBlock textBlock = new TextBlock { Text = money + "₴" };
                        if (money == "0")
                        {
                            textBlock.Foreground = new SolidColorBrush(Colors.Orange);
                            textBlock.ToolTip = "Ви впевнені що хочете зробити проліт\nбезкоштовним для цієї категорії?";
                        }

                        ScrollViewer scrollViewer = new ScrollViewer { Content = textBlock };
                        Grid.SetRow(scrollViewer, row);
                        Grid.SetColumn(scrollViewer, j); ;
                        ResultTableGrid.Children.Add(scrollViewer);
                    }

                    if (bigLuggage == false)
                    {
                        TextBlock textBlock = new TextBlock { Text = "-" };
                        ScrollViewer scrollViewer = new ScrollViewer { Content = textBlock };
                        Grid.SetRow(scrollViewer, row);
                        Grid.SetColumn(scrollViewer, 5); ;
                        ResultTableGrid.Children.Add(scrollViewer);
                    }
                    else
                    {
                        TextBlock textBlock = new TextBlock { Text = _classesPricesDictionary[Classes.Suitcase][lst[i]] + "₴" };
                        ScrollViewer scrollViewer = new ScrollViewer { Content = textBlock };
                        Grid.SetRow(scrollViewer, row);
                        Grid.SetColumn(scrollViewer, 5); ;
                        ResultTableGrid.Children.Add(scrollViewer);
                    }
                    row += 2;
                }
            }
        }

        private DispatcherTimer timer;
        private int holdCounter;
        private string currentBorderName;
        private bool isInitialClickProcessed;

        private void p10_MouseLeave(object sender, MouseEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        private void Minute_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            currentBorderName = border.Name;
            holdCounter = 0;
            isInitialClickProcessed = false;

            UpdateTime(currentBorderName);
            isInitialClickProcessed = true;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.07);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Minute_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            holdCounter++;

            if (holdCounter < 5)
            {
                return;
            }

            int difference;
            if (currentBorderName == "m10" || currentBorderName == "p10")
            {
                difference = holdCounter > 30 ? 10 : 5;
            }
            else
            {
                difference = holdCounter > 30 ? 5 : 1;
            }

            switch (currentBorderName)
            {
                case "m10":
                    Time -= difference;
                    break;
                case "m1":
                    Time -= difference;
                    break;
                case "p1":
                    Time += difference;
                    break;
                case "p10":
                    Time += difference;
                    break;
            }
        }

        private void UpdateTime(string borderName)
        {
            switch (borderName)
            {
                case "m10":
                    Time -= 10;
                    break;
                case "m1":
                    Time -= 1;
                    break;
                case "p1":
                    Time += 1;
                    break;
                case "p10":
                    Time += 10;
                    break;
            }
        }

        private void BackPack_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            handLuggage = !handLuggage;

            if (handLuggage)
            {
                Backpack.Fill = new SolidColorBrush(Colors.LimeGreen);
            }
            else
            {
                Backpack.Fill = HintColor;
            }
        }

        private void BigSuitcase_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            bigLuggage = !bigLuggage;

            if (bigLuggage)
            {
                BigSuitcase.Stroke = new SolidColorBrush(Colors.LimeGreen);
                BigSuitcaseActivated();

            }
            else
            {
                BigSuitcaseActivated(false);
                BigSuitcase.Stroke = HintColor;
            }
        }

        private bool PageMouseDown = false;
        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PageMouseDown = true;
            HidenButton.Focus();
            AirportPicker.TriggerUpdate();
            PageMouseDown = false;
        }

        private void BackAddPlane_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void SeatsBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                switch (border.Name)
                {
                    case "EconomBorder":
                        CurentClass = Classes.Econom;
                        break;
                    case "BussinessBorder":
                        CurentClass = Classes.Bussiness;
                        break;
                    case "FirstBorder":
                        CurentClass = Classes.First;
                        break;
                    case "SuitcaseBorder":
                        CurentClass = Classes.Suitcase;
                        break;
                }
            }
            return;
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

            if ((e.Key == Key.Tab || e.Key == Key.Enter))
            {
                PlaneName = PlaneHint;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

            if (isCtrlPressed && (e.Key == Key.C || e.Key == Key.V))
            {
                e.Handled = true;
            }

            if (e.Key == Key.Enter) Page_MouseDown(null, null);
        }
        #endregion

        #region Focus
        private bool IsAirportAnimationPlayed = false;
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (IsAirportAnimationPlayed) return;

            IsAirportAnimationPlayed = true;
            var storyboard = new Storyboard();

            storyboard.Children.Add(AddAnimation(AirportGrid.ColumnDefinitions[0], 0.25, 0.32, 0, 0.12));
            storyboard.Children.Add(AddAnimation(AirportGrid.ColumnDefinitions[2], 0, 1.1, 0, 0.12));
            storyboard.Children.Add(AddAnimation(AirportGrid.ColumnDefinitions[3], 0.1, 0, 0, 0.12));
            storyboard.Children.Add(AddAnimation(AirportGrid.ColumnDefinitions[4], 1, 0, 0, 0.12));
            storyboard.Children.Add(AddAnimation(AirportGrid.ColumnDefinitions[5], 0.25, 0.32, 0, 0.12));

            storyboard.Begin();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DepartureTextBox.IsFocused || ArrivalTextBox.IsFocused || !IsAirportAnimationPlayed) return;

                IsAirportAnimationPlayed = false;
                var storyboard = new Storyboard();

                storyboard.Children.Add(AddAnimation(AirportGrid.ColumnDefinitions[0], 0.32, 0.25, 0, 0.12));
                storyboard.Children.Add(AddAnimation(AirportGrid.ColumnDefinitions[2], 1.1, 0, 0, 0.12));
                storyboard.Children.Add(AddAnimation(AirportGrid.ColumnDefinitions[3], 0, 0.1, 0, 0.12));
                storyboard.Children.Add(AddAnimation(AirportGrid.ColumnDefinitions[4], 0, 1, 0, 0.12));
                storyboard.Children.Add(AddAnimation(AirportGrid.ColumnDefinitions[5], 0.32, 0.25, 0, 0.12));

                storyboard.Begin();
            }));
        }
        #endregion

        #region TextChanged
        private void NumberPercentTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            Regex regex = new Regex(@"^([0-9]{1,1}(\.[0-9]{0,2})?)?$");
            e.Handled = !regex.IsMatch(newText);
        }
        private void NumberMoneyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            Regex regex = new Regex(@"^([0-9]{1,10}(\.[0-9]{0,2})?)?$");
            e.Handled = !regex.IsMatch(newText);
        }
        #endregion

        #region Airport
        private void AiroportPicker_DepartureSelect(object sender, EventArgs e)
        {
            DepartureBorder.BorderBrush = MainColor100;

            if (PageMouseDown) return;
            if (string.IsNullOrWhiteSpace(ArrivalTextBox.Text)) ArrivalTextBox.Focus(); ;
        }

        private void AiroportPicker_DepartureUnselect(object sender, EventArgs e)
        {
            DepartureBorder.BorderBrush = MainColor20;
        }

        private void AiroportPicker_ArrivalSelect(object sender, EventArgs e)
        {
            ArrivalBorder.BorderBrush = MainColor100;

            if (PageMouseDown) return;
            if (string.IsNullOrWhiteSpace(DepartureTextBox.Text)) DepartureTextBox.Focus();
        }

        private void AiroportPicker_ArrivalUnselect(object sender, EventArgs e)
        {
            ArrivalBorder.BorderBrush = MainColor20;
        }

        private void ToDeparture(object sender, MouseButtonEventArgs e)
        {
            AirportPicker.ToDeparture();
        }

        private void ToArrival(object sender, MouseButtonEventArgs e)
        {
            AirportPicker.ToArrival();
        }
        #endregion

        #region Приватні методи
        private bool CheckRouteNameAvailability()
        {
            if (CurrentAccount.id == -1) throw new Exception("Аккаунт не обраний");

            string query = "SELECT dbo.check_routename_availability(@airlineID, @name)";

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);
                command.Parameters.AddWithValue("@name", RouteName);
                return (bool)command.ExecuteScalar();
            }
        }

        private void BigSuitcaseActivated(bool toSuitcase = true)
        {
            var storyboard = new Storyboard();

            if (toSuitcase)
            {
                storyboard.Children.Add(AddAnimation(LaggageGrid.RowDefinitions[0], 2.15, 0.5, 0, 0.12));
                storyboard.Children.Add(AddAnimation(LaggageGrid.RowDefinitions[2], 0, 4, 0, 0.12));
                storyboard.Children.Add(AddAnimation(LaggageGrid.RowDefinitions[3], 2.35, 0, 0, 0.12));
            }
            else
            {
                storyboard.Children.Add(AddAnimation(LaggageGrid.RowDefinitions[0], 0.5, 2.15, 0, 0.12));
                storyboard.Children.Add(AddAnimation(LaggageGrid.RowDefinitions[2], 4, 0, 0, 0.12));
                storyboard.Children.Add(AddAnimation(LaggageGrid.RowDefinitions[3], 0, 2.35, 0, 0.12));
            }

            storyboard.Begin();
        }

        private bool isSuitcase = true;
        private void ToSuitcase(bool toSuitcase = true)
        {
            if (toSuitcase)
            {
                if (isSuitcase) return;
                var storyboard = new Storyboard();

                storyboard.Children.Add(AddAnimation(SeatsGrid.ColumnDefinitions[1], 2, 0, 0, 0.12));
                storyboard.Children.Add(AddAnimation(SeatsGrid.ColumnDefinitions[2], 0, 2, 0, 0.12));
                storyboard.Begin();
                isSuitcase = true;
            }
            else
            {
                if (!isSuitcase) return;
                var storyboard = new Storyboard();

                storyboard.Children.Add(AddAnimation(SeatsGrid.ColumnDefinitions[1], 0, 2, 0, 0.12));
                storyboard.Children.Add(AddAnimation(SeatsGrid.ColumnDefinitions[2], 2, 0, 0, 0.12));
                storyboard.Begin();

                isSuitcase = false;
            }
        }

        private bool isUpGrid = false;
        private void UpdateUpMenu()
        {
            if (UpGrid3)
            {
                (bool, bool, bool) seats = GetPlaneSeats();

                _classesPricesDictionary.Add(Classes.Suitcase, new ClassesPrice());

                if (seats.Item1)
                {
                    EconomBorder.Visibility = Visibility.Visible;
                    if (!_classesPricesDictionary.ContainsKey(Classes.Econom))
                        _classesPricesDictionary.Add(Classes.Econom, new ClassesPrice());
                }
                else
                {
                    EconomBorder.Visibility = Visibility.Collapsed;
                }

                if (seats.Item2)
                {
                    BussinessBorder.Visibility = Visibility.Visible;
                    if (!_classesPricesDictionary.ContainsKey(Classes.Bussiness))
                        _classesPricesDictionary.Add(Classes.Bussiness, new ClassesPrice());
                }
                else
                {
                    BussinessBorder.Visibility = Visibility.Collapsed;
                }

                if (seats.Item3)
                {
                    FirstBorder.Visibility = Visibility.Visible;
                    if (!_classesPricesDictionary.ContainsKey(Classes.First))
                        _classesPricesDictionary.Add(Classes.First, new ClassesPrice());
                }
                else
                {
                    FirstBorder.Visibility = Visibility.Collapsed;
                }
                if (isUpGrid) return;

                foreach (var kvp in _classesPricesDictionary)
                {
                    ClassesPrice currentClassPrice = kvp.Value;
                    currentClassPrice.team.Clear();

                    foreach (var neighbor in _classesPricesDictionary)
                    {
                        if (neighbor.Key != kvp.Key)
                        {
                            currentClassPrice.team.Add(neighbor.Value);
                        }
                    }
                }

                var storyboard = new Storyboard();

                storyboard.Children.Add(AddAnimation(MainGrid.RowDefinitions[0], 0.8, 0.2, 0, 0.12));
                storyboard.Children.Add(AddAnimation(MainGrid.RowDefinitions[2], 0, 0.2, 0, 0.12));
                storyboard.Children.Add(AddAnimation(MainGrid.RowDefinitions[3], 0, 1.4, 0, 0.12));
                storyboard.Children.Add(AddAnimation(MainGrid.RowDefinitions[4], 1.1, 0.1, 0, 0.12));

                storyboard.Begin();
                isUpGrid = true;
            }
            else
            {
                if (!isUpGrid) return;

                var storyboard = new Storyboard();

                storyboard.Children.Add(AddAnimation(MainGrid.RowDefinitions[0], 0.2, 0.8, 0, 0.12));
                storyboard.Children.Add(AddAnimation(MainGrid.RowDefinitions[2], 0.2, 0, 0, 0.12));
                storyboard.Children.Add(AddAnimation(MainGrid.RowDefinitions[3], 1.4, 0, 0, 0.12));
                storyboard.Children.Add(AddAnimation(MainGrid.RowDefinitions[4], 0.1, 1.1, 0, 0.12));

                storyboard.Begin();
                isUpGrid = false;
                CurentClass = Classes.Suitcase;
                _classesPricesDictionary.Clear();
            }
        }

        private (bool, bool, bool) GetPlaneSeats()
        {
            if (CurrentAccount.id == -1 || CurrentAccount.accountType != AccountType.Bussines)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show("Ви не авторизовані");
                return (false, false, false);
            }

            bool economy = false, bussiness = false, first = false;
            string query = @"SELECT Name, Economy, Bussiness, First FROM Plane WHERE AirlineID = @airlineID AND Name LIKE @name";

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);
                if (!string.IsNullOrWhiteSpace(PlaneName))
                {
                    command.Parameters.AddWithValue("@name", PlaneName);
                }

                try
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.GetInt32(1) > 0) economy = true;
                            if (reader.GetInt32(2) > 0) bussiness = true;
                            if (reader.GetInt32(3) > 0) first = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return (economy, bussiness, first);
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

        private void SetBackgroundColor(Classes clas)
        {
            Border border;
            switch (clas)
            {
                case Classes.Econom: border = EconomBorder; break;
                case Classes.Bussiness: border = BussinessBorder; break;
                case Classes.First: border = FirstBorder; break;
                case Classes.Suitcase: border = SuitcaseBorder; break;
                default: return;
            }

            var child = border.Child as Path;
            (border.Background, child.Fill) = (child.Fill, border.Background);
        }

        public void PlaneHintUpdate()
        {
            if (DataBase.GetConnection().State != System.Data.ConnectionState.Open)
                return;

            if (CurrentAccount.id == -1 || CurrentAccount.accountType != AccountType.Bussines) return;

            if (PlaneName == "")
            {
                PlaneHint = "";
                PlaneButton.Visibility = Visibility.Hidden;
                return;
            }

            string query = @"SELECT TOP 1 Name, ID FROM Plane WHERE AirlineID = @airlineID AND Name LIKE @searchText";

            string planeHint = "$";
            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@searchText", $"{PlaneName}%");
                command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);

                try
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string read = reader.GetString(0);
                            PlaneID = reader.GetInt32(1);
                            planeHint = PlaneName + read.Substring(PlaneName.Length);
                        }
                        else PlaneButton.Visibility = Visibility.Hidden;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            if (planeHint != "$") PlaneHint = planeHint;

            if (PlaneHint == PlaneName) PlaneButton.Visibility = Visibility.Hidden;
            CurentClass = Classes.Suitcase;
        }
        #endregion
        #endregion

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

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SeatButtonSize = (sender as Grid).ActualHeight / 7.1 * 1.3;
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public class ClassesPrice : INotifyPropertyChanged
        {
            public string this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0: return PensionerMoney;
                        case 1: return AdultMoney;
                        case 2: return ChildMoney;
                        case 3: return BabyMoney;
                        default: return "-";
                    }
                }
            }

            public string this[Classes index]
            {
                get
                {
                    switch (index)
                    {
                        case Classes.Econom: return PensionerMoney;
                        case Classes.Bussiness: return ChildMoney;
                        case Classes.First: return BabyMoney;
                        default: return "-";
                    }
                }
            }

            private Price adult = new Price(0, 1, 0);
            private Price child = new Price(0, 1, 0);
            private Price baby = new Price(0, 1, 0);
            private Price pensioner = new Price(0, 1, 0);
            public bool isChanged { get; private set; } = false;
            public List<ClassesPrice> team = new List<ClassesPrice>();

            public string AdultMoney
            {
                get => adult.Money.ToString(CultureInfo.InvariantCulture);
                set
                {
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double money))
                    {
                        adult.Money = money;
                        child.FirstValue = money;
                        baby.FirstValue = money;
                        pensioner.FirstValue = money;
                    }
                    else
                    {
                        adult.Money = 0;
                    }

                    OnPropertyChanged(nameof(AdultMoney));
                    OnPropertyChanged(nameof(ChildMoney));
                    OnPropertyChanged(nameof(PensionerMoney));
                    OnPropertyChanged(nameof(BabyMoney));
                }
            }

            public string AdultPercent
            {
                get => adult.Percent.ToString(CultureInfo.InvariantCulture);
            }

            public string ChildMoney
            {
                get => child.Money.ToString(CultureInfo.InvariantCulture);
                set
                {
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double money))
                    {
                        child.Money = money;
                    }
                    else
                    {
                        child.Money = 0;
                    }

                    OnPropertyChanged(nameof(ChildMoney));
                    OnPropertyChanged(nameof(ChildPercent));
                }
            }

            public string ChildPercent
            {
                get => child.Percent.ToString(CultureInfo.InvariantCulture);
                set
                {
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double percent))
                    {
                        child.Percent = percent;
                    }
                    else
                    {
                        child.Percent = 0;
                    }

                    foreach (var item in team)
                    {
                        if (item.isChanged == false) item.child.Percent = child.Percent;
                    }

                    OnPropertyChanged(nameof(ChildPercent));
                    OnPropertyChanged(nameof(ChildMoney));
                }
            }

            public string BabyMoney
            {
                get => baby.Money.ToString(CultureInfo.InvariantCulture);
                set
                {
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double money))
                    {
                        baby.Money = money;
                    }
                    else
                    {
                        baby.Money = 0;
                    }

                    OnPropertyChanged(nameof(BabyMoney));
                    OnPropertyChanged(nameof(BabyPercent));
                }
            }

            public string BabyPercent
            {
                get => baby.Percent.ToString(CultureInfo.InvariantCulture);
                set
                {
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double percent))
                    {
                        baby.Percent = percent;
                    }
                    else
                    {
                        baby.Percent = 0;
                    }

                    foreach (var item in team)
                    {
                        if (item.isChanged == false) item.baby.Percent = baby.Percent;
                    }

                    OnPropertyChanged(nameof(BabyMoney));
                    OnPropertyChanged(nameof(BabyPercent));
                }
            }

            public string PensionerMoney
            {
                get => pensioner.Money.ToString(CultureInfo.InvariantCulture);
                set
                {
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double money))
                    {
                        pensioner.Money = money;
                    }
                    else
                    {
                        pensioner.Money = 0;
                    }

                    OnPropertyChanged(nameof(PensionerMoney));
                    OnPropertyChanged(nameof(PensionerPercent));
                }
            }

            public string PensionerPercent
            {
                get => pensioner.Percent.ToString(CultureInfo.InvariantCulture);
                set
                {
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double percent))
                    {
                        pensioner.Percent = percent;
                    }
                    else
                    {
                        pensioner.Percent = 0;
                    }

                    foreach (var item in team)
                    {
                        if (item.isChanged == false) item.pensioner.Percent = pensioner.Percent;
                    }

                    OnPropertyChanged(nameof(PensionerMoney));
                    OnPropertyChanged(nameof(PensionerPercent));
                }
            }

            private struct Price
            {
                private double _money;
                private double _percent;
                private double _firstValue;

                public Price(double money, double percent, double firstValue)
                {
                    _money = money;
                    _percent = percent;
                    _firstValue = firstValue;
                }

                public double Money
                {
                    get => _money;
                    set
                    {
                        _money = value;
                        if (FirstValue == 0) return;
                        _percent = Math.Round(_money / _firstValue, 2);
                    }
                }

                public double Percent
                {
                    get => _percent;
                    set
                    {
                        _percent = value;
                        _money = Math.Round(_firstValue * Percent, 2);
                    }
                }

                public double FirstValue
                {
                    get => _firstValue;
                    set
                    {
                        if (value == 0)
                        {
                            return;
                        }
                        _firstValue = value;
                        _money = Math.Round(_firstValue * Percent, 2);
                    }
                }
            }

            #region PropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                isChanged = true;
            }
            #endregion
        }
    }
}