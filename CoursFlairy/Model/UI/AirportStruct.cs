using System.ComponentModel;

namespace CoursFlairy.Model.UI
{
    public record AirportStruct : INotifyPropertyChanged
    {
        private string _country { get; set; }
        private string _city { get; set; }
        private string _name { get; set; }
        private string _code { get; set; }

        public string Country
        {
            get { return _country; }
            set { _country = value; OnPropertyChanged(nameof(Country)); }
        }

        public string City
        {
            get { return _city; }
            set { _city = value; OnPropertyChanged(nameof(City)); }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Code
        {
            get { return _code; }
            set { _code = value; OnPropertyChanged(nameof(Code)); }
        }

        public AirportStruct(string country, string city, string name, string code)
        {
            Country = country;
            City = city;
            Name = name;
            Code = code;
        }

        public override string ToString()
        {
            return $"{Country}{(string.IsNullOrWhiteSpace(City) ? "" : ". " + City)}{(string.IsNullOrWhiteSpace(Name) ? "" : ". " + Name)}";
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
