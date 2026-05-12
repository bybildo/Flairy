using CoursFlairy.Model.Enum;
using CoursFlairy.Model.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursFlairy.Model
{
    public class FlightStruct
    {
        private AirportStruct _arrivalAirport;
        private AirportStruct _departureAirport;
        private List<DateTime> _dateFlight = new List<DateTime>();
        private int[] _personClasses;
        private int _flightID;
        private double _adultPrice;

        public int FlightId { get { return _flightID; } set { _flightID = value; } }
        public string DepartureCity { get { return $"{DepartureAirport.City}. {DepartureAirport.Code}"; } }
        public string ArrivalCity { get { return $"{ArrivalAirport.City}. {ArrivalAirport.Code}"; } }
        public string DepartureTime { get { return DateFlight[0].ToString("HH:mm"); } }
        public string DepartureDate { get { return DateFlight[0].ToString("dd.MM.yy"); } }
        public string ArrivalTime { get { return DateFlight[1].ToString("HH:mm"); } }
        public string ArrivalDate { get { return DateFlight[1].ToString("dd.MM.yy"); } }
        public Classes CurrentClass { get { return (Classes)_personClasses[0]; } }
        public string CurrentClassDisplay { get { 
            switch(CurrentClass) {
                case Classes.Econom: return "Economy";
                case Classes.Bussiness: return "Business";
                case Classes.First: return "First";
                default: return "Economy";
            }
        } }
        public double AdultPrice { get { return _adultPrice; } set { _adultPrice = value; } }
        public string AdultPriceDisplay { get { return $"{AdultPrice:F2} грн"; } }

        public string FlightDuration 
        { 
            get 
            {
                TimeSpan duration = DateFlight[1] - DateFlight[0];
                int hours = (int)duration.TotalHours;
                int minutes = duration.Minutes;
                
                if (hours > 0 && minutes > 0)
                    return $"{hours} год {minutes} хв";
                else if (hours > 0)
                    return $"{hours} год";
                else
                    return $"{minutes} хв";
            } 
        }
        public AirportStruct ArrivalAirport { get => _arrivalAirport; set => _arrivalAirport = value; }
        public AirportStruct DepartureAirport { get => _departureAirport; set => _departureAirport = value; }
        public List<DateTime> DateFlight { get => _dateFlight; set => _dateFlight = value; }
        public int[] PersonClasses { get => _personClasses; set => _personClasses = value; }

        public FlightStruct(AirportStruct departureAirport, AirportStruct arrivalAirport, List<DateTime> dateFlight, int[] personClasses)
        {
            ArrivalAirport = arrivalAirport;
            DepartureAirport = departureAirport;
            DateFlight = dateFlight;
            PersonClasses = personClasses;
        }

        public FlightStruct(int flightId , int Classes)
        {
            FlightId = flightId;
            PersonClasses = new int[] { Classes };
        }

        public override string ToString()
        {
            return $"Рейс з {DepartureCity} до {ArrivalCity} | Виліт: {DepartureTime} | Прибуття: {ArrivalTime} | Тривалість: {FlightDuration}";
        }

    }
}
