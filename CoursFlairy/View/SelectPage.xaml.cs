using CoursFlairy.Model;
using CoursFlairy.Model.Enum;
using CoursFlairy.Model.UI;
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

        public SelectPage()
        {
            InitializeComponent();
        }

        public SelectPage(FlightStruct filterStruct)
        {
            InitializeComponent();
            filters = filterStruct;
            MessageBox.Show($"із: {filters.DepartureAirport} \nдо: {filters.ArrivalAirport} \nколи: {filters.DateFlight[0].ToString()}\nкількість: {filters.PersonClasses.Count()} \nхто: {((Classes)filters.PersonClasses[0]).ToString()}");

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
                ResetAllPaths();

                int clickedIndex = int.Parse(clickedArea.Name.Substring(9));

                for (int i = 1; i <= clickedIndex; i++)
                {
                    Path path = FindName($"Path{i}") as Path;
                    TextBlock text = FindName($"Text{i}") as TextBlock;

                    if (path != null)
                    {
                        path.Fill = (SolidColorBrush)FindResource("MainColor100");
                        path.StrokeThickness = 1.7;
                    }

                    if (text != null)
                    {
                        text.Foreground = (SolidColorBrush)FindResource("White");
                    }
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
                }

                if (text != null)
                {
                    text.Foreground = (SolidColorBrush)FindResource("MainColor100");
                }
            }
        }
    }
}