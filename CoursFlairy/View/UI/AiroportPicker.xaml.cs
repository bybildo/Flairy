using CoursFlairy.Data;
using CoursFlairy.Model.UI;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CoursFlairy.View.UI
{
    public partial class AiroportPicker : UserControl, INotifyPropertyChanged
    {
        private string _searchDepartureField = "";
        private string _searchArrivalField = "";
        private List<AirportStruct> _airports = new List<AirportStruct>();
        private bool _isDeparture = true;

        private AirportStruct _departureAirport;
        private AirportStruct _arrivalAirport;

        public event EventHandler DepartureSelect;
        public event EventHandler ArrivalSelect;
        public event EventHandler DepartureUnselect;
        public event EventHandler ArrivalUnselect;

        #region Властивості
        public string SearchField
        {
            get
            {
                if (_isDeparture) return _searchDepartureField;
                else return _searchArrivalField;
            }
            set
            {
                if (_isDeparture)
                {
                    if (value == SearchDepartureField) return;
                    _searchDepartureField = value;
                }
                else
                {
                    if (value == SearchArrivalField) return;
                    _searchArrivalField = value;
                }
                AirportsUpdate(value);
                OnPropertyChanged(nameof(SearchField));
                OnPropertyChanged(nameof(SearchDepartureField));
                OnPropertyChanged(nameof(SearchArrivalField));
            }
        }

        public string SearchDepartureField
        {
            get { return _searchDepartureField; }
            set
            {
                SearchField = value;
                OnPropertyChanged(nameof(SearchDepartureField));
            }
        }

        public string SearchArrivalField
        {
            get { return _searchArrivalField; }
            set
            {
                SearchField = value;
                OnPropertyChanged(nameof(SearchArrivalField));
            }
        }

        public List<AirportStruct> Airports
        {
            get { return _airports; }
            set
            {
                _airports = value;
                OnPropertyChanged(nameof(Airports));
            }
        }

        public AirportStruct DepartureAirport
        {
            get { return _departureAirport; }
            set
            {
                _departureAirport = value;
                OnPropertyChanged(nameof(DepartureAirport));

                if (_departureAirport != null)
                {
                    DepartureSelect?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    DepartureUnselect?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public AirportStruct ArrivalAirport
        {
            get { return _arrivalAirport; }
            set
            {
                _arrivalAirport = value;
                OnPropertyChanged(nameof(ArrivalAirport));

                if (_arrivalAirport != null)
                {
                    ArrivalSelect?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ArrivalUnselect?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion

        public AiroportPicker()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SearchFieldUpdate();
        }

        #region Методи

        #region Оновлення ListBox
        private void AirportsUpdate(string text)
        {
            if (DataBase.GetConnection().State != System.Data.ConnectionState.Open)
                return;

            List<AirportStruct> result = new List<AirportStruct>();
            string[] words = text.Replace(".", "").Replace(",", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string query = @"SELECT TOP 4 c.Name, a.City, a.Name, a.ICAO FROM Airport a JOIN Country c ON a.CountryID = c.ID ";

            if (words.Length > 0)
            {
                query += "WHERE " + string.Join(" AND ", words.Select((w, i) =>
                  $"(a.City LIKE @searchText{i} OR a.Name LIKE @searchText{i} OR c.Name LIKE @searchText{i} OR a.ICAO LIKE @searchText{i})"));
            }

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                for (int i = 0; i < words.Length; i++)
                {
                    command.Parameters.AddWithValue($"@searchText{i}", $"%{words[i]}%");
                }

                try
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new AirportStruct(
                                reader.IsDBNull(0) ? "" : reader.GetString(0),
                                reader.IsDBNull(1) ? "" : reader.GetString(1),
                                reader.IsDBNull(2) ? "" : reader.GetString(2),
                                reader.IsDBNull(3) ? "" : reader.GetString(3)
                            ));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            Airports = result;
            FocusChange();
        }

        private void FocusChange()
        {
            if (AirportsDisplay.Items.Count != 1 || SearchField.Length < 4)
            {
                AirportsDisplay.SelectedItem = null;
                return;
            }

            var airport = (AirportStruct)AirportsDisplay.Items[0];
            string search = SearchField.ToUpper().Trim();

            if (search.Length == 4 && search == airport.Code.ToUpper())
            {
                AirportsDisplay.SelectedItem = airport;
                return;
            }

            if (search == airport.Country.ToUpper() || search == airport.City.ToUpper() || search == airport.Name.ToUpper())
            {
                AirportsDisplay.SelectedItem = airport;
                return;
            }

            string[] words = search.Replace(".", "").Replace(",", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string airportString = airport.ToString().Replace(".", "").Replace(" ", "").ToUpper();

            foreach (var word in words)
            {
                airportString = airportString.Replace(word, "");
            }

            if (airportString.Length == 0)
            {
                AirportsDisplay.SelectedItem = airport;
                return;
            }

            if (_isDeparture)
            {
                DepartureAirport = null;
            }
            else
            {
                ArrivalAirport = null;
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid border && VisualTreeHelper.GetParent(border) is ListBoxItem item && item.Content is AirportStruct airport)
            {
                if (!item.IsSelected)
                {
                    SearchField = item.Content.ToString();
                    if (_isDeparture)
                    {
                        DepartureAirport = airport;
                        if (ArrivalAirport == null)
                            ToArrival();
                    }
                    else
                    {
                        ArrivalAirport = airport;
                        if (DepartureAirport == null)
                            ToDeparture();
                    }
                }
                else
                {
                    SearchField = "";
                    if (_isDeparture)
                        DepartureAirport = null;
                    else
                        ArrivalAirport = null;
                }

                item.IsSelected = !item.IsSelected;
            }
            e.Handled = true;
        }
        #endregion

        #region Інтерфейсу
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

        #region Взаємодії

        public void ToDeparture()
        {
            if (_isDeparture) return;

            ArrivalAirport = (AirportStruct)AirportsDisplay.SelectedItem;
            AirportsDisplay.UnselectAll();
            _isDeparture = true;
            SearchFieldUpdate();
        }

        public void ToArrival()
        {
            if (!_isDeparture) return;

            DepartureAirport = (AirportStruct)AirportsDisplay.SelectedItem;
            AirportsDisplay.UnselectAll();
            _isDeparture = false;
            SearchFieldUpdate();
        }

        public void Swap()
        {
            (DepartureAirport, ArrivalAirport) = (ArrivalAirport, DepartureAirport);
            (_searchArrivalField, _searchDepartureField) = (_searchDepartureField, _searchArrivalField);

            OnPropertyChanged(nameof(SearchArrivalField));
            OnPropertyChanged(nameof(SearchDepartureField));
        }

        private void SearchFieldUpdate()
        {
            string temp = SearchField;
            SearchField = "@";
            SearchField = temp;
        }

        #endregion

        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}