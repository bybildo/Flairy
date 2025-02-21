using CoursFlairy.Data;
using CoursFlairy.View.UI;
using CoursFlairy.ViewModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;


namespace CoursFlairy.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AutoScreen();
            GlobalMessage.Visibility = Visibility.Visible;
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

        private async void AccountShow(object sender, MouseEventArgs e)
        {
            await Task.Delay(200);
            if (!AccountUI.IsMouseOver && !AccountGrid.IsMouseOver) return;

            AccountUI.Visibility = Visibility.Visible;

            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            AccountUI.BeginAnimation(AccountControl.OpacityProperty, fadeInAnimation);
        }

        private async void AccountHide(object sender, System.Windows.Input.MouseEventArgs e)
        {
            await Task.Delay(200);
            if (AccountUI.IsMouseOver || AccountGrid.IsMouseOver) return;

            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            AccountUI.BeginAnimation(AccountControl.OpacityProperty, fadeInAnimation);
        }

        private void AccountUI_RegistonUser_MouseDown(object sender, EventArgs e)
        {
            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            AccountUI.BeginAnimation(AccountControl.OpacityProperty, fadeInAnimation);
        }
    }
}