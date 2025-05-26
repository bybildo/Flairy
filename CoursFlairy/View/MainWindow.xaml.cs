using CoursFlairy.Data;
using CoursFlairy.View.Bussiness;
using CoursFlairy.View.ClientPage;
using CoursFlairy.View.UI;
using CoursFlairy.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


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

        private async void Form1_Load(object sender, RoutedEventArgs e)
        {
            DataBase.Open();
            MainWindowVM viewModel = new MainWindowVM();
            this.DataContext = viewModel;
            CurrentAccount.Load();

            if (CurrentAccount.accountType == Model.Enum.AccountType.Bussines)
                PageManager.Navigate(new BussinessControlPage());
            else
            {
                PageManager.Navigate(new SearchPage());
                await CloudGenerate();
            }
        }

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

            foreach (var child in FindVisualChildren<Path>(original))
            {
                var clone = new Path
                {
                    Data = child.Data,
                    Fill = child.Fill,
                    Stretch = child.Stretch
                };
                newCanvas.Children.Add(clone);
            }

            return newCanvas;
        }

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

            AccountUI.BeginAnimation(LogIn.OpacityProperty, fadeInAnimation);
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

            AccountUI.BeginAnimation(LogIn.OpacityProperty, fadeInAnimation);
        }

        private void AccountUI_RegistonUser_MouseDown(object sender, EventArgs e)
        {
            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            AccountUI.BeginAnimation(LogIn.OpacityProperty, fadeInAnimation);
        }
    }
}