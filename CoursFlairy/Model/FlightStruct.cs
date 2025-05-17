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
    }
}
