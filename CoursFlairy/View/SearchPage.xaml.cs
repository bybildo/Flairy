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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await CloudGenerate();
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

        private void SearchGridScreenUpdate()
        {
            SearchGrid.Height = SystemParameters.WorkArea.Height * 0.95;

            SearchGrid.RowDefinitions.Clear();
            SearchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25, GridUnitType.Star) });
            SearchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(22, GridUnitType.Star) });
            SearchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32, GridUnitType.Star) });
        }

        private void SearchButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(AiroportPicker.DepartureAirport != null && AiroportPicker.ArrivalAirport != null && !string.IsNullOrEmpty(DatePicker.CheckedDays) && PeoplePicker.Images.Count > 0)) return;
            MessageBox.Show($"із: {AiroportPicker.DepartureAirport} \nдо: {AiroportPicker.ArrivalAirport} \nколи: {DatePicker.CheckedDays}\nколи назад: {DatePicker.CheckedBackDays} \nкількість: {PeoplePicker.Images.Count} \nхто: {PeoplePicker.Images.Aggregate((current, next) => current + ", " + next)}");
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

            double speed = SystemParameters.WorkArea.Height * 0.03;

            var currentScrollViewer = (ScrollViewer)sender;

            if (FindVisualChildren<ScrollViewer>(currentScrollViewer).Any(sv => sv.IsMouseOver))
                return;

            currentScrollViewer.ScrollToVerticalOffset(currentScrollViewer.VerticalOffset + (e.Delta > 0 ? -speed : speed));
            e.Handled = true;
        }
        #endregion

        #region Анімація

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

            if (AiroportPicker.DepartureAirport != null && AiroportPicker.ArrivalAirport != null && !string.IsNullOrEmpty(DatePicker.CheckedDays) && PeoplePicker.Images.Count > 0)
                brush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(MainColor100.Color, TimeSpan.FromSeconds(0.15)));
            else brush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(MainColor20.Color, TimeSpan.FromSeconds(0.15)));
        }

        private void SearchButton_AnimationEnter(object sender, MouseEventArgs e)
        {
            if  (!(AiroportPicker.DepartureAirport != null && AiroportPicker.ArrivalAirport != null && !string.IsNullOrEmpty(DatePicker.CheckedDays) && PeoplePicker.Images.Count > 0)) return;

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

        #region Генерація хмар
        private const int cloudSpeed = 40;
        private const int maxCloudCount = 10;

        private int currentCloudCount = 0;
        private async Task CloudGenerate()
        {
            Random random = new Random();

            for (int i = 1; i <= maxCloudCount - 2; i++)
            {
                await CreateCloud(random, i, true);
            }

            while (true)
            {
                if (currentCloudCount < maxCloudCount)
                {
                    await CreateCloud(random, 0);
                }

                await Task.Delay(1000);
            }
        }

        private async Task CreateCloud(Random random, int i, bool start = false)
        {
            Canvas cloud = GetRandomCloud();

            if (cloud == null) return;

            cloud.Width = 700;
            cloud.Height = 300;
            cloud.Opacity = random.NextDouble();

            int zIndex = (int)(cloud.Opacity * 100);
            Panel.SetZIndex(cloud, zIndex);

            int randomWidth = random.Next(300, 1000);
            Viewbox viewbox = new Viewbox
            {
                Stretch = Stretch.Uniform,
                Width = randomWidth
            };

            int heightDiapason = ((int)SearchBackground.ActualHeight - 100) / 2;
            double t = 1 - Math.Pow(random.NextDouble(), 4);
            int randomY = (int)(t * (2 * heightDiapason)) - heightDiapason;

            double edgesEnd = -(SearchBackground.ActualWidth / 2 + randomWidth / 2);
            double edgesStart = -edgesEnd;

            if (start)
            {
                edgesStart = edgesEnd + (SearchBackground.ActualWidth / (maxCloudCount - 2) * i);
            }

            viewbox.RenderTransform = new TranslateTransform(edgesStart, randomY);
            viewbox.Child = cloud;
            SearchBackground.Children.Add(viewbox);

            AnimateCloud(viewbox, edgesStart, edgesEnd);

            currentCloudCount++;
        }

        private void AnimateCloud(Viewbox cloud, double startX, double endX)
        {
            if (cloud.RenderTransform == null || !(cloud.RenderTransform is TranslateTransform))
            {
                cloud.RenderTransform = new TranslateTransform();
            }

            TranslateTransform transform = (TranslateTransform)cloud.RenderTransform;

            double stateProcent = (startX - endX) / (-endX * 2);

            double minDuration = cloudSpeed;
            double maxDuration = cloudSpeed * 4;
            double animationDurationSeconds = minDuration + (1 - cloud.Child.Opacity) * (maxDuration - minDuration);

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                From = startX,
                To = endX,
                Duration = TimeSpan.FromSeconds(animationDurationSeconds * stateProcent),
                FillBehavior = FillBehavior.Stop
            };

            moveAnimation.Completed += (s, e) =>
            {
                if (SearchBackground.Children.Contains(cloud))
                {
                    SearchBackground.Children.Remove(cloud);
                    currentCloudCount--;
                }
            };

            transform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
        }

        private Canvas GetRandomCloud()
        {
            Random random = new Random();
            Canvas original = (Canvas)FindResource($"cloud {random.Next(1, 7)}");
            Canvas newCanvas = new Canvas();

            foreach (UIElement child in original.Children)
            {
                if (child is Path path)
                {
                    Path newPath = new Path
                    {
                        Data = path.Data,
                        Fill = path.Fill.CloneCurrentValue(),
                    };
                    newCanvas.Children.Add(newPath);
                }
            }
            return newCanvas;
        }
        #endregion
    }
}