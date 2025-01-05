using System.Windows;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using CoursFlairy.Data;

namespace CoursFlairy
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, RoutedEventArgs e)
        {
            DataBase.Open();
        }
    }
}