using System.Windows.Controls;
using System.Windows;
using CoursFlairy.Model;
using CoursFlairy.Model.Enum;
using CoursFlairy.Data;
using Microsoft.Data.SqlClient;

namespace CoursFlairy.View.UI
{
    /// <summary>
    /// Interaction logic for TicketUI.xaml
    /// </summary>
    public partial class TicketUI : UserControl
    {
        public static readonly DependencyProperty TicketIdProperty = DependencyProperty.Register(
            nameof(TicketId), typeof(int), typeof(TicketUI),
            new PropertyMetadata(0, OnTicketIdChanged));

        public int TicketId
        {
            get => (int)GetValue(TicketIdProperty);
            set => SetValue(TicketIdProperty, value);
        }

        public TicketUI()
        {
            InitializeComponent();
        }

        private static void OnTicketIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TicketUI ticketUI && e.NewValue is int id && id > 0)
            {
                ticketUI.LoadTicketInfo(id);
            }
        }

        private void LoadTicketInfo(int ticketId)
        {
            try
            {
                string query = @"
                    SELECT 
                        t.ID as TicketId,
                        c.Name as PassengerName,
                        c.Surname as PassengerSurname,
                        c.Passport,
                        depAir.City as DepartureCity,
                        arrAir.City as ArrivalCity,
                        depAir.ICAO as DepartureIcao,
                        arrAir.ICAO as ArrivalIcao,
                        f.DTime as DepartureTime,
                        DATEADD(MINUTE, r.AmountTime, f.DTime) as ArrivalTime,
                        t.Seat,
                        t.SeatCode,
                        t.Class,
                        t.Price,
                        t.Baggage,
                        al.Name as AirlineName,
                        t.AddDate
                    FROM Ticket t
                    JOIN Client c ON t.ClientID = c.ID
                    JOIN Flight f ON t.FlightID = f.ID
                    JOIN Route r ON f.RouteID = r.ID
                    JOIN Airport depAir ON r.DepartureID = depAir.ID
                    JOIN Airport arrAir ON r.ArrivalID = arrAir.ID
                    JOIN Plane pl ON r.PlaneID = pl.ID
                    JOIN Airline al ON pl.AirlineID = al.ID
                    WHERE t.ID = @ticketId";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@ticketId", ticketId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var ticketInfo = new TicketInfo
                            {
                                TicketId = (int)reader["TicketId"],
                                PassengerName = (string)reader["PassengerName"],
                                PassengerSurname = (string)reader["PassengerSurname"],
                                Passport = reader["Passport"] == DBNull.Value ? "" : (string)reader["Passport"],
                                DepartureCity = (string)reader["DepartureCity"],
                                ArrivalCity = (string)reader["ArrivalCity"],
                                DepartureIcao = (string)reader["DepartureIcao"],
                                ArrivalIcao = (string)reader["ArrivalIcao"],
                                DepartureTime = (System.DateTime)reader["DepartureTime"],
                                ArrivalTime = (System.DateTime)reader["ArrivalTime"],
                                Seat = (string)reader["Seat"],
                                SeatCode = (string)reader["SeatCode"],
                                Class = GetClassFromId((int)reader["Class"]),
                                Price = (decimal)reader["Price"],
                                Baggage = (bool)reader["Baggage"],
                                AirlineName = (string)reader["AirlineName"],
                                AddDate = (System.DateTime)reader["AddDate"],
                                Gate = GenerateGate()
                            };
                            this.DataContext = ticketInfo;
                        }
                    }
                }
            }
            catch { /* Можна додати логування */ }
        }

        private Classes GetClassFromId(int classId)
        {
            return classId switch
            {
                1 => Classes.Econom,
                2 => Classes.Bussiness,
                3 => Classes.First,
                _ => Classes.Econom
            };
        }

        private string GenerateGate()
        {
            // Генеруємо випадковий гейт для демонстрації
            var random = new System.Random();
            char letter = (char)('A' + random.Next(0, 26));
            int number = random.Next(1, 50);
            return $"{letter}{number}";
        }
    }
} 
