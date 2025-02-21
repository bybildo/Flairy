using CoursFlairy.Data;
using CoursFlairy.Model.Enum;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using static CoursFlairy.ViewModel.ResourceColor;


namespace CoursFlairy.View.UI
{
    /// <summary>
    /// Interaction logic for UserPassportData.xaml
    /// </summary>
    public partial class UserPassportData : UserControl, INotifyPropertyChanged
    {
        public Gender gender = Gender.none;
        private string _surname = "";
        private string _name = "";
        private string _citizenship = "";
        private string _citizenshipHint = "";
        private string _passport = "";

        private string _personalDay = "";
        private string _personalMonth = "";
        private string _personalYear = "";

        private string _passportDay = "";
        private string _passportMonth = "";
        private string _passportYear = "";

        public event EventHandler DataChanged;

        #region Властивості
        public string CitizenshipHint
        {
            get { return _citizenshipHint; }
            set
            {
                _citizenshipHint = value;
                if (CitizenshipHint == Citizenship && Citizenship != "")
                {
                    BorderAnimation(CitizenshipBorder, false);
                    citizenshipButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    BorderAnimation(CitizenshipBorder);
                    citizenshipButton.Visibility = Visibility.Visible;
                }
                OnPropertyChanged(nameof(CitizenshipHint));
            }
        }

        public DateTime PersonalDate
        {
            get
            {
                if (DateTime.TryParse($"{personalDaytb.Text}/{personalMonthtb.Text}/{personalYeartb.Text}", out DateTime personalDate))
                {
                    return personalDate;
                }
                return DateTime.MinValue;
            }
        }

        public DateTime PassportDate
        {
            get
            {
                if (DateTime.TryParse($"{passportDaytb.Text}/{passportMonthtb.Text}/{passportYeartb.Text}", out DateTime passportDate))
                {
                    return passportDate;
                }
                return DateTime.MinValue;
            }
        }

        public string Surname
        {
            get { return _surname; }
            set
            {
                _surname = value;
                OnPropertyChanged(nameof(Surname));
            }
        }

        public string Namee
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Passport
        {
            get { return _passport; }
            set
            {
                _passport = value;
                if (Passport.Length >= 6 && Passport.Length <= 12) BorderAnimation(passportBorder, false);
                else BorderAnimation(passportBorder);
                OnPropertyChanged(nameof(Passport));
            }
        }

        public string Citizenship
        {
            get { return _citizenship; }
            set
            {
                _citizenship = value;
                CitizenshipHint = "";
                CitizenshipUpdate();
                OnPropertyChanged(nameof(Citizenship));
            }
        }

        public string PersonalDay
        {
            get { return _personalDay; }
            set
            {
                _personalDay = value;
                OnPropertyChanged(nameof(PersonalDay));
            }
        }

        public string PersonalMonth
        {
            get { return _personalMonth; }
            set
            {
                _personalMonth = value;
                OnPropertyChanged(nameof(PersonalMonth));
            }
        }

        public string PersonalYear
        {
            get { return _personalYear; }
            set
            {
                _personalYear = value;
                OnPropertyChanged(nameof(PersonalYear));
            }
        }

        public string PassportDay
        {
            get { return _passportDay; }
            set
            {
                _passportDay = value;
                OnPropertyChanged(nameof(PassportDay));
            }
        }

        public string PassportMonth
        {
            get { return _passportMonth; }
            set
            {
                _passportMonth = value;
                OnPropertyChanged(nameof(PassportMonth));
            }
        }

        public string PassportYear
        {
            get { return _passportYear; }
            set
            {
                _passportYear = value;
                OnPropertyChanged(nameof(PassportYear));
            }
        }
        #endregion

        public UserPassportData()
        {
            InitializeComponent();
        }

        #region Методи взаємодії
        #region Гендер
        private void GridAnimation_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((Grid)sender).Name.Replace("Grid", "").ToLower() == gender.ToString()) return;

            var border = ((Border)((Grid)sender).Children[0]);
            var dropShadowEffect = border.Effect as DropShadowEffect;

            dropShadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, null);

            var shadowAnimation = new DoubleAnimation
            {
                To = 0.2,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            dropShadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, shadowAnimation);
        }

        private void GridAnimation_MouseLeave(object sender, MouseEventArgs e)
        {
            var border = ((Border)((Grid)sender).Children[0]);
            var dropShadowEffect = border.Effect as DropShadowEffect;

            if (dropShadowEffect == null) return;

            var shadowAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            dropShadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, shadowAnimation);
        }


        private void Grid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Enum.TryParse(((Grid)sender).Name.Replace("Grid", ""), true, out Gender selectedGender))
            {
                if (gender == selectedGender && gender != Gender.none)
                {
                    gender = Gender.none;

                    ApplyColorAnimation(ManGrid.Children[0] as Border, Colors.White);
                    ApplyColorAnimation(((Viewbox)ManGrid.Children[1]).Child as TextBlock, Colors.Navy);

                    ApplyColorAnimation(WomanGrid.Children[0] as Border, Colors.White);
                    ApplyColorAnimation(((Viewbox)WomanGrid.Children[1]).Child as TextBlock, Colors.HotPink);

                    DataChanged?.Invoke(this, EventArgs.Empty);
                    return;
                }
                gender = selectedGender;
            }

            var selectedGrid = (Grid)sender;
            var selectedBorder = (Border)selectedGrid.Children[0];
            var selectedTextBlock = (TextBlock)((Viewbox)selectedGrid.Children[1]).Child;

            var oppositeGrid = gender == Gender.man ? WomanGrid : ManGrid;
            var oppositeBorder = (Border)oppositeGrid.Children[0];
            var oppositeTextBlock = (TextBlock)((Viewbox)oppositeGrid.Children[1]).Child;

            ApplyColorAnimation(selectedBorder, gender == Gender.man ? Colors.Navy : Colors.HotPink);
            ApplyColorAnimation(selectedTextBlock, Colors.White);
            ApplyColorAnimation(oppositeBorder, Colors.White);
            ApplyColorAnimation(oppositeTextBlock, gender == Gender.man ? Colors.HotPink : Colors.Navy);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyColorAnimation(UIElement element, Color toColor)
        {
            if (element is Border border)
            {
                var brush = border.Background as SolidColorBrush ?? new SolidColorBrush();
                border.Background = new SolidColorBrush(brush.Color);
                ((SolidColorBrush)border.Background).BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation { To = toColor, Duration = TimeSpan.FromSeconds(0.1) });
            }
            else if (element is TextBlock textBlock)
            {
                var brush = textBlock.Foreground as SolidColorBrush ?? new SolidColorBrush();
                textBlock.Foreground = new SolidColorBrush(brush.Color);
                ((SolidColorBrush)textBlock.Foreground).BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation { To = toColor, Duration = TimeSpan.FromSeconds(0.1) });
            }
        }
        #endregion

        #region MouseDown
        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            surnametb.Focus();
            DataChanged?.Invoke(this, EventArgs.Empty);
            Keyboard.ClearFocus();
        }
        #endregion

        #region Focus
        private void GridChildrenFocus(object sender, MouseButtonEventArgs e)
        {
            ((TextBox)((Viewbox)((Grid)((Grid)sender).Parent).Children[1]).Child).Focus();
        }

        private void highlightGot(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Opacity = 0.5;
        }

        private void highlightLost(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Opacity = 1;

            if (PersonalDate != DateTime.MinValue && PersonalDate > DateTime.Now.AddYears(-130) && PersonalDate < DateTime.Now) BorderAnimation(personalDateBorder, false);
            else BorderAnimation(personalDateBorder);

            if (PassportDate != DateTime.MinValue && PassportDate >= DateTime.Now && PassportDate < DateTime.Now.AddYears(15)) BorderAnimation(passportDateBorder, false);
            else BorderAnimation(passportDateBorder);

            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region KeyControl
        private void KeyControl(object sender, KeyEventArgs e)
        {
            var tb = (TextBox)sender;
            if (e.Key == Key.Space && !(tb.Name.Contains("citizenshiptb") || ((Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V))))
            {
                e.Handled = true;
                return;
            }

            if ((e.Key == Key.Tab || e.Key == Key.Enter) && tb.Name.Contains("citizenshiptb"))
            {
                Citizenship = CitizenshipHint;
            }

            if (e.Key == Key.Down)
            {
                Dictionary<TextBox, TextBox> queue = new Dictionary<TextBox, TextBox>() { { surnametb, nametb }, { nametb, personalDaytb }, { personalDaytb, surnametb }, { personalMonthtb, surnametb }, { personalYeartb, surnametb } };
                if (queue.ContainsKey(tb))
                {
                    var nextControl = queue[tb];
                    nextControl.Focus();
                }
                else
                {
                    queue = new Dictionary<TextBox, TextBox>() { { citizenshiptb, passporttb }, { passporttb, passportDaytb }, { passportDaytb, citizenshiptb }, { passportMonthtb, citizenshiptb }, { passportYeartb, citizenshiptb } };
                    if (queue.ContainsKey(tb))
                    {
                        var nextControl = queue[tb];
                        nextControl.Focus();
                    }
                }
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up)
            {
                Dictionary<TextBox, TextBox> queue = new Dictionary<TextBox, TextBox>() { { surnametb, personalDaytb }, { nametb, surnametb }, { personalDaytb, nametb }, { personalMonthtb, nametb }, { personalYeartb, nametb } };
                if (queue.ContainsKey(tb))
                {
                    var nextControl = queue[tb];
                    nextControl.Focus();
                }
                else
                {

                    queue = new Dictionary<TextBox, TextBox>() { { citizenshiptb, personalDaytb }, { passporttb, citizenshiptb }, { passportDaytb, passporttb }, { passportMonthtb, passporttb }, { passportYeartb, passporttb } };
                    if (queue.ContainsKey(tb))
                    {
                        var nextControl = queue[tb];
                        nextControl.Focus();
                    }
                }
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                Dictionary<TextBox, TextBox> queue = new Dictionary<TextBox, TextBox>() { { surnametb, nametb }, { nametb, personalDaytb }, { personalDaytb, personalMonthtb }, { personalMonthtb, personalYeartb }, { personalYeartb, citizenshiptb }, { citizenshiptb, passporttb }, { passporttb, passportDaytb }, { passportDaytb, passportMonthtb }, { passportMonthtb, passportYeartb } };
                if (queue.ContainsKey(tb))
                {
                    var nextControl = queue[tb];
                    nextControl.Focus();
                }
                else Keyboard.ClearFocus();
                e.Handled = true;
                return;
            }

            if (tb.Name.Contains("personal") && (e.Key == Key.Left || e.Key == Key.Right))
            {
                Dictionary<TextBox, TextBox> queue = new Dictionary<TextBox, TextBox>() { { personalDaytb, personalMonthtb }, { personalMonthtb, personalYeartb }, { personalYeartb, personalDaytb } };

                if (e.Key == Key.Left)
                {
                    var previousQueue = queue.FirstOrDefault(x => x.Value == tb).Key;
                    if (previousQueue != null)
                    {
                        previousQueue.Focus();
                    }
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Right)
                {
                    if (queue.ContainsValue(tb))
                    {
                        queue[tb].Focus();
                    }
                    e.Handled = true;
                    return;
                }
            }

            if (tb.Name.Contains("passport") && (e.Key == Key.Left || e.Key == Key.Right))
            {
                Dictionary<TextBox, TextBox> queue = new Dictionary<TextBox, TextBox>() { { passportDaytb, passportMonthtb }, { passportMonthtb, passportYeartb }, { passportYeartb, passportDaytb } };

                if (e.Key == Key.Left)
                {
                    var previousQueue = queue.FirstOrDefault(x => x.Value == tb).Key;
                    if (previousQueue != null)
                    {
                        previousQueue.Focus();
                    }
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Right)
                {
                    if (queue.ContainsValue(tb))
                    {
                        queue[tb].Focus();
                    }
                    e.Handled = true;
                    return;
                }
            }
        }
        #endregion

        #region TextInput
        private const string passportPattern = @"^[A-Z0-9]*$";
        private void passport_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            bool isTextAllowed = Regex.IsMatch(e.Text, passportPattern);
            if (!isTextAllowed)
            {
                e.Handled = true;
                ControlShakingAnimation(((FrameworkElement)((FrameworkElement)sender).Parent).Parent);
            }
        }

        private void name_TextInput(object sender, TextCompositionEventArgs e)
        {
            Regex namePattern = new Regex("^[a-zA-Z'\\s]+$");
            bool isTextAllowed = namePattern.IsMatch(e.Text);

            if (!isTextAllowed)
            {
                e.Handled = true;
                ControlShakingAnimation(((Control)((Control)sender).Parent).Parent);
            }
        }

        private void number_TextInput(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            string fullText = (tb.Text + e.Text);
            bool isTextAllowed = int.TryParse(fullText, out int result);
            if (!isTextAllowed)
            {
                e.Handled = true;
                ControlShakingAnimation(((FrameworkElement)((FrameworkElement)sender).Parent).Parent);
            }

            if (tb.Name.Contains("Day"))
            {
                if (result > 31 || result < 1)
                {
                    e.Handled = true;
                    ControlShakingAnimation(((FrameworkElement)((FrameworkElement)sender).Parent).Parent);
                }
            }

            if (tb.Name.Contains("Month"))
            {
                if (result > 12 || result < 1)
                {
                    e.Handled = true;
                    ControlShakingAnimation(((FrameworkElement)((FrameworkElement)sender).Parent).Parent);
                }
            }

            if (tb.Name.Contains("Year") && tb.Name != "passportYeartb")
            {
                if (result > DateTime.Now.Year)
                {
                    e.Handled = true;
                    ControlShakingAnimation(((FrameworkElement)((FrameworkElement)sender).Parent).Parent);
                }
            }

            if (PersonalDate != DateTime.MinValue && PersonalDate > DateTime.Now.AddYears(-130) && PersonalDate < DateTime.Now) BorderAnimation(personalDateBorder, false);
            else BorderAnimation(personalDateBorder);

            if (PassportDate != DateTime.MinValue && PassportDate > DateTime.Now && PassportDate < DateTime.Now.AddYears(15)) BorderAnimation(passportDateBorder, false);
            else BorderAnimation(passportDateBorder);
        }
        #endregion

        #region TextChange

        private const string namePattern = @"^[a-zA-Z]*$";
        private void surname_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (surnametb.Text.Length != 0 && Regex.IsMatch(((TextBox)sender).Text, namePattern))
            {
                BorderAnimation(surnameBorder, false);
            }
            else
            {
                BorderAnimation(surnameBorder);
            }
        }

        private void name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (nametb.Text.Length != 0 && Regex.IsMatch(((TextBox)sender).Text, namePattern))
            {
                BorderAnimation(nameBorder, false);
            }
            else
            {
                BorderAnimation(nameBorder);
            }
        }
        #endregion

        #region Anime
        private bool isAnimating = false;
        private void ControlShakingAnimation(object elementForAnimation)
        {
            if (elementForAnimation is FrameworkElement control && !isAnimating)
            {
                isAnimating = true;

                Thickness currentMargin = control.Margin;

                ThicknessAnimationUsingKeyFrames animation = new ThicknessAnimationUsingKeyFrames
                {
                    Duration = TimeSpan.FromMilliseconds(500),
                    RepeatBehavior = new RepeatBehavior(1)
                };

                animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(currentMargin.Left + 3, currentMargin.Top, currentMargin.Right - 3, currentMargin.Bottom), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(50))));
                animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(currentMargin.Left - 3, currentMargin.Top, currentMargin.Right + 3, currentMargin.Bottom), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
                animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(currentMargin.Left + 2, currentMargin.Top, currentMargin.Right - 2, currentMargin.Bottom), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150))));
                animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(currentMargin.Left - 2, currentMargin.Top, currentMargin.Right + 2, currentMargin.Bottom), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
                animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(currentMargin.Left + 1, currentMargin.Top, currentMargin.Right - 1, currentMargin.Bottom), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(250))));
                animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(currentMargin.Left - 1, currentMargin.Top, currentMargin.Right + 1, currentMargin.Bottom), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
                animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(currentMargin, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(350))));

                animation.Completed += (s, e) =>
                {
                    isAnimating = false;
                };

                control.BeginAnimation(FrameworkElement.MarginProperty, animation);
            }
        }

        private void BorderAnimation(Border border, bool start = true)
        {

            if (border.BorderBrush is not SolidColorBrush brush || brush.IsFrozen)
                border.BorderBrush = brush = new SolidColorBrush(MainColor20.Color);

            if (!start)
                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor100.Color, TimeSpan.FromSeconds(0.15)));
            else
                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor20.Color, TimeSpan.FromSeconds(0.15)));
        }
        #endregion
        #endregion

        public void CitizenshipUpdate()
        {
            if (DataBase.GetConnection().State != System.Data.ConnectionState.Open)
                return;

            if (Citizenship == "")
            {
                CitizenshipHint = "";
                citizenshipButton.Visibility = Visibility.Hidden;
                return;
            }

            string query = @"SELECT TOP 1 Name FROM Country WHERE Name LIKE @searchText";

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@searchText", $"{Citizenship}%");

                try
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string read = reader.GetString(0);
                            CitizenshipHint = Citizenship + read.Substring(Citizenship.Length);
                        }
                        else citizenshipButton.Visibility = Visibility.Hidden;
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (CitizenshipHint == Citizenship) citizenshipButton.Visibility = Visibility.Hidden;
        }

        public State Validation(bool showMessage = true)
        {
            if (gender == Gender.none && Namee == "" && Surname == "" && PersonalDay == "" && PersonalMonth == "" && PersonalYear == "" && Citizenship == "" && Passport == "" && PassportDay == "" && PassportMonth == "" && PassportYear == "") return State.empty;

            var mainWindow = (MainWindow)Application.Current.MainWindow;
            if (gender == Gender.none)
            {
                if (showMessage) mainWindow.GlobalMessage.Show("Вкажіть свою стать", 3);
                return State.unsuccessful;
            }

            if (Surname.Length == 0)
            {
                if (showMessage) mainWindow.GlobalMessage.Show("Введіть прізвище (так як вказано в паспорті)", 3);
                return State.unsuccessful;
            }

            if (Namee.Length == 0)
            {
                if (showMessage) mainWindow.GlobalMessage.Show("Введіть ім'я (так як вказано в паспорті)", 3);
                return State.unsuccessful;
            }

            if (PersonalDate == DateTime.MinValue)
            {
                if (showMessage) mainWindow.GlobalMessage.Show("Введіть дату народження", 3);
                return State.unsuccessful;
            }

            if (PersonalDate < DateTime.Now.AddYears(-130) || PersonalDate > DateTime.Now)
            {
                if (showMessage) mainWindow.GlobalMessage.Show("Введіть коректно дату народження", 3);
                return State.unsuccessful;
            }

            if (Citizenship.Length == 0)
            {
                if (showMessage) mainWindow.GlobalMessage.Show("Введіть громадянство", 3);
                return State.unsuccessful;
            }

            if (Citizenship != CitizenshipHint)
            {
                if (showMessage) mainWindow.GlobalMessage.Show("Введіть громадянство правильно", 3);
                return State.unsuccessful;
            }

            if (Passport.Length == 0)
            {
                if (showMessage) mainWindow.GlobalMessage.Show("Введіть номер закордонного паспорта", 3);
                return State.unsuccessful;
            }

            if (Passport.Length < 6 || Passport.Length > 12)
            {
                if (showMessage) mainWindow.GlobalMessage.Show("Номер паспорта може містити від 6 до 12 символів", 3);
                return State.unsuccessful;
            }

            if (PassportDate == DateTime.MinValue)
            {
                if (showMessage) mainWindow.GlobalMessage.Show("Введіть дату закінчення паспорта", 3);
                return State.unsuccessful;
            }

            if (PassportDate < DateTime.Now)
            {
                if (showMessage) mainWindow.GlobalMessage.Show("Паспорт недійсний", 3);
                return State.unsuccessful;
            }

            if (PassportDate > DateTime.Now.AddYears(15))
            {
                if (showMessage) mainWindow.GlobalMessage.Show("Введіть коректно дату народження", 3);
                return State.unsuccessful;
            }

            return State.successful;
        }


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

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
