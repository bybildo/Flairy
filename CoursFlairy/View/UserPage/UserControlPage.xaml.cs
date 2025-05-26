using CoursFlairy.Model.Enum;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static CoursFlairy.ViewModel.ResourceColor;

namespace CoursFlairy.View.UserPage
{
    /// <summary>
    /// Interaction logic for UserControlPage.xaml
    /// </summary>
    public partial class UserControlPage : Page
    {
        private StateUser _PageState = StateUser.Profile;

        public StateUser PageState
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

        public UserControlPage()
        {
            InitializeComponent();
            PageState = StateUser.Profile;
        }

        private void ChangeState(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            switch (border.Name)
            {
                case "ProfileBorder": PageState = StateUser.Profile; break;
                case "TicketsBorder": PageState = StateUser.Tickets; break;
                case "InterfaceBorder": PageState = StateUser.Interface; break;
                default: break;
            }
        }

        private void Exit(object sender, MouseButtonEventArgs e)
        {
            // TODO: Implement exit logic
            MessageBox.Show("Exit clicked");
        }

        private Page GetPageByState(StateUser state)
        {
            return state switch
            {
                StateUser.Profile => new ProfilePage(),
                StateUser.Tickets => new TicketsPage(),
                StateUser.Interface => new InterfacePage(),
                _ => null
            };
        }

        private Border GetBorderByState(StateUser state)
        {
            return state switch
            {
                StateUser.Profile => ProfileBorder,
                StateUser.Tickets => TicketsBorder,
                StateUser.Interface => InterfaceBorder,
                _ => null
            };
        }

        private void BorderAnimation(Border startBorder, Border endBorder)
        {
            var startText = ((TextBlock)((Viewbox)startBorder.Child).Child);
            var endText = ((TextBlock)((Viewbox)endBorder.Child).Child);

            if ((startBorder.Background is not SolidColorBrush brush1 || brush1.IsFrozen)) startBorder.Background = brush1 = new SolidColorBrush(White.Color);
            if ((endBorder.Background is not SolidColorBrush brush2 || brush2.IsFrozen)) endBorder.Background = brush2 = new SolidColorBrush(MainColor100.Color);
            if ((startText.Foreground is not SolidColorBrush brush3 || brush3.IsFrozen)) startText.Foreground = brush3 = new SolidColorBrush(MainColor100.Color);
            if ((endText.Foreground is not SolidColorBrush brush4 || brush4.IsFrozen)) endText.Foreground = brush4 = new SolidColorBrush(MainColor100.Color);

            var backgroundstartAnimation = new ColorAnimation
            {
                To = MainColor100.Color,
                Duration = TimeSpan.FromSeconds(0.1)
            };

            var backgroundendAnimation = new ColorAnimation
            {
                To = White.Color,
                Duration = TimeSpan.FromSeconds(0.1)
            };

            var foregroundstartAnimation = new ColorAnimation
            {
                To = White.Color,
                Duration = TimeSpan.FromSeconds(0.1)
            };

            var foregroundendAnimation = new ColorAnimation
            {
                To = MainColor100.Color,
                Duration = TimeSpan.FromSeconds(0.1)
            };

            brush1.BeginAnimation(SolidColorBrush.ColorProperty, backgroundstartAnimation);
            brush2.BeginAnimation(SolidColorBrush.ColorProperty, backgroundendAnimation);
            brush3.BeginAnimation(SolidColorBrush.ColorProperty, foregroundstartAnimation);
            brush4.BeginAnimation(SolidColorBrush.ColorProperty, foregroundendAnimation);
        }
    }
}
