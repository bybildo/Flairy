using CoursFlairy.Model.Enum;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using static CoursFlairy.ViewModel.ResourceColor;

namespace CoursFlairy.View.UI
{
    /// <summary>
    /// Interaction logic for PeoplePicker.xaml
    /// </summary>
    public partial class PeoplePicker : UserControl, INotifyPropertyChanged
    {
        private string _baby = "0";
        private string _child = "0";
        private string _adult = "1";
        private string _pensioner = "0";
        private Classes _class = Classes.Econom;
        private List<string> _images = new List<string>();

        public event EventHandler PeopleSelect;
        public event EventHandler PeopleUnselect;

        public string Baby { get => _baby; set { _baby = value; OnPropertyChanged(nameof(Baby)); UpdateImage(); } }
        public string Child { get => _child; set { _child = value; OnPropertyChanged(nameof(Child)); UpdateImage(); } }
        public string Adult { get => _adult; set { _adult = value; OnPropertyChanged(nameof(Adult)); UpdateImage(); } }
        public string Pensioner { get => _pensioner; set { _pensioner = value; OnPropertyChanged(nameof(Pensioner)); UpdateImage(); } }
        public Classes Class { get => _class; set { _class = value; OnPropertyChanged(nameof(Class)); UpdateImage(); } }
        public List<string> Images { get => _images; }

        public PeoplePicker()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Grid initialGrid = Economy;
            Grid_PreviewMouseDown(initialGrid, null);
        }

        #region Вибір класу
        private void Grid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Grid grid1 = sender as Grid;
            GridAnimation_MouseLeave(grid1, e);

            if (grid1 == null) return;

            List<(Grid Grid, Classes ClassType)> grids = new List<(Grid, Classes)> { (Economy, Classes.Econom), (Business, Classes.Bussiness), (First, Classes.First) };

            foreach (var (grid, classType) in grids)
            {
                bool isCurrentGrid = grid == grid1;


                Border border = FindChild<Border>(grid);
                if (border != null)
                {
                    border.Background = isCurrentGrid ? MainColor100 : new SolidColorBrush(Colors.Transparent);
                    border.Margin = isCurrentGrid ? new Thickness(-0.4) : new Thickness(0);
                }

                Viewbox viewbox = FindChild<Viewbox>(grid);
                if (viewbox != null)
                {
                    TextBlock textBlock = FindChild<TextBlock>(viewbox);
                    if (textBlock != null)
                    {
                        textBlock.Foreground = isCurrentGrid ? White : MainColor100;
                        textBlock.Opacity = 1;
                    }
                }

                if (isCurrentGrid)
                {
                    Class = classType;
                }
            }
        }

        public static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T)
                    return (T)child;

                T result = FindChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
        #endregion

        #region  Вибір кількості
        private ref string GetCategoryRef(char category)
        {
            switch (category)
            {
                case 'b':
                    return ref _baby;
                case 'c':
                    return ref _child;
                case 'a':
                    return ref _adult;
                case 'p':
                    return ref _pensioner;
                default:
                    throw new ArgumentException("Invalid category");
            }
        }

        private void Minus_MouseDown(object sender, MouseButtonEventArgs e)
        {
            char category = ((Grid)sender).Name[0];
            ref string element = ref GetCategoryRef(category);

            if (element != null)
            {
                if (!int.TryParse(element, out int value)) return;

                if (value > 0)
                {
                    element = (value - 1).ToString();
                }
            }

            OnPropertyChanged(nameof(Baby));
            OnPropertyChanged(nameof(Child));
            OnPropertyChanged(nameof(Adult));
            OnPropertyChanged(nameof(Pensioner));
            UpdateImage();
        }

        private void Plus_MouseDown(object sender, MouseButtonEventArgs e)
        {
            char category = ((Grid)sender).Name[0];
            ref string element = ref GetCategoryRef(category);

            if (element != null)
            {
                if (!int.TryParse(element, out int value)) return;

                if (value < 99)
                {
                    element = (value + 1).ToString();
                }
            }

            OnPropertyChanged(nameof(Baby));
            OnPropertyChanged(nameof(Child));
            OnPropertyChanged(nameof(Adult));
            OnPropertyChanged(nameof(Pensioner));
            UpdateImage();
        }

        public static T FindElementByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T element && element.Name == name)
                {
                    return element;
                }

                var result = FindElementByName<T>(child, name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
        #endregion

        #region Ввдення даних
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var vb = FindChild<Viewbox>((Border)sender);
            var tb = FindChild<TextBox>((Viewbox)vb);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                tb.Focus();
                tb.CaretIndex = tb.Text.Length;
            }));
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            string fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            if (fullText.StartsWith("00") || !int.TryParse(fullText, out _))
            {
                e.Handled = true;
            }

            if (fullText.StartsWith("0"))
            {
                if (int.TryParse(e.Text, out _))
                    ((TextBox)sender).Text = e.Text;
                ((TextBox)sender).CaretIndex = 1;
                e.Handled = true;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
                TextBox_LostFocus(sender, e);
                return;
            }

            if (e.Key == Key.Up || e.Key == Key.PageUp)
            {
                var dict = new Dictionary<string, string>() { { "pt", "at" }, { "at", "ct" }, { "ct", "bt" }, { "bt", "pt" } };

                var el = FindElementByName<TextBox>(Category, dict[((TextBox)sender).Name]);
                if (el != null)
                {
                    el.Focus();
                    el.CaretIndex = 1;
                }
            }

            if (e.Key == Key.Down || e.Key == Key.PageDown)
            {
                var dict = new Dictionary<string, string>() { { "pt", "bt" }, { "at", "pt" }, { "ct", "at" }, { "bt", "ct" } };

                var el = FindElementByName<TextBox>(Category, dict[((TextBox)sender).Name]);
                if (el != null)
                {
                    el.Focus();
                    el.CaretIndex = 1;
                }
            }
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text == "")
            {
                ((TextBox)sender).Text = "0";
            }

            TextBoxAnimation_LostFocus(sender, e);
        }
        #endregion

        #region Анімація
        private void GridAnimation_MouseEnter(object sender, MouseEventArgs e)
        {
            var border = FindChild<Border>((Grid)sender);
            if (border != null && ((SolidColorBrush)border.Background).Color == MainColor100.Color) return;

            var vb = FindChild<Viewbox>((Grid)sender);
            var tb = FindChild<TextBlock>((Viewbox)vb);

            if (tb != null && tb.Foreground is SolidColorBrush color && color.Color == MainColor10.Color) return;

            if (tb != null)
            {
                var textAnimation = new DoubleAnimation
                {
                    To = 0.7,
                    Duration = TimeSpan.FromSeconds(0.25)
                };
                tb.BeginAnimation(TextBlock.OpacityProperty, textAnimation);
            }

            var el = FindChild<Ellipse>((Grid)sender);

            if (el != null)
            {
                var dropShadowEffect = el.Effect as DropShadowEffect;
                if (dropShadowEffect != null)
                {
                    var shadowAnimation = new DoubleAnimation
                    {
                        To = 0.65,
                        Duration = TimeSpan.FromSeconds(0.25)
                    };
                    dropShadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, shadowAnimation);
                }
                return;
            }

            if (border != null)
            {
                border.Background = White;
                var dropShadowEffect = border.Effect as DropShadowEffect;
                if (dropShadowEffect != null)
                {
                    var shadowAnimation = new DoubleAnimation
                    {
                        To = 0.2,
                        Duration = TimeSpan.FromSeconds(0.25)
                    };
                    dropShadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, shadowAnimation);
                }
            }
        }

        private void GridAnimation_MouseLeave(object sender, MouseEventArgs e)
        {
            var vb = FindChild<Viewbox>((Grid)sender);
            var tb = FindChild<TextBlock>((Viewbox)vb);

            if (tb.Foreground is SolidColorBrush color && color.Color == MainColor10.Color) return;

            if (tb != null)
            {
                var textAnimation = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.25)
                };
                tb.BeginAnimation(TextBlock.OpacityProperty, textAnimation);
            }

            var el = FindChild<Ellipse>((Grid)sender);

            if (el != null)
            {
                var dropShadowEffect = el.Effect as DropShadowEffect;
                if (dropShadowEffect != null)
                {
                    var shadowAnimation = new DoubleAnimation
                    {
                        To = 0.3,
                        Duration = TimeSpan.FromSeconds(0.25)
                    };
                    dropShadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, shadowAnimation);
                }
                return;
            }

            var border = FindChild<Border>((Grid)sender);
            if (border != null)
            {
                BorderAnimation_MouseLeave(border, e);
                if (((SolidColorBrush)border.Background).Color != MainColor100.Color)
                    border.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void BorderAnimtion_MouseEnter(object sender, MouseEventArgs e)
        {
            var el = (Border)sender;

            if (el != null)
            {
                var dropShadowEffect = el.Effect as DropShadowEffect;
                if (dropShadowEffect != null)
                {
                    var shadowAnimation = new DoubleAnimation
                    {
                        To = 0.65,
                        Duration = TimeSpan.FromSeconds(0.2)
                    };
                    dropShadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, shadowAnimation);
                }
            }
        }

        private void BorderAnimation_MouseLeave(object sender, MouseEventArgs e)
        {
            var el = (Border)sender;

            if (el != null)
            {
                var dropShadowEffect = el.Effect as DropShadowEffect;
                if (dropShadowEffect != null)
                {
                    var shadowAnimation = new DoubleAnimation
                    {
                        To = 0.3,
                        Duration = TimeSpan.FromSeconds(0.2)
                    };
                    dropShadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, shadowAnimation);
                }
            }
        }

        private void TextBoxAnimation_GotFocus(object sender, RoutedEventArgs e)
        {
            var el = (TextBox)sender;

            if (el != null)
            {
                var textAnimation = new DoubleAnimation
                {
                    To = 0.6,
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                el.BeginAnimation(TextBox.OpacityProperty, textAnimation);
            }
        }

        private void TextBoxAnimation_LostFocus(object sender, RoutedEventArgs e)
        {
            var el = (TextBox)sender;

            if (el != null)
            {
                var textAnimation = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                el.BeginAnimation(TextBox.OpacityProperty, textAnimation);
            }
        }
        #endregion

        #region Методи
        private void UpdateImage()
        {
            int numOfClass = (int)Class + 1;

            var categories = new Dictionary<string, string> { { "pensioner", Pensioner }, { "adult", Adult }, { "child", Child }, { "baby", Baby } };
            List<string> result = new List<string>();

            foreach (var category in categories)
                if (int.TryParse(category.Value, out int count) && count > 0)
                    for (int i = 0; i < count; i++)
                        result.Add($"{category.Key} {numOfClass}");

            _images = result;
            OnPropertyChanged(nameof(Images));

            if (_images.Count > 0)
            {
                PeopleSelect?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                PeopleUnselect?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}