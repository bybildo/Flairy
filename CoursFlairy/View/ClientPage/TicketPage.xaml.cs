using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CoursFlairy.View.ClientPage
{
    /// <summary>
    /// Interaction logic for TicketPage.xaml
    /// </summary>
    public partial class TicketPage : Page, INotifyPropertyChanged
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

        public TicketPage(List<int> ticketIds)
        {
            InitializeComponent();
            DataContext = this;
            Tickets = ticketIds;
        }

        public TicketPage()
        {
            InitializeComponent();
            DataContext = this;
            
            var parent = FindParent<SelectPage>(this);
            if (parent != null)
            {
                Tickets = parent.TicketId;
            }
            
            UpdateVisibility();
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

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                child = VisualTreeHelper.GetParent(child);
                if (child is T parent)
                {
                    return parent;
                }
            }
            return null;
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
