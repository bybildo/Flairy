using CoursFlairy.Model;
using CoursFlairy.Model.Enum;
using CoursFlairy.Model.UI;
using CoursFlairy.View.Bussiness;
using CoursFlairy.View.ClientPage;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static CoursFlairy.ViewModel.ResourceColor;

namespace CoursFlairy.View
{
    public partial class SelectPage : Page, INotifyPropertyChanged
    {
        private FlightStruct filters;
      
        public int CurentPage { get; set; } = 2;
        public List<string> SelectedSeatsCode { get; set; } = new List<string>();
        public List<string> SelectedSeats { get; set; } = new List<string>();
        public List<PassportInfo> Clients { get; set; } = new List<PassportInfo>();
        public List<decimal> Prices { get; set; } = new List<decimal>();
        public List<int> TicketId { get; set; } = new List<int>();
        public List<bool> HasBaggage { get; set; } = new List<bool>();
        public string Email { get; set; }

        private int _flightId;
        private Classes _currentClass;
        private List<Classes> _passengerClasses;

        public int flightId
        {
            get { return _flightId; }
            set { _flightId = value; }
        }

        public Classes currentClass
        {
            get { return _currentClass; }
            set { _currentClass = value; }
        }

        public List<Classes> passengerClasses
        {
            get { return _passengerClasses; }
            set { _passengerClasses = value; }
        }


        public SelectPage()
        {
            InitializeComponent();
        }

        public SelectPage(FlightStruct filterStruct)
        {
            InitializeComponent();
            filters = filterStruct;
            PageManager.Navigate(new FlightClientPage(filters));
            FillPath(2);
            //MessageBox.Show($"із: {filters.DepartureAirport} \nдо: {filters.ArrivalAirport} \nколи: {filters.DateFlight[0].ToString()}\nкількість: {filters.PersonClasses.Count()} \nхто: {((Classes)filters.PersonClasses[0]).ToString()}");
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void ClickArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid clickedArea)
            {
                int clickedIndex = int.Parse(clickedArea.Name.Substring(9));
                if (clickedIndex > CurentPage)
                {
                    var mainWindow = (MainWindow)Application.Current.MainWindow;
                    mainWindow.GlobalMessage.Show("Ви не можете перейти до наступного етапу не пройшовши цей", 1);
                    return;
                }

                if (CurentPage == 6) return;

                ResetAllPaths();
                FillPath(clickedIndex);
                switch (clickedIndex)
                {
                    case 1:
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        if (mainWindow != null)
                        {
                            mainWindow.PageManager.Navigate(new SearchPage());
                        }
                        break;
                    case 2:
                        CurentPage = 2;
                        PageManager.Navigate(new FlightClientPage(filters));
                        break;
                    case 3:
                        CurentPage = 3;
                        PageManager.Navigate(new ChoosingSeatsPage(flightId, currentClass));
                        break;
                    case 4:
                        CurentPage = 4;
                        PageManager.Navigate(new PassengerDataPage(flightId, passengerClasses));
                        break;
                }
            }
        }

        public void FillPath(int clickedIndex)
        {
            for (int i = 1; i <= clickedIndex; i++)
            {
                Path path = FindName($"Path{i}") as Path;
                TextBlock text = FindName($"Text{i}") as TextBlock;

                if (path != null)
                {
                    path.Fill = (SolidColorBrush)FindResource("MainColor100");
                    path.StrokeThickness = 1.7;
                    path.Stroke = MainColor100;
                }

                if (text != null)
                {
                    text.Foreground = (SolidColorBrush)FindResource("White");
                }
            }
        }

        private void ResetAllPaths()
        {
            for (int i = 1; i <= 6; i++)
            {
                Path path = FindName($"Path{i}") as Path;
                TextBlock text = FindName($"Text{i}") as TextBlock;

                if (path != null)
                {
                    path.Fill = (SolidColorBrush)FindResource("White");
                    path.StrokeThickness = 1.7;
                    path.Stroke = MainColor10;
                }

                if (text != null)
                {
                    text.Foreground = (SolidColorBrush)FindResource("MainColor100");
                }
            }
        }
    }
}