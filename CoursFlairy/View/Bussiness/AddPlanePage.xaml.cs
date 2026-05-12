using CoursFlairy.Model.Enum;
using System.ComponentModel;
using static CoursFlairy.ViewModel.ResourceColor;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CoursFlairy.Model;
using System.Text.RegularExpressions;
using System.Windows.Navigation;
using System.Text;
using CoursFlairy.Data;
using Microsoft.Data.SqlClient;
using System.ServiceProcess;
using System.Diagnostics.Metrics;
using System.IO;

namespace CoursFlairy.View.Bussiness
{
    /// <summary>
    /// Interaction logic for AddPlanePage.xaml
    /// </summary>
    public partial class AddPlanePage : Page, INotifyPropertyChanged
    {
        private PlaneBrushes _curentBrush = PlaneBrushes.empty;
        private PlaneBrushes CurentBrush { get => _curentBrush; set { _curentBrush = value; } }

        private List<List<PlaneStruct>> _planeStructsList = new List<List<PlaneStruct>>();
        private List<List<PlaneStruct>> _planeStructsListEnd = new List<List<PlaneStruct>>();
        private string _widthTb = "";
        private string _heightTb = "";
        private string _planeName = "";
        private int economyCount = 0;
        private int businessCount = 0;
        private int firstCount = 0;


        private int currentWidth = -1;
        private int currentHeight = -1;
        private int _constructorColumns = 0;
        private double _constructorWidth = 0;
        private Thickness _TextBoxSeatMargin = new Thickness(0);
        private Geometry _buttonGeometry = (Geometry)Application.Current.Resources["update"];

        private bool _showPlane = true;
        private bool textBoxEnabled = false;
        private bool limitLock = true;
        private bool _seatsHave = false;

        public double SeatSize { get; private set; } = 50;

        #region Властивості
        public List<List<PlaneStruct>> PlaneStructsList { get => _planeStructsList; set { _planeStructsList = value; OnPropertyChanged(nameof(PlaneStructsList)); } }
        public List<List<PlaneStruct>> PlaneStructsListEnd { get => _planeStructsListEnd; set { _planeStructsListEnd = value; OnPropertyChanged(nameof(PlaneStructsListEnd)); } }
        public string WidthTb
        {
            get => _widthTb; set
            {
                if (int.TryParse(value, out int res) && res > 110)
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow.GlobalMessage.Show("Кількість рядів не може бути більше 110");
                    return;
                }

                _widthTb = value;
                OnPropertyChanged(nameof(WidthTb));
                OnPropertyChanged(nameof(ButtonGeometry));
            }
        }

        public string HeightTb

        {
            get => _heightTb; set
            {
                if (int.TryParse(value, out int res) && res > 12)
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow.GlobalMessage.Show("Кількість сидінь у ряді не може бути більше 12");
                    return;
                }

                _heightTb = value;
                OnPropertyChanged(nameof(HeightTb));
                OnPropertyChanged(nameof(ButtonGeometry));
            }
        }

        public Geometry ButtonGeometry
        {
            get
            {
                if (int.TryParse(WidthTb, out int width) && int.TryParse(HeightTb, out int height) && width == currentWidth && height == currentHeight)
                    return (Geometry)Application.Current.Resources["clean"];
                else return (Geometry)Application.Current.Resources["update"];
            }
        }
        public bool SeatsHave { get => _seatsHave; set { _seatsHave = value; OnPropertyChanged(nameof(SeatsHave)); } }

        public static readonly DependencyProperty ToogleButtonRow1HeightProperty = DependencyProperty.Register(nameof(ToogleButtonRow1Height), typeof(GridLength), typeof(AddPlanePage), new PropertyMetadata(new GridLength(0, GridUnitType.Star)));
        public static readonly DependencyProperty ToogleButtonRow2HeightProperty = DependencyProperty.Register(nameof(ToogleButtonRow2Height), typeof(GridLength), typeof(AddPlanePage), new PropertyMetadata(new GridLength(1, GridUnitType.Star)));

        public string PlaneNameTb { get => _planeName; set { _planeName = value; OnPropertyChanged(nameof(PlaneNameTb)); } }
        public Thickness TextBoxSeatMargin { get => _TextBoxSeatMargin; set { _TextBoxSeatMargin = value; OnPropertyChanged(nameof(TextBoxSeatMargin)); } }
        public GridLength ToogleButtonRow1Height { get => (GridLength)GetValue(ToogleButtonRow1HeightProperty); set { SetValue(ToogleButtonRow1HeightProperty, value); } }
        public GridLength ToogleButtonRow2Height { get => (GridLength)GetValue(ToogleButtonRow2HeightProperty); set => SetValue(ToogleButtonRow2HeightProperty, value); }
        public bool ShowPlane { get => _showPlane; set { _showPlane = value; OnPropertyChanged(nameof(ShowPlane)); } }
        public int ConstructorColumns { get => _constructorColumns; set { _constructorColumns = value; OnPropertyChanged(nameof(ConstructorColumns)); } }
        public double ConstructorWidth { get => _constructorWidth; set { _constructorWidth = value; OnPropertyChanged(nameof(ConstructorWidth)); OnPropertyChanged(nameof(ConstructorWidthItem)); OnPropertyChanged(nameof(ForLine1)); OnPropertyChanged(nameof(ForLine2)); } }
        public double ConstructorWidthItem { get => _constructorWidth + 2; }
        public double ForLine1 { get => SeatSize - 2; }
        public double ForLine2 { get => ConstructorWidth - SeatSize + 2; }
        #endregion

        public AddPlanePage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private string GetPlaneScheme()
        {
            StringBuilder result = new StringBuilder();
            economyCount = businessCount = firstCount = 0;
            for (int i = 0; i < PlaneStructsList.Count; i++)
            {
                string letter = "";
                for (int j = 0; j < PlaneStructsList[i].Count; j++)
                {
                    if (j == 0)
                    {
                        letter = PlaneStructsList[i][j].TextBoxText;
                        continue;
                    }

                    string seat = PlaneStructsList[0][j].TextBoxSeatText;
                    int brush = (int)PlaneStructsList[i][j].Brush;

                    switch (PlaneStructsList[i][j].Brush)
                    {
                        case PlaneBrushes.econom: economyCount += 1; break;
                        case PlaneBrushes.bussiness: businessCount += 1; break;
                        case PlaneBrushes.first: firstCount += 1; break;
                    }

                    result.Append($"{letter}-{seat}-{brush}");
                    if (j != PlaneStructsList[i].Count - 1) result.Append(",");
                }
                if (i != PlaneStructsList.Count - 1) result.Append("|");
            }
            return result.ToString();
        }

        #region Взаємодії
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void AddPlane_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool enterHave = false;

            for (int i = 0; i < PlaneStructsList[0].Count; i++)
            {
                if (PlaneStructsList[0][i].Brush == PlaneBrushes.enter)
                {
                    enterHave = true;
                    break;
                }
            }

            if (!enterHave)
                for (int i = 0; i < PlaneStructsList[PlaneStructsList.Count - 1].Count; i++)
                {
                    if (PlaneStructsList[PlaneStructsList.Count - 1][i].Brush == PlaneBrushes.enter)
                    {
                        enterHave = true;
                        break;
                    }
                }

            if (!enterHave)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show("Вкажіть місця входу у літак");
                return;
            }

            if (PlaneNameTb == "")
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show("Ім'я літака не може бути порожнім");
                return;
            }

            try
            {
                if (CheckPlaneNameAvailability())
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow.GlobalMessage.Show("У вашій колекції вже є літак з таким іменем");
                    return;
                }
            }
            catch (Exception ex) { var mainWindow = Application.Current.MainWindow as MainWindow; mainWindow.GlobalMessage.Show(ex.Message); return; }

            EndAddPlaneGrid.Visibility = Visibility.Visible;
            List<List<PlaneStruct>> temp = new List<List<PlaneStruct>>();
            for (int i = 0; i < PlaneStructsList.Count; i++)
            {
                temp.Add(new List<PlaneStruct>());
                for (int j = 0; j < PlaneStructsList[i].Count; j++)
                {
                    if (j == 0) continue;
                    temp[i].Add(PlaneStructsList[i][j]);
                }
            }
            PlaneStructsListEnd = temp;
        }

        private void AddPlaneEnd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string shem = GetPlaneScheme();
            string query = @"INSERT INTO [dbo].[Plane] ([AirlineID], [Name], [SeatScheme], [Economy], [Bussiness], [First]) VALUES (@airlineID, @name, @seatschem, @economy, @bussiness, @first)";
            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);
                command.Parameters.AddWithValue("@name", PlaneNameTb);
                command.Parameters.AddWithValue("@seatschem", shem);
                command.Parameters.AddWithValue("@economy", economyCount);
                command.Parameters.AddWithValue("@bussiness", businessCount);
                command.Parameters.AddWithValue("@first", firstCount);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            this.NavigationService.Navigate(new PlanePage());
        }

        private void BackAddPlaneEnd_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            EndAddPlaneGrid.Visibility = Visibility.Collapsed;
        }

        private void BackAddPlane_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private async void UpdatePlane_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (int.TryParse(WidthTb, out _) && int.TryParse(HeightTb, out _))
            {
                ConstructorUpdate();
            }

            await Task.Delay(2);

            UpdateScroll();
        }

        private bool _isMouseDown = false;
        private object _lastHoveredSeat = null;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            _lastHoveredSeat = sender;
            ChangeSeat(sender, e);
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown && sender != _lastHoveredSeat)
            {
                _lastHoveredSeat = sender;
                ChangeSeat(sender, new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
            _lastHoveredSeat = null;
        }

        private void ChangeSeat(object sender, MouseButtonEventArgs e)
        {
            Border border = ((Grid)sender).Children[0] as Border;
            if (border != null)
            {
                PlaneStruct seat = border.DataContext as PlaneStruct;
                if (seat != null)
                {
                    seat.SetBrush(CurentBrush);
                    UpdateWC();
                }
            }

            UpdateTextBoxes();
        }

        private async void ToogleButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ToggleButtonAnimation();

            await Task.Delay(2);

            UpdateScroll();
        }

        private void TextBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid grid)
            {
                if (grid.Children[0] is Viewbox viewbox)
                {
                    if (viewbox.Child is TextBox textBox)
                    {
                        var datecontext = textBox.DataContext as PlaneStruct;
                        int index = datecontext.textBoxColumn;

                        for (int i = 0; i < PlaneStructsList[index].Count; i++)
                        {
                            PlaneStructsList[index][i].SetBrush(CurentBrush);
                        }

                        UpdateWC();
                        UpdateTextBoxes();
                    }
                }
            }
        }

        private void TextBoxSeat_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (sender is Grid grid)
            {
                if (grid.Children[0] is Viewbox viewbox)
                {
                    if (viewbox.Child is TextBox textBox)
                    {
                        var datecontext = textBox.DataContext as PlaneStruct;
                        int index = datecontext.textBoxRow;

                        for (int i = 0; i < PlaneStructsList.Count - 1; i++)
                        {
                            PlaneStructsList[i][index].SetBrush(CurentBrush);
                        }

                        UpdateWC();
                        UpdateTextBoxes();
                    }
                }
            }
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).ContextMenu = null;
        }

        private void SizeInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out int result)) e.Handled = true;
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (Width.IsFocused || PlaneName.IsFocused || Height.IsFocused || textBoxEnabled) return;

            if (e.Key == Key.D1 && CurentBrush != PlaneBrushes.empty) ChangeBrush(EmptyBorder, null);
            if (e.Key == Key.D2 && CurentBrush != PlaneBrushes.enter) ChangeBrush(EnterBorder, null);
            if (e.Key == Key.D3 && CurentBrush != PlaneBrushes.wc) ChangeBrush(WCBorder, null);
            if (e.Key == Key.D4 && CurentBrush != PlaneBrushes.econom) ChangeBrush(EconomBorder, null);
            if (e.Key == Key.D5 && CurentBrush != PlaneBrushes.bussiness) ChangeBrush(BussinessBorder, null);
            if (e.Key == Key.D6 && CurentBrush != PlaneBrushes.first) ChangeBrush(FirstBorder, null);
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            textBoxEnabled = true;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            textBoxEnabled = false;
        }

        Regex bigLettersPattern = new Regex("[A-Z]");
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!bigLettersPattern.IsMatch(e.Text)) e.Handled = true;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                if (textBlock.Text == "🔒")
                {
                    textBlock.Text = "🔓";
                    textBlock.Opacity = 0.5;
                    limitLock = false;
                    UpdateTextBoxes();
                }
                else
                {
                    textBlock.Text = "🔒";
                    textBlock.Opacity = 1;
                    limitLock = true;
                }
            }
        }

        private void ToHeight_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Height.Focus();
        }

        private void ToWidth_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Width.Focus();
        }
        #endregion

        #region Анімації
        private Storyboard _currentStoryboard;
        public void ToggleButtonAnimation()
        {
            _currentStoryboard?.Stop();

            var fromValue1 = ToogleButtonRow1Height.Value;
            var toValue1 = fromValue1 > 0.5 ? 0 : 1;
            var fromValue2 = ToogleButtonRow2Height.Value;
            var toValue2 = fromValue2 > 0.5 ? 0 : 1;

            _currentStoryboard = new Storyboard();

            var animation1 = new GridLengthAnimation
            {
                From = new GridLength(fromValue1, GridUnitType.Star),
                To = new GridLength(toValue1, GridUnitType.Star),
                Duration = TimeSpan.FromSeconds(0.2)
            };
            Storyboard.SetTarget(animation1, this);
            Storyboard.SetTargetProperty(animation1, new PropertyPath(nameof(ToogleButtonRow1Height)));
            _currentStoryboard.Children.Add(animation1);

            var animation2 = new GridLengthAnimation
            {
                From = new GridLength(fromValue2, GridUnitType.Star),
                To = new GridLength(toValue2, GridUnitType.Star),
                Duration = TimeSpan.FromSeconds(0.2)
            };
            Storyboard.SetTarget(animation2, this);
            Storyboard.SetTargetProperty(animation2, new PropertyPath(nameof(ToogleButtonRow2Height)));
            _currentStoryboard.Children.Add(animation2);

            _currentStoryboard.Completed += (sender, e) => { ShowPlane = toValue1 == 0; UpdateScroll(); };
            _currentStoryboard.Begin();
        }
        #endregion

        #region Кисті
        private void ChangeBrush(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                SelectPlane(GetBorderFromBrush(CurentBrush));
                CurentBrush = GetBrushFromName(border.Name);
                SelectPlane(GetBorderFromBrush(CurentBrush));
            }

            if (e != null && e.ChangedButton == MouseButton.Right && PlaneStructsList != null)
            {
                for (int i = 0; i < PlaneStructsList.Count; i++)
                {
                    for (int j = 0; j < PlaneStructsList[i].Count; j++)
                    {
                        if (PlaneStructsList[i][j].Brush == CurentBrush)
                            PlaneStructsList[i][j].SetBrush(CurentBrush);
                    }
                }
            }
            UpdateTextBoxes();
        }

        private void SelectPlane(Border border)
        {
            if (border.Child is System.Windows.Shapes.Path el)
                (el.Stroke, border.Background) = (border.Background, el.Stroke);
            else if (border.Child is Viewbox vb)
            {
                if (vb.Child is TextBlock tb)
                    (tb.Foreground, border.Background) = (border.Background, tb.Foreground);
            }
        }

        private PlaneBrushes GetBrushFromName(string name)
        {
            return name switch
            {
                "EmptyBorder" => PlaneBrushes.empty,
                "EnterBorder" => PlaneBrushes.enter,
                "WCBorder" => PlaneBrushes.wc,
                "EconomBorder" => PlaneBrushes.econom,
                "BussinessBorder" => PlaneBrushes.bussiness,
                _ => PlaneBrushes.first,
            };
        }

        private Border GetBorderFromBrush(PlaneBrushes brush)
        {
            return brush switch
            {
                PlaneBrushes.empty => EmptyBorder,
                PlaneBrushes.enter => EnterBorder,
                PlaneBrushes.wc => WCBorder,
                PlaneBrushes.econom => EconomBorder,
                PlaneBrushes.bussiness => BussinessBorder,
                _ => FirstBorder,
            };
        }
        #endregion

        #region Підказка підсвітка
        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                if (grid.Children[0] is Viewbox viewbox)
                {
                    if (viewbox.Child is TextBox textBox)
                    {
                        var datecontext = textBox.DataContext as PlaneStruct;
                        int index = datecontext.textBoxColumn;
                        if (index == -1) return;

                        for (int i = 1; i < PlaneStructsList[index].Count; i++)
                        {
                            PlaneStructsList[index][i].HintOpacity = 0.05;
                        }
                    }
                }
            }
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                if (grid.Children[0] is Viewbox viewbox)
                {
                    if (viewbox.Child is TextBox textBox)
                    {
                        var datecontext = textBox.DataContext as PlaneStruct;
                        int index = datecontext.textBoxColumn;
                        if (index == -1) return;

                        for (int i = 1; i < PlaneStructsList[index].Count; i++)
                        {
                            PlaneStructsList[index][i].HintOpacity = 0;
                        }
                    }
                }
            }
        }

        private void TextBoxSeat_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                if (grid.Children[0] is Viewbox viewbox)
                {
                    if (viewbox.Child is TextBox textBox)
                    {
                        var datecontext = textBox.DataContext as PlaneStruct;
                        int index = datecontext.textBoxRow;
                        if (index == -1) return;

                        for (int i = 1; i < PlaneStructsList.Count - 1; i++)
                        {
                            PlaneStructsList[i][index].HintOpacity = 0.05;
                        }
                    }
                }
            }
        }

        private void TextBoxSeat_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                if (grid.Children[0] is Viewbox viewbox)
                {
                    if (viewbox.Child is TextBox textBox)
                    {
                        var datecontext = textBox.DataContext as PlaneStruct;
                        int index = datecontext.textBoxRow;
                        if (index == -1) return;


                        for (int i = 1; i < PlaneStructsList.Count - 1; i++)
                        {
                            PlaneStructsList[i][index].HintOpacity = 0;
                        }
                    }
                }
            }
        }

        private PlaneBrushes becupBrush = PlaneBrushes.empty;
        private void ConstructorHint_MouseEnter(object sender, MouseEventArgs e)
        {
            becupBrush = CurentBrush;

            var datecontext = ((sender as Grid).Children[0] as Border).DataContext as PlaneStruct;
            if (datecontext.textBoxEnabled) return;

            var isLimit = (datecontext).borderLimit;
            if (isLimit == true)
            {
                if (becupBrush == PlaneBrushes.enter || becupBrush == PlaneBrushes.empty)
                    ((Grid)sender).Children[1].Opacity = 0.05;
            }
            else
            {
                if (becupBrush == PlaneBrushes.enter) return;
                ((Grid)sender).Children[1].Opacity = 0.05;
            }
        }

        private void ConstructorHint_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                var grid = sender as Grid;
                if (grid == null) return;

                if (grid.Children.Count < 1) return;

                var border = grid.Children[0] as Border;
                if (border == null) return;

                var planeStruct = border.DataContext as PlaneStruct;
                if (planeStruct == null) return;
                var isLimit = planeStruct.borderLimit;

                if (isLimit == true)
                {
                    if (becupBrush == PlaneBrushes.enter || becupBrush == PlaneBrushes.empty)
                        grid.Children[1].Opacity = 0;
                }
                else
                {
                    if (becupBrush == PlaneBrushes.enter) return;
                    grid.Children[1].Opacity = 0;
                }

                becupBrush = CurentBrush;
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region Update методи

        private readonly Dictionary<int, string> letters = new Dictionary<int, string> { { 0, "" }, { 1, "A" }, { 2, "B" }, { 3, "C" }, { 4, "D" }, { 5, "E" }, { 6, "F" }, { 7, "G" }, { 8, "H" }, { 9, "I" }, { 10, "J" }, { 11, "K" }, { 12, "L" }, { 13, "M" } };
        private void ConstructorUpdate()
        {
            if (string.IsNullOrWhiteSpace(WidthTb) || string.IsNullOrWhiteSpace(HeightTb)) return;
            UpdateSeatSize();

            int.TryParse(WidthTb, out int width);
            int.TryParse(HeightTb, out int height);

            bool similar = height == currentHeight && width == currentWidth;
            currentHeight = height;
            currentWidth = width;

            width++; //для текстбокса
            ConstructorColumns = width;
            ConstructorWidth = width * (SeatSize + 7);

            height += 2;

            List<List<PlaneStruct>> result = new List<List<PlaneStruct>>();
            for (int i = 0; i < height; i++)
            {
                List<PlaneStruct> tempList = new List<PlaneStruct>();
                for (int j = 0; j < width; j++)
                {
                    var brush = PlaneBrushes.empty;
                    string letter = letters[i];
                    string seatNum = j.ToString();

                    if (!similar)
                    {
                        if (i < PlaneStructsList.Count)
                        {
                            if (j < PlaneStructsList[i].Count)
                            {
                                brush = PlaneStructsList[i][j].Brush;
                            }
                        }
                    }

                    if ((i == 0 || i == height - 1)) tempList.Add(new PlaneStruct(brush, this, true));
                    else tempList.Add(new PlaneStruct(brush, this));

                    if (i == 0 && j == 0)
                    {
                        if (limitLock) { tempList[j].TextBlockStateText = "🔒"; }
                        else tempList[j].TextBlockStateText = "🔓";
                        tempList[j].TextBlockStateVisiblity = Visibility.Visible;
                    }

                    if (i == 0 && j != 0)
                    {
                        if (limitLock)
                        {
                            if (i < PlaneStructsList.Count)
                            {
                                if (j < PlaneStructsList[i].Count)
                                {
                                    seatNum = PlaneStructsList[i][j].TextBoxSeatText;
                                }
                            }
                        }

                        tempList[j].TextBoxSeatText = seatNum;
                        tempList[j].textBoxRow = j;
                    }

                    if (j == 0)
                    {
                        if (i != 0 && i != height - 1)
                        {
                            if (limitLock)
                            {
                                if (i < PlaneStructsList.Count)
                                {
                                    if (j < PlaneStructsList[i].Count)
                                    {
                                        letter = PlaneStructsList[i][j].TextBoxText;
                                    }
                                }
                            }

                            tempList[j].textBoxColumn = i;
                            tempList[j].TextBoxText = letter;
                        }
                        tempList[j].textBoxEnabled = true;
                    }

                    if (i == 0) tempList[j].Angles = 270;
                    if (i == height - 1) tempList[j].Angles = 90;

                }
                result.Add(tempList);
            }
            PlaneStructsList = result;

            TextBoxSeatMargin = new Thickness(0, -SeatSize / 2, 0, SeatSize / 2);
            OnPropertyChanged(nameof(ButtonGeometry));
            UpdateTextBoxes();
            UpdateWC();
        }

        private void UpdateSeatSize()
        {
            int.TryParse(WidthTb, out int width);
            int.TryParse(HeightTb, out int height);

            var widthA = this.ActualWidth;
            SeatSize = widthA / Math.Min((15 + (Math.Max(width, height) / 1.3)), 40);
        }

        private async void UpdateScroll()
        {
            var gridHeight = ConstructorGrid.ActualHeight;
            var constructorHeight = Constructor.ActualHeight;

            if (constructorHeight < gridHeight)
                ConstructorScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            else
            {
                ConstructorScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                await Task.Delay(2);

                double offsetX = (ConstructorScroll.ExtentWidth - ConstructorScroll.ViewportWidth) / 2;
                double offsetY = (ConstructorScroll.ExtentHeight - ConstructorScroll.ViewportHeight) / 2;

                if (offsetX > 0) ConstructorScroll.ScrollToHorizontalOffset(offsetX);
                if (offsetY > 0) ConstructorScroll.ScrollToVerticalOffset(offsetY);
            }
        }

        public void UpdateWC()
        {
            Dictionary<int, List<int>> wcWidth = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> wcHeight = new Dictionary<int, List<int>>();

            for (int i = 0; i < PlaneStructsList.Count; i++)
            {
                for (int j = 0; j < PlaneStructsList[i].Count; j++)
                {
                    if (PlaneStructsList[i][j].Brush == PlaneBrushes.wc)
                    {
                        if (!wcWidth.ContainsKey(i)) wcWidth[i] = new List<int>();
                        if (!wcHeight.ContainsKey(j)) wcHeight[j] = new List<int>();

                        wcWidth[i].Add(j);
                        wcHeight[j].Add(i);

                        PlaneStructsList[i][j].Margin = new Thickness(3);
                        double marg = -4;

                        for (int allJ = 1; allJ < j; allJ++)
                        {
                            int index = j - allJ;
                            if (wcWidth[i].Contains(index))
                            {
                                PlaneStructsList[i][j].Margin = new Thickness(marg, Math.Min(PlaneStructsList[i][index].Margin.Top, PlaneStructsList[i][j].Margin.Top), 3, Math.Min(PlaneStructsList[i][index].Margin.Bottom, PlaneStructsList[i][j].Margin.Bottom));
                                PlaneStructsList[i][index].Margin = new Thickness(PlaneStructsList[i][index].Margin.Left, Math.Min(PlaneStructsList[i][index].Margin.Top, PlaneStructsList[i][j].Margin.Top), marg, Math.Min(PlaneStructsList[i][index].Margin.Bottom, PlaneStructsList[i][j].Margin.Bottom));
                            }
                            else break;
                        }

                        for (int allI = 1; allI < i; allI++)
                        {
                            int index = i - allI;
                            if (wcHeight[j].Contains(index))
                            {
                                PlaneStructsList[i][j].Margin = new Thickness(Math.Min(PlaneStructsList[index][j].Margin.Left, PlaneStructsList[i][j].Margin.Left), marg, Math.Min(PlaneStructsList[index][j].Margin.Right, PlaneStructsList[i][j].Margin.Right), PlaneStructsList[i][j].Margin.Bottom);
                                PlaneStructsList[index][j].Margin = new Thickness(Math.Min(PlaneStructsList[index][j].Margin.Left, PlaneStructsList[i][j].Margin.Left), PlaneStructsList[index][j].Margin.Top, Math.Min(PlaneStructsList[index][j].Margin.Right, PlaneStructsList[i][j].Margin.Right), marg);
                            }
                            else break;
                        }
                    }
                    else
                    {
                        PlaneStructsList[i][j].Margin = new Thickness(3);
                    }
                }
            }
        }

        private void UpdateTextBoxes()
        {
            int countLetters = 0;
            for (int i = 0; i < PlaneStructsList.Count; i++)
            {
                bool isVisible = false;
                for (int j = 1; j < PlaneStructsList[i].Count; j++)
                {
                    if (PlaneStructsList[i][j].Brush != PlaneBrushes.empty && PlaneStructsList[i][j].Brush != PlaneBrushes.wc && PlaneStructsList[i][j].Brush != PlaneBrushes.enter)
                    {
                        isVisible = true;
                        break;
                    }
                }

                if (isVisible)
                {
                    PlaneStructsList[i][0].TextBoxVisibility = Visibility.Visible;
                    countLetters++;
                    if (!limitLock)
                    {
                        PlaneStructsList[i][0].TextBoxText = letters[countLetters];
                    }
                }
                else PlaneStructsList[i][0].TextBoxVisibility = Visibility.Collapsed;
            }

            int countSeats = 0;
            if (PlaneStructsList.Count > 0)
                for (int j = 1; j < PlaneStructsList[0].Count; j++)
                {
                    bool isVisibleSeat = false;

                    for (int i = 0; i < PlaneStructsList.Count; i++)
                    {
                        if (PlaneStructsList[i][j].Brush != PlaneBrushes.empty && PlaneStructsList[i][j].Brush != PlaneBrushes.wc && PlaneStructsList[i][j].Brush != PlaneBrushes.enter)
                        {
                            isVisibleSeat = true;
                            break;
                        }
                    }

                    if (isVisibleSeat)
                    {
                        PlaneStructsList[0][j].TextBoxSeatVisibility = Visibility.Visible;
                        countSeats++;
                        if (!limitLock)
                        {
                            PlaneStructsList[0][j].TextBoxSeatText = countSeats.ToString();
                        }
                    }
                    else PlaneStructsList[0][j].TextBoxSeatVisibility = Visibility.Collapsed;
                }

            if (countLetters == 0 && countLetters == 0) SeatsHave = false;
            else SeatsHave = true;
        }
        #endregion

        private bool CheckPlaneNameAvailability()
        {
            if (CurrentAccount.id == -1) throw new Exception("Аккаунт не обраний");

            string query = "SELECT dbo.check_planename_availability(@airlineID, @name)";

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@airlineID", CurrentAccount.id);
                command.Parameters.AddWithValue("@name", PlaneNameTb);
                return (bool)command.ExecuteScalar();
            }
        }

        public class PlaneStruct : INotifyPropertyChanged
        {
            private PlaneBrushes _brush = PlaneBrushes.empty;
            private double _angle = 0;
            private double _width = 50;
            private double _height = 50;
            private double _hintOpacity = 0;
            private Thickness _margin = new Thickness(3);
            private CornerRadius _radius = new CornerRadius(6);
            private SolidColorBrush _background = EmptyColor;
            private SolidColorBrush _borderBrush = EmptyColor;
            private SolidColorBrush _svgColor = new SolidColorBrush(Colors.Transparent);
            private Visibility _textBoxVisibility = Visibility.Collapsed;
            private Visibility _textBoxSeatVisibility = Visibility.Collapsed;
            private Visibility _textBlockStateVisiblity = Visibility.Collapsed;
            private string _textBoxText = "";
            private string _textBoxSeatText = "";
            private string _textBlockStateText = "";

            public bool textBoxEnabled { get; set; } = false;
            public int textBoxColumn { get; set; } = -1;
            public int textBoxRow { get; set; } = -1;
            public bool borderLimit { get; private set; } = false;
            public PlaneBrushes Brush
            {
                get => _brush; set
                {
                    _brush = value;

                    if (value == PlaneBrushes.wc) { Radius = new CornerRadius(0); }
                    else { Radius = new CornerRadius(6); }

                    if (value == PlaneBrushes.enter) { SvgColor = EnterColor; }
                    else { SvgColor = new SolidColorBrush(Colors.Transparent); }

                    OnPropertyChanged(nameof(Brush));
                }
            }

            public RotateTransform PathAngleTransform { get => new RotateTransform { Angle = Angles }; }
            public string TextBlockStateText { get => _textBlockStateText; set { _textBlockStateText = value; OnPropertyChanged(nameof(TextBlockStateText)); } }
            public string TextBoxSeatText { get => _textBoxSeatText; set { _textBoxSeatText = value; OnPropertyChanged(nameof(TextBoxSeatText)); } }
            public string TextBoxText { get => _textBoxText; set { _textBoxText = value; OnPropertyChanged(nameof(TextBoxText)); } }
            public double HintOpacity { get => _hintOpacity; set { _hintOpacity = value; OnPropertyChanged(nameof(HintOpacity)); } }
            public Visibility TextBoxVisibility { get => _textBoxVisibility; set { _textBoxVisibility = value; OnPropertyChanged(nameof(TextBoxVisibility)); } }
            public Visibility TextBoxSeatVisibility { get => _textBoxSeatVisibility; set { _textBoxSeatVisibility = value; OnPropertyChanged(nameof(TextBoxSeatVisibility)); } }
            public Visibility TextBlockStateVisiblity { get => _textBlockStateVisiblity; set { _textBlockStateVisiblity = value; OnPropertyChanged(nameof(TextBlockStateVisiblity)); } }
            public double Angles { get => _angle; set { _angle = value; OnPropertyChanged(nameof(Angles)); OnPropertyChanged(nameof(PathAngleTransform)); } }
            public double Width { get => _width; set { _width = value; OnPropertyChanged(nameof(Width)); } }
            public double Height { get => _height; set { _height = value; OnPropertyChanged(nameof(Height)); } }
            public Thickness Margin { get => _margin; set { _margin = value; OnPropertyChanged(nameof(Margin)); } }
            public SolidColorBrush Background { get => _background; set { _background = value; OnPropertyChanged(nameof(Background)); } }
            public SolidColorBrush BorderBrush { get => _borderBrush; set { _borderBrush = value; OnPropertyChanged(nameof(BorderBrush)); } }
            public SolidColorBrush SvgColor { get => _svgColor; set { _svgColor = value; OnPropertyChanged(nameof(SvgColor)); } }
            public CornerRadius Radius { get => _radius; set { _radius = value; OnPropertyChanged(nameof(Radius)); } }


            public PlaneStruct(PlaneBrushes brush, Page page = null, bool borderLimit = false)
            {
                this.borderLimit = borderLimit;

                Height = (page as AddPlanePage).SeatSize;
                Width = Height;

                if (borderLimit) Height /= 2;
                SetBrush(brush);
            }

            public void SetBrush(PlaneBrushes brush)
            {
                if (textBoxEnabled) return;

                if (borderLimit && !(brush == PlaneBrushes.enter || brush == PlaneBrushes.empty)) return;
                if (brush == Brush) brush = PlaneBrushes.empty;

                switch (brush)
                {
                    case PlaneBrushes.enter: if (!borderLimit) return; Brush = PlaneBrushes.enter; Background = new SolidColorBrush(Colors.Transparent); BorderBrush = Background; break;
                    case PlaneBrushes.wc: Brush = PlaneBrushes.wc; Background = WCColor; BorderBrush = WCColor; break;
                    case PlaneBrushes.econom: Brush = PlaneBrushes.econom; Background = TransparentColor(EconomColor.Color); BorderBrush = EconomColor; break;
                    case PlaneBrushes.bussiness: Brush = PlaneBrushes.bussiness; Background = TransparentColor(BusinessColor.Color); BorderBrush = BusinessColor; break;
                    case PlaneBrushes.first: Brush = PlaneBrushes.first; Background = TransparentColor(FirstColor.Color); BorderBrush = FirstColor; break;
                    default: Brush = PlaneBrushes.empty; Background = EmptyColor; BorderBrush = EmptyColor; break;
                }
            }

            private SolidColorBrush TransparentColor(Color color)
            {
                return new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B));
            }

            public override string ToString()
            {
                return "мем";
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
