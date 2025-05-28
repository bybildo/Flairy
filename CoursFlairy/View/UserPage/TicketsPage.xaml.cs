using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CoursFlairy.Data;
using CoursFlairy.Model;
using Microsoft.Data.SqlClient;

namespace CoursFlairy.View.UserPage
{
    public partial class TicketsPage : Page, INotifyPropertyChanged
    {
        private List<int> _tickets;

        public List<int> Tickets
        {
            get => _tickets;
            set
            {
                _tickets = value;
                OnPropertyChanged(nameof(Tickets));
                UpdateVisibility();
            }
        }

        public TicketsPage()
        {
            InitializeComponent();
            DataContext = this;
            LoadUserTickets();
        }

        private void LoadUserTickets()
        {
            SqlConnection connection = null;
            try
            {
                connection = DataBase.GetConnection();
                string query = @"SELECT t.ID 
                               FROM Ticket t
                               JOIN Client c ON t.ClientID = c.ID
                               JOIN [User] u ON c.Email = u.Email
                               WHERE u.ID = @UserId
                               ORDER BY t.AddDate DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", CurrentAccount.id);
                    List<int> ticketIds = new List<int>();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ticketIds.Add(reader.GetInt32(0));
                        }
                    }

                    Tickets = ticketIds;
                }
            }
            catch (Exception ex)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show($"Помилка завантаження квитків: {ex.Message}");
            }
            finally
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void UpdateVisibility()
        {
            if (Tickets != null && Tickets.Count > 0)
            {
                TicketsContainer.Visibility = Visibility.Visible;
                NoTicketsMessage.Visibility = Visibility.Collapsed;
            }
            else
            {
                TicketsContainer.Visibility = Visibility.Collapsed;
                NoTicketsMessage.Visibility = Visibility.Visible;
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