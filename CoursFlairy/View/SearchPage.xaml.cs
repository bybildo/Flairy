using static CoursFlairy.ViewModel.ResourceColor;
using CoursFlairy.View.UI;
using Microsoft.IdentityModel.Tokens;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using CoursFlairy.Model;

namespace CoursFlairy.View
{
    /// <summary>
    /// Interaction logic for SearchPage.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        public SearchPage()
        {
            InitializeComponent();
            SearchGridScreenUpdate();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        #region Пошук елементів
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            var children = new List<T>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T)
                    children.Add((T)child);

                children.AddRange(FindVisualChildren<T>(child));
            }

            return children;
        }
        #endregion

        #region Інтерфейс

        private void SearchButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(AiroportPicker.DepartureAirport != null && AiroportPicker.ArrivalAirport != null && PeoplePicker.Images.Count > 0)) { BorderShackingAnimation(SearchButton); return; }

            int[] personClasses = new int[PeoplePicker.Images.Count()];
            for (int i = 0; i < PeoplePicker.Images.Count(); i++) personClasses[i] = (int)PeoplePicker.Class;

            FlightStruct filter = new FlightStruct(AiroportPicker.DepartureAirport, AiroportPicker.ArrivalAirport, DatePicker.GetSelectedDays(), personClasses);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.PageManager.Navigate(new SelectPage(filter));
        }

        private void SearchGridScreenUpdate()
        {
            SearchGrid.Height = SystemParameters.WorkArea.Height * 0.95;

            SearchGrid.RowDefinitions.Clear();
            SearchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25, GridUnitType.Star) });
            SearchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(22, GridUnitType.Star) });
            SearchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32, GridUnitType.Star) });
        }

        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!DatePicker.IsMouseOver)
            {
                DatePicker.Visibility = Visibility.Collapsed;
                BackHint.Text = "Назад (не обов'язково)";
                DateD.Opacity = 1;
                DateA.Opacity = 1;
            }

            if (!AiroportPicker.IsMouseOver)
            {
                AiroportPicker.Visibility = Visibility.Collapsed;
                (GridSet1.Width, GridSet2.Width) = (new GridLength(3, GridUnitType.Star), new GridLength(2, GridUnitType.Star));
            }

            if (!PeoplePicker.IsMouseOver)
            {
                PeoplePicker.Visibility = Visibility.Collapsed;
                Panel.SetZIndex(PeoplePickerBorder, 1);
                Panel.SetZIndex(SearchButton, 1);
            }
        }

        #region AiroportPicker
        private void AirportPickerDepartureShow(object sender, MouseEventArgs e)
        {
            AiroportPicker.ToDeparture();
            AiroportPicker.Visibility = Visibility.Visible;
            (GridSet1.Width, GridSet2.Width) = (new GridLength(2.5, GridUnitType.Star), new GridLength(2.5, GridUnitType.Star));
        }

        private void AirportPickerArrivalShow(object sender, MouseEventArgs e)
        {
            AiroportPicker.ToArrival();
            AiroportPicker.Visibility = Visibility.Visible;
            (GridSet1.Width, GridSet2.Width) = (new GridLength(2.5, GridUnitType.Star), new GridLength(2.5, GridUnitType.Star));
        }

        private void SwapArrows(object sender, MouseButtonEventArgs e)
        {
            if (AiroportPicker.Visibility == Visibility.Visible) AiroportPicker.Visibility = Visibility.Visible;

            Keyboard.ClearFocus();
            AiroportPicker.Swap();
        }
        #endregion

        #region DataPicker
        private void DatePickerShow(object sender, MouseEventArgs e)
        {
            DatePicker.Visibility = Visibility.Visible;
            DatePicker.WayForwardInterface();
            DateA.Opacity = 0.4;
            DateD.Opacity = 1;
        }

        private void DatePickerBackShow(object sender, MouseButtonEventArgs e)
        {
            if (DateA.Text.IsNullOrEmpty())
            {
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.GlobalMessage.Show("Оберіть дату відправлення", 3);
                return;
            }

            DatePicker.Visibility = Visibility.Visible;
            DatePicker.WayBackInterface();
            DateD.Opacity = 0.4;
            DateA.Opacity = 1;
        }
        #endregion

        #region PeoplePicker
        private void PeoplePickerShow(object sender, MouseEventArgs e)
        {
            PeoplePicker.Visibility = Visibility.Visible;
            Panel.SetZIndex(PeoplePickerBorder, 4);
            Panel.SetZIndex(SearchButton, 4);
        }
        #endregion

        private void ScrollViewer_PreviewHorizontalMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta != 0)
            {
                var scrollViewer = sender as ScrollViewer;
                if (scrollViewer != null)
                {
                    if (e.Delta > 0)
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 5);
                    }
                    else
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 5);
                    }

                    e.Handled = true;
                }
            }
        }

        private void ScrollViewer_PreviewVerticalMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double screenHeight = SystemParameters.WorkArea.Height;

            double speedFactor = 0.03;
            double minScreenHeight = 900;

            double speed = screenHeight * speedFactor;
            if (screenHeight < minScreenHeight)
            {
                speed *= 0.25;
            }
            else if (screenHeight > minScreenHeight * 2)
            {
                speed *= 1.5;
            }

            var currentScrollViewer = (ScrollViewer)sender;

            if (FindVisualChildren<ScrollViewer>(currentScrollViewer).Any(sv => sv.IsMouseOver))
                return;

            currentScrollViewer.ScrollToVerticalOffset(currentScrollViewer.VerticalOffset + (e.Delta > 0 ? -speed : speed));
            e.Handled = true;
        }
        #endregion

        #region Анімація

        private void BorderShackingAnimation(Border border)
        {
            if (border == null) return;

            ThicknessAnimationUsingKeyFrames animation = new ThicknessAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(500),
                RepeatBehavior = new RepeatBehavior(1)
            };

            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(13, 10, 7, 10), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(50))));
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(7, 10, 13, 10), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(12, 10, 8, 10), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150))));
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(8, 10, 12, 10), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(11, 10, 9, 10), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(250))));
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(9, 10, 11, 10), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(10, 10, 10, 10), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(350))));

            border.BeginAnimation(Border.MarginProperty, animation);
        }

        private void BorderAnimation(Border border, bool start = true, Color startColor = default)
        {
            if (startColor == default)
                startColor = (MainColor20 as SolidColorBrush)?.Color ?? Colors.Transparent;

            if (border.BorderBrush is not SolidColorBrush brush || brush.IsFrozen)
                border.BorderBrush = brush = new SolidColorBrush(startColor);

            if (start)
                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor100.Color, TimeSpan.FromSeconds(0.15)));
            else
                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor20.Color, TimeSpan.FromSeconds(0.15)));

            SearchButtonUpdate();
        }

        private void AiroportPicker_DepartureSelect(object sender, EventArgs e)
        {
            if (CountryLine.Stroke is not SolidColorBrush brush1 || brush1.IsFrozen)
                CountryLine.Stroke = brush1 = new SolidColorBrush(MainColor20.Color);

            if (CountryArrowD.Fill is not SolidColorBrush brush2 || brush2.IsFrozen)
                CountryArrowD.Fill = brush2 = new SolidColorBrush(MainColor20.Color);

            brush1.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(MainColor100.Color, TimeSpan.FromSeconds(0.15)));

            brush2.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(MainColor100.Color, TimeSpan.FromSeconds(0.15)));

            if (AiroportPicker.ArrivalAirport != null)
            {
                BorderAnimation(CountryBorder);
                Keyboard.ClearFocus();
                AiroportPicker.Visibility = Visibility.Collapsed;
                (GridSet1.Width, GridSet2.Width) = (new GridLength(3, GridUnitType.Star), new GridLength(2, GridUnitType.Star));
            }
            else
            {
                ArrivalTextBox.Focus();
            }

            SearchButtonUpdate();
        }

        private void AiroportPicker_DepartureUnselect(object sender, EventArgs e)
        {
            if (CountryLine.Stroke is not SolidColorBrush brush1 || brush1.IsFrozen)
                CountryLine.Stroke = brush1 = new SolidColorBrush(MainColor20.Color);

            if (CountryArrowD.Fill is not SolidColorBrush brush2 || brush2.IsFrozen)
                CountryArrowD.Fill = brush2 = new SolidColorBrush(MainColor20.Color);

            if (AiroportPicker.ArrivalAirport == null)
                brush1.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor20.Color, TimeSpan.FromSeconds(0.15)));

            brush2.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(MainColor20.Color, TimeSpan.FromSeconds(0.15)));

            BorderAnimation(CountryBorder, false);
        }

        private void AiroportPicker_ArrivalSelect(object sender, EventArgs e)
        {
            if (CountryLine.Stroke is not SolidColorBrush brush1 || brush1.IsFrozen)
                CountryLine.Stroke = brush1 = new SolidColorBrush(MainColor20.Color);

            if (CountryArrowA.Fill is not SolidColorBrush brush2 || brush2.IsFrozen)
                CountryArrowA.Fill = brush2 = new SolidColorBrush(MainColor20.Color);

            brush1.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(MainColor100.Color, TimeSpan.FromSeconds(0.15)));

            brush2.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(MainColor100.Color, TimeSpan.FromSeconds(0.15)));

            if (AiroportPicker.DepartureAirport != null)
            {
                BorderAnimation(CountryBorder);
                Keyboard.ClearFocus();
                AiroportPicker.Visibility = Visibility.Collapsed;
                (GridSet1.Width, GridSet2.Width) = (new GridLength(3, GridUnitType.Star), new GridLength(2, GridUnitType.Star));
            }
            else
            {
                DepartureTextBox.Focus();
            }

            SearchButtonUpdate();
        }

        private void AiroportPicker_ArrivalUnselect(object sender, EventArgs e)
        {
            if (CountryLine.Stroke is not SolidColorBrush brush1 || brush1.IsFrozen)
                CountryLine.Stroke = brush1 = new SolidColorBrush(MainColor20.Color);

            if (CountryArrowA.Fill is not SolidColorBrush brush2 || brush2.IsFrozen)
                CountryArrowA.Fill = brush2 = new SolidColorBrush(MainColor20.Color);

            if (AiroportPicker.DepartureAirport == null)
                brush1.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(MainColor20.Color, TimeSpan.FromSeconds(0.15)));

            brush2.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(MainColor20.Color, TimeSpan.FromSeconds(0.15)));

            BorderAnimation(CountryBorder, false);
        }

        private void DatePicker_Select(object sender, EventArgs e)
        {
            BorderAnimation(DateBorder);
        }

        private void DatePicker_Unselect(object sender, EventArgs e)
        {
            BorderAnimation(DateBorder, false);
        }

        private void PeoplePicker_Select(object sender, EventArgs e)
        {
            PeopleHint.Visibility = Visibility.Collapsed;
            BorderAnimation(PeoplePickerBorder, true, MainColor100.Color);
            SearchButtonUpdate();
        }

        private void PeoplePicker_Unselect(object sender, EventArgs e)
        {
            BorderAnimation(PeoplePickerBorder, false);
            PeopleHint.Visibility = Visibility.Visible;
            SearchButtonUpdate();
        }

        private void SearchButtonUpdate()
        {
            if (SearchButton.Background is not SolidColorBrush brush || brush.IsFrozen)
                SearchButton.Background = brush = new SolidColorBrush(Colors.LightBlue);

            if (AiroportPicker.DepartureAirport != null && AiroportPicker.ArrivalAirport != null && PeoplePicker.Images.Count > 0)
                brush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(MainColor100.Color, TimeSpan.FromSeconds(0.15)));
            else brush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(MainColor20.Color, TimeSpan.FromSeconds(0.15)));
        }

        private void SearchButton_AnimationEnter(object sender, MouseEventArgs e)
        {
            if (!(AiroportPicker.DepartureAirport != null && AiroportPicker.ArrivalAirport != null   && PeoplePicker.Images.Count > 0)) return;

            var dropShadowEffect = SearchButton.Effect as DropShadowEffect;
            if (dropShadowEffect != null)
            {
                var shadowAnimation = new DoubleAnimation
                {
                    To = 0.6,
                    Duration = TimeSpan.FromSeconds(0.25)
                };
                dropShadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, shadowAnimation);
            }
            return;
        }

        private void SearchButton_AnimationLeave(object sender, MouseEventArgs e)
        {
            var dropShadowEffect = SearchButton.Effect as DropShadowEffect;
            if (dropShadowEffect != null)
            {
                var shadowAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.25)
                };
                dropShadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, shadowAnimation);
            }
            return;
        }
        #endregion
    }
}