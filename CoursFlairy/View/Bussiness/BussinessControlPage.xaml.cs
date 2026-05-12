using CoursFlairy.Model.Enum;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static CoursFlairy.ViewModel.ResourceColor;

namespace CoursFlairy.View.Bussiness
{
    /// <summary>
    /// Interaction logic for BussinessControlPage.xaml
    /// </summary>
    public partial class BussinessControlPage : Page
    {
        private StateBussines _PageState = StateBussines.Profile;

        public StateBussines PageState
        {
            get { return _PageState; }
            set
            {
                if (value == _PageState) return;

                BorderAnimation(GetBorderByState(value), GetBorderByState(_PageState));
                _PageState = value;
                PageManager.Content = GetPageByState(value);
            }
        }

        public BussinessControlPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            InitializePageState();
        }

        private void InitializePageState()
        {
            // Set initial page content
            PageManager.Content = GetPageByState(StateBussines.Profile);
            
            // Ensure ProfileBorder is styled as active
            var profileBorder = GetBorderByState(StateBussines.Profile);
            var profileText = ((TextBlock)((Viewbox)profileBorder.Child).Child);
            
            if ((profileBorder.Background is not SolidColorBrush brush1 || brush1.IsFrozen)) 
                profileBorder.Background = new SolidColorBrush(MainColor100.Color);
            if ((profileText.Foreground is not SolidColorBrush brush2 || brush2.IsFrozen)) 
                profileText.Foreground = new SolidColorBrush(White.Color);
        }

        private void ChangeState(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            switch (border.Name)
            {
                case "ProfileBorder": PageState = StateBussines.Profile; break;
                case "PlaneBorder": PageState = StateBussines.Palne; break;
                case "RouteBorder": PageState = StateBussines.Route; break;
                case "FlightBorder": PageState = StateBussines.Flight; break;
                default: break;
            }
        }

        private Page GetPageByState(StateBussines state)
        {
            return state switch { StateBussines.Profile => new ProfilePage(), StateBussines.Palne => new PlanePage(), StateBussines.Route => new RoutePage(), StateBussines.Flight => new FlightPage(), _ => null };
        }

        private Border GetBorderByState(StateBussines state)
        {
            return state switch { StateBussines.Profile => ProfileBorder, StateBussines.Palne => PlaneBorder, StateBussines.Route => RouteBorder, StateBussines.Flight => FlightBorder, _ => null };
        }

        private void Exit(object sender, MouseButtonEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var result = mainWindow.GlobalMessage.ShowConfirm("Ви впевнені, що хочете вийти?");

            if (result == true)
            {
                CurrentAccount.Set(AccountType.None, -1);
                CurrentAccount.Save();
                mainWindow.PageManager.Navigate(new SearchPage());
                mainWindow.AccountUI.Visibility = Visibility.Hidden;
                mainWindow.AccountUI.ShowLogin();
            }
        }

        private void BorderAnimation(Border startBorder, Border endBorder)
        {
            // startBorder becomes active (MainColor100 background, White text)
            // endBorder becomes inactive (White background, MainColor100 text)
            
            var startText = ((TextBlock)((Viewbox)startBorder.Child).Child);
            var endText = ((TextBlock)((Viewbox)endBorder.Child).Child);

            if ((startBorder.Background is not SolidColorBrush brush1 || brush1.IsFrozen)) 
                startBorder.Background = brush1 = new SolidColorBrush(White.Color);
            if ((endBorder.Background is not SolidColorBrush brush2 || brush2.IsFrozen)) 
                endBorder.Background = brush2 = new SolidColorBrush(MainColor100.Color);
            if ((startText.Foreground is not SolidColorBrush brush3 || brush3.IsFrozen)) 
                startText.Foreground = brush3 = new SolidColorBrush(MainColor100.Color);
            if ((endText.Foreground is not SolidColorBrush brush4 || brush4.IsFrozen)) 
                endText.Foreground = brush4 = new SolidColorBrush(White.Color);

            // Animation to make startBorder active
            var backgroundActiveAnimation = new ColorAnimation
            {
                To = MainColor100.Color,
                Duration = TimeSpan.FromSeconds(0.1)
            };

            // Animation to make endBorder inactive
            var backgroundInactiveAnimation = new ColorAnimation
            {
                To = White.Color,
                Duration = TimeSpan.FromSeconds(0.1)
            };

            // Animation to make startBorder text white
            var textActiveAnimation = new ColorAnimation
            {
                To = White.Color,
                Duration = TimeSpan.FromSeconds(0.1)
            };

            // Animation to make endBorder text MainColor100
            var textInactiveAnimation = new ColorAnimation
            {
                To = MainColor100.Color,
                Duration = TimeSpan.FromSeconds(0.1)
            };

            brush1.BeginAnimation(SolidColorBrush.ColorProperty, backgroundActiveAnimation);
            brush2.BeginAnimation(SolidColorBrush.ColorProperty, backgroundInactiveAnimation);
            brush3.BeginAnimation(SolidColorBrush.ColorProperty, textActiveAnimation);
            brush4.BeginAnimation(SolidColorBrush.ColorProperty, textInactiveAnimation);
        }
    }
}
