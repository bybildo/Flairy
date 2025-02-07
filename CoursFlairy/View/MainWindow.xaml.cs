using CoursFlairy.Data;
using CoursFlairy.ViewModel;
using System.Windows;


namespace CoursFlairy.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AutoScreen();
        }

        private void Form1_Load(object sender, RoutedEventArgs e)
        {
            DataBase.Open();
            MainWindowVM viewModel = new MainWindowVM();
            this.DataContext = viewModel;
        }

        private void AutoScreen()
        {
            var workArea = SystemParameters.WorkArea;

            this.Left = workArea.Left;
            this.Top = workArea.Top;
            this.Width = workArea.Width;
            this.Height = workArea.Height;

            line.StrokeThickness = workArea.Width / 1300;
        }
    }
}