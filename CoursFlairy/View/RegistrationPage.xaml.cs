using CoursFlairy.Data;
using CoursFlairy.Model.Enum;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using static CoursFlairy.ViewModel.ResourceColor;

namespace CoursFlairy.View
{
    /// <summary>
    /// Interaction logic for RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page, INotifyPropertyChanged
    {
        private Random random = new Random();
        private PageState _pageState = PageState.Registration;

        private string _country = "";
        private string _countryHint = "";
        private string _login = "";

        private TranslateTransform loginTransform = new TranslateTransform();
        private TranslateTransform registrationTransform = new TranslateTransform();
        private TranslateTransform bussinessTransform = new TranslateTransform();

        #region Властивості
        public string CountryHint
        {
            get { return _countryHint; }
            set
            {
                _countryHint = value;
                if (CountryHint == Country && Country != "")
                {
                    BorderAnimation(countryBorder, false);
                    countryButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    BorderAnimation(countryBorder);
                    countryButton.Visibility = Visibility.Visible;
                }
                OnPropertyChanged(nameof(CountryHint));
            }
        }

        public string Country
        {
            get { return _country; }
            set
            {
                _country = value;
                CountryHint = "";
                CountryUpdate();
                OnPropertyChanged(nameof(Country));
            }
        }

        private PageState pageState
        {
            get { return _pageState; }
            set
            {
                _pageState = value;
                if (value == PageState.BigInterface) ArrowAnimation(180, 0);
                else ArrowAnimation(0, 180);

                TextBlock_MouseLeaveAnimation(additionalOptionText, null);
            }
        }
        #endregion

        public RegistrationPage()
        {
            InitializeComponent();
            Loaded += RegistrationPage_Loaded;

            LogIn.RenderTransform = loginTransform;
            Registration.RenderTransform = registrationTransform;
            Bussiness.RenderTransform = bussinessTransform;
        }

        private async void RegistrationPage_Loaded(object sender, RoutedEventArgs e)
        {
            LogIn.ToBigInterface();
            UpdateCanvasSize();
            await CreateAnimatedEllipsesAsync();
        }
        #region Методи 
        #region Взаємодії
        #region MouseDown
        private void RegistrationUser_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            if (string.IsNullOrWhiteSpace(login.Text) || string.IsNullOrWhiteSpace(email.Text) || string.IsNullOrEmpty(password.Password) || string.IsNullOrEmpty(confirmPassword.Password))
            {
                mainWindow.GlobalMessage.Show("Заповніть всі поля", 3);
                ControlShakingAnimation(RegistrationButton);
                return;
            }

            if (login.Text.Count(c => char.IsLetter(c) && c < 128) < 2)
            {
                mainWindow.GlobalMessage.Show("Логін має складатися мінімум з 2 латинських літер", 3);
                ControlShakingAnimation(RegistrationButton);
                return;
            }

            if (!Regex.IsMatch(login.Text, loginPattern))
            {
                mainWindow.GlobalMessage.Show("Логін може містити тільки латинські літери, цифри та підкреслювання", 3);
                ControlShakingAnimation(RegistrationButton);
                return;
            }

            if (!CheckLoginAvailability(login.Text))
            {
                mainWindow.GlobalMessage.Show("Логін зайнятий", 3);
                ControlShakingAnimation(RegistrationButton);
                return;
            }

            if (!Regex.IsMatch(email.Text, emailPattern))
            {
                mainWindow.GlobalMessage.Show("Введіть коректно пошту", 3);
                ControlShakingAnimation(RegistrationButton);
                return;
            }

            if (!CheckEmailAvailability(email.Text))
            {
                mainWindow.GlobalMessage.Show("Ця пошта вже зареєстрована", 3);
                ControlShakingAnimation(RegistrationBussButton);
                return;
            }

            if (password.Password.Length < 8)
            {
                mainWindow.GlobalMessage.Show("Пароль має містити мінімум 8 символів", 3);
                ControlShakingAnimation(RegistrationButton);
                return;
            }

            if (password.Password != confirmPassword.Password)
            {
                mainWindow.GlobalMessage.Show("Паролі мають збігатися", 3);
                ControlShakingAnimation(RegistrationButton);
                return;
            }

            if (phone.Text.Length > 0 && !Regex.IsMatch(phone.Text, phonePattern))
            {
                mainWindow.GlobalMessage.Show("Введіть номер телефону коректно", 3);
                ControlShakingAnimation(RegistrationButton);
                return;
            }

            if (!CheckPhoneAvailability(phone.Text))
            {
                mainWindow.GlobalMessage.Show("Цей номер телефону вже зареєстрований", 3);
                ControlShakingAnimation(RegistrationButton);
                return;
            }

            if (UserPassport.Validation() == State.unsuccessful)
            {
                ControlShakingAnimation(RegistrationButton);
                return;
            }

            RegistrationProcces();
        }

        private void RegistrationBussUser_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            if (string.IsNullOrWhiteSpace(firmName.Text) || string.IsNullOrWhiteSpace(emailBuss.Text) || string.IsNullOrEmpty(Country) || string.IsNullOrEmpty(passwordBuss.Password) || string.IsNullOrEmpty(confirmBussPassword.Password))
            {
                mainWindow.GlobalMessage.Show("Заповніть всі поля", 3);
                ControlShakingAnimation(RegistrationBussButton);
                return;
            }

            if (!Regex.IsMatch(firmName.Text, loginPattern))
            {
                mainWindow.GlobalMessage.Show("Ім'я може містити тільки латинські літери, цифри та підкреслювання", 3);
                ControlShakingAnimation(RegistrationBussButton);
                return;
            }

            if (CountryHint != Country)
            {
                mainWindow.GlobalMessage.Show("Введіть коректно країну", 3);
                ControlShakingAnimation(RegistrationBussButton);
                return;
            }

            if (!Regex.IsMatch(emailBuss.Text, emailPattern))
            {
                mainWindow.GlobalMessage.Show("Введіть коректно пошту", 3);
                ControlShakingAnimation(RegistrationBussButton);
                return;
            }

            if (!CheckEmailAvailability(emailBuss.Text))
            {
                mainWindow.GlobalMessage.Show("Ця пошта вже зареєстрована", 3);
                ControlShakingAnimation(RegistrationBussButton);
                return;
            }

            if (passwordBuss.Password.Length < 8)
            {
                mainWindow.GlobalMessage.Show("Пароль має містити мінімум 8 символів", 3);
                ControlShakingAnimation(RegistrationBussButton);
                return;
            }

            if (passwordBuss.Password != confirmBussPassword.Password)
            {
                mainWindow.GlobalMessage.Show("Паролі мають збігатися", 3);
                ControlShakingAnimation(RegistrationBussButton);
                return;
            }

            RegistrationBussProcces();
        }

        private void SvgChoose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (icon.Visibility == Visibility.Visible)
            {
                icon.Visibility = Visibility.Collapsed;
                plus.Visibility = Visibility.Visible;
                BorderAnimation(logoBorder);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "SVG Files (*.svg)|*.svg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFilePath = openFileDialog.FileName;
                var projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
                var saveDirectory = System.IO.Path.Combine(projectDirectory, "Data", "Images");

                var savePath = System.IO.Path.Combine(saveDirectory, "TempIcon.svg");
                File.Copy(selectedFilePath, savePath, true);
                icon.Source = new Uri(savePath);
            }

            plus.Visibility = Visibility.Collapsed;
            icon.Visibility = Visibility.Visible;
            BorderAnimation(logoBorder, false);
        }

        private async void ToBigInterface_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await ToBigInterfaceAsync();
        }

        private async void ToBussiness_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await ToBussinessAsync();
        }

        private async void FromBussToLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FromBussToLogin();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            email.Focus();
            Keyboard.ClearFocus();
        }

        private void LogIn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ToLogin();
        }
        #endregion

        #region KeyControl
        private void KeyControl(object sender, KeyEventArgs e)
        {
            Dictionary<Control, Control> queue = new Dictionary<Control, Control>()
            {{ login, email }, { email, password }, { password, confirmPassword }, { confirmPassword, login } };

            if (e.Key == Key.Space || ((Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V)))
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Down || e.Key == Key.Enter)
            {
                if (queue.ContainsKey(sender as Control))
                {
                    queue[sender as Control].Focus();
                }

                e.Handled = true;
                return;
            }
            else if (e.Key == Key.Up)
            {
                var previousControl = queue.FirstOrDefault(x => x.Value == sender as Control).Key;
                if (previousControl != null)
                {
                    previousControl.Focus();
                }

                e.Handled = true;
                return;
            }
        }

        private void KeyBussControl(object sender, KeyEventArgs e)
        {
            Dictionary<Control, Control> queue = new Dictionary<Control, Control>()
            {{ firmName, country }, { country, emailBuss }, { emailBuss, passwordBuss }, { passwordBuss, confirmBussPassword }, { confirmBussPassword, firmName } };

            if ((e.Key == Key.Space && sender as TextBox != firmName && sender as TextBox != country) || ((Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V)))
            {
                e.Handled = true;
                return;
            }

            if (sender as TextBox == country && (e.Key == Key.Tab || e.Key == Key.Enter))
            {
                Country = CountryHint;
            }

            if (e.Key == Key.Down || e.Key == Key.Enter)
            {

                if (queue.ContainsKey(sender as Control))
                {
                    queue[sender as Control].Focus();
                }

                e.Handled = true;
                return;
            }
            else if (e.Key == Key.Up)
            {
                var previousControl = queue.FirstOrDefault(x => x.Value == sender as Control).Key;
                if (previousControl != null)
                {
                    previousControl.Focus();
                }

                e.Handled = true;
                return;
            }
        }
        #endregion

        #region TextChanged
        private void UserPassport_DataChanged(object sender, EventArgs e)
        {
            RegistrationButtonUpdate();

            if (UserPassport.Validation(false) == State.successful)
            {
                referralCode.IsEnabled = true;
            }
            else
            {
                referralCode.IsEnabled = false;
            }
        }

        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not PasswordBox passwordEl) return;

            UIElement hint = passwordEl.Name switch
            {
                "password" => PasswordHint,
                "confirmPassword" => ConfirmPasswordHint,
                "passwordBuss" => PasswordBussHint,
                _ => ConfirmPasswordBussHint
            };

            Border border = passwordEl.Name switch
            {
                "password" => passwordBorder,
                "confirmPassword" => confirmPasswordBorder,
                "passwordBuss" => passwordBussBorder,
                _ => confirmPasswordBussBorder
            };

            bool isValid = passwordEl.Name switch
            {
                "confirmPassword" => passwordEl.Password == password.Password,
                "confirmBussPassword" => passwordEl.Password == passwordBuss.Password,
                _ => passwordEl.Password.Length >= 8
            };

            hint.Visibility = passwordEl.Password.Length == 0 ? Visibility.Visible : Visibility.Hidden;
            BorderAnimation(border, !isValid);
        }

        private const string loginPattern = @"^[a-zA-Z0-9_ ]*$";
        private void login_TextChanged(object sender, TextChangedEventArgs e)
        {
            int letterCount = ((TextBox)sender).Text.Count(c => char.IsLetter(c) && c < 128);

            if (!CheckLoginAvailability(((TextBox)sender).Text))
            {
                dangerLogin.Opacity = 1;
                BorderAnimation(loginBorder);
                return;
            }
            else
            {
                dangerLogin.Opacity = 0;
            }

            if (letterCount > 1 && Regex.IsMatch(((TextBox)sender).Text, loginPattern))
            {
                BorderAnimation(loginBorder, false);
            }
            else
            {
                BorderAnimation(loginBorder);
            }
        }

        private void firmName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(firmName.Text) && Regex.IsMatch(((TextBox)sender).Text, loginPattern))
            {
                BorderAnimation(firmNameBorder, false);
            }
            else
            {
                BorderAnimation(firmNameBorder);
            }
        }
        #endregion

        #region TextInput
        private void phone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "+" && !((TextBox)sender).Text.Contains("+")) return;

            e.Handled = !int.TryParse(e.Text, out _);
        }
        #endregion

        #region Anime
        private void TextBlock_MouseEnterAnimation(object sender, MouseEventArgs e)
        {
            if (sender as TextBlock == plus)
            {
                ((TextBlock)sender).Foreground = HintColor;
                ((TextBlock)sender).Opacity = 0.8;
                return;
            }

            ((TextBlock)sender).Foreground = MainColor100;
            ((TextBlock)sender).Opacity = 0.8;
        }

        private void TextBlock_MouseLeaveAnimation(object sender, MouseEventArgs e)
        {
            if (((TextBlock)sender).Name == "additionalOptionText" && pageState == PageState.BigInterface)
            {
                ((TextBlock)sender).Foreground = MainColor100;
                ((TextBlock)sender).Opacity = 0.8;
                return;
            }

            if (sender as TextBlock == plus)
            {
                ((TextBlock)sender).Foreground = MainColor100;
                ((TextBlock)sender).Opacity = 1;
                return;
            }

            ((TextBlock)sender).Foreground = HintColor;
            ((TextBlock)sender).Opacity = 0.6;
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

            RegistrationButtonUpdate();
            RegistrationBussButtonUpdate();
        }

        private void RegistrationButtonUpdate()
        {
            bool Validation = (string.IsNullOrWhiteSpace(login.Text) || string.IsNullOrWhiteSpace(email.Text) || string.IsNullOrEmpty(password.Password) || string.IsNullOrEmpty(confirmPassword.Password)) || (login.Text.Count(c => char.IsLetter(c) && c < 128) < 2) || (!Regex.IsMatch(login.Text, loginPattern)) || (!CheckLoginAvailability(login.Text)) || (!Regex.IsMatch(email.Text, emailPattern)) || (!CheckEmailAvailability(email.Text)) || (password.Password.Length < 8) || (password.Password != confirmPassword.Password) || (phone.Text.Length > 0 && !Regex.IsMatch(phone.Text, phonePattern)) || (!CheckPhoneAvailability(phone.Text)) || (UserPassport.Validation() == State.unsuccessful);

            if (RegistrationButton.Background is not SolidColorBrush brush || brush.IsFrozen) RegistrationButton.Background = brush = new SolidColorBrush(MainColor20.Color);

            if (!Validation && UserPassport.Validation(false) != State.unsuccessful)
            {
                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor100.Color, TimeSpan.FromSeconds(0.15)));
            }
            else
                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor20.Color, TimeSpan.FromSeconds(0.15)));
        }

        private void RegistrationBussButtonUpdate()
        {
            bool Validation = (string.IsNullOrWhiteSpace(firmName.Text) || string.IsNullOrWhiteSpace(emailBuss.Text) || string.IsNullOrEmpty(Country) || string.IsNullOrEmpty(passwordBuss.Password) || string.IsNullOrEmpty(confirmBussPassword.Password)) || (!Regex.IsMatch(firmName.Text, loginPattern)) || (CountryHint != Country) || (!Regex.IsMatch(emailBuss.Text, emailPattern)) || (!CheckEmailAvailability(emailBuss.Text)) || (passwordBuss.Password.Length < 8) || (passwordBuss.Password != confirmBussPassword.Password);

            if (RegistrationBussButton.Background is not SolidColorBrush brush || brush.IsFrozen) RegistrationBussButton.Background = brush = new SolidColorBrush(MainColor20.Color);

            if (!Validation)
            {
                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor100.Color, TimeSpan.FromSeconds(0.15)));
            }
            else
                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor20.Color, TimeSpan.FromSeconds(0.15)));
        }

        private const string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        private void emailAnimation(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox email)
            {
                if (email.Name == "email")
                {
                    if (!CheckEmailAvailability(((TextBox)sender).Text))
                    {
                        dangerEmail.Opacity = 1;
                        BorderAnimation(emailBorder);
                        return;
                    }
                    else
                    {
                        dangerEmail.Opacity = 0;
                    }

                    bool isValid = Regex.IsMatch(email.Text, emailPattern);

                    if (isValid)
                    {
                        BorderAnimation(emailBorder, false);
                    }
                    else
                    {
                        BorderAnimation(emailBorder);
                    }
                }
                else
                {
                    if (!CheckEmailAvailability(((TextBox)sender).Text))
                    {
                        dangerBussEmail.Opacity = 1;
                        BorderAnimation(emailBussBorder);
                        return;
                    }
                    else
                    {
                        dangerBussEmail.Opacity = 0;
                    }

                    bool isValid = Regex.IsMatch(emailBuss.Text, emailPattern);

                    if (isValid)
                    {
                        BorderAnimation(emailBussBorder, false);
                    }
                    else
                    {
                        BorderAnimation(emailBussBorder);
                    }
                }
            }
        }

        private void referalAnimation(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox referral)
            {
                if (referral.Text.Length > 0)
                {
                    BorderAnimation(referralCodeBorder, false);
                }
                else
                {
                    BorderAnimation(referralCodeBorder);
                }
            }
        }

        private const string phonePattern = @"^\+?[1-9]\d{6,14}$";
        private void PhoneAnimation(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox phone)
            {
                if (!CheckPhoneAvailability(((TextBox)sender).Text))
                {
                    dangerPhone.Opacity = 1;
                    BorderAnimation(phoneBorder);
                    return;
                }
                else
                {
                    dangerPhone.Opacity = 0;
                }

                bool isValid = Regex.IsMatch(phone.Text, phonePattern);

                if (isValid)
                {
                    BorderAnimation(phoneBorder, false);
                }
                else
                {
                    BorderAnimation(phoneBorder);
                }
            }
        }

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
        #endregion
        #endregion

        #region Анімації між станами сторінки
        private void ArrowAnimation(int angle1, int angle2)
        {
            if (arrowa.RenderTransform is TransformGroup transformGroup)
            {
                var rotateTransform = transformGroup.Children.OfType<RotateTransform>().FirstOrDefault();

                if (rotateTransform != null)
                {
                    var animation = new DoubleAnimation
                    {
                        From = angle1,
                        To = angle2,
                        Duration = TimeSpan.FromSeconds(0.4),
                        AccelerationRatio = 0.3,
                        DecelerationRatio = 0.7
                    };

                    rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
                }
            }
        }

        public async void ToRegistration()
        {
            if (pageState == PageState.Registration) return;

            if (pageState == PageState.BigInterface) await ToBigInterfaceAsync();
            else AnimateTransition(PageState.Registration);

            pageState = PageState.Registration;
        }

        public async void ToLogin()
        {
            if (pageState == PageState.BigInterface) await ToBigInterfaceAsync();
            AnimateTransition(PageState.Login);

            pageState = PageState.Login;
        }

        public async void FromBussToLogin()
        {
            AnimateTransition(PageState.Bussiness);
            pageState = PageState.Registration;
        }

        private void AnimateTransition(PageState pageState)
        {
            double width = MainGrid.ActualWidth;
            double moveDistance = width * 0.5;
            double offScreenDistance = width;

            var easing = new ElasticEase { Oscillations = 2, Springiness = 9, EasingMode = EasingMode.EaseOut };

            if (pageState == PageState.Registration)
            {
                LogIn.Visibility = Visibility.Collapsed;
                Registration.Visibility = Visibility.Visible;
                loginTransform.X = 0;
                registrationTransform.X = -moveDistance;
                AnimateElement(loginTransform, 0, moveDistance, easing, () => { });
                AnimateElement(registrationTransform, -moveDistance, 0, easing, () => { });
            }
            else if (pageState == PageState.Login)
            {
                LogIn.Visibility = Visibility.Visible;
                Registration.Visibility = Visibility.Collapsed;
                loginTransform.X = moveDistance;
                registrationTransform.X = 0;
                AnimateElement(registrationTransform, 0, -moveDistance, easing, () => { });
                AnimateElement(loginTransform, moveDistance, 0, easing, () => { });
            }
            else
            {
                easing = new ElasticEase { Oscillations = 2, Springiness = 5, EasingMode = EasingMode.EaseOut };

                loginTransform.X = bussinessTransform.X + moveDistance / 2;
                AnimateElement(bussinessTransform, 0, bussinessTransform.X + moveDistance / 2, easing, () => { });

                MainGrid.ColumnDefinitions.Clear();
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.6, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.4, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.7, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) });

                AnimateElement(registrationTransform, moveDistance, 0, easing, () => { });
                AnimateElement(bussinessTransform, moveDistance, 0, easing, () => { });
            }
        }

        private void AnimateElement(TranslateTransform transform, double from, double to, IEasingFunction easing, Action onComplete)
        {
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(0.6),
                EasingFunction = easing
            };

            animation.Completed += (s, e) => onComplete?.Invoke();

            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private bool isAnimatingBigInterface = false;
        private async Task ToBigInterfaceAsync()
        {
            if (isAnimatingBigInterface) return;

            isAnimatingBigInterface = true;

            double[] timeForAnimation = { 0.08, 0.08, 0.15, 0.08, 0.08, 0.1, 0.1 };

            ColumnDefinition[] columns = { MainGrid.ColumnDefinitions[0], MainGrid.ColumnDefinitions[1], MainGrid.ColumnDefinitions[2], MainGrid.ColumnDefinitions[3], MainGrid.ColumnDefinitions[4], MainGrid.ColumnDefinitions[6] };

            Storyboard storyboard = new Storyboard();

            double[] currentTime = new double[timeForAnimation.Length];
            currentTime[0] = 0;
            for (int i = 1; i < timeForAnimation.Length; i++)
            {
                currentTime[i] = currentTime[i - 1] + timeForAnimation[i - 1];
            }

            if (pageState != PageState.BigInterface)
            {
                // Етап 1
                storyboard.Children.Add(AddAnimation(columns[2], 2.4, 2.3, currentTime[0], timeForAnimation[0]));
                storyboard.Children.Add(AddAnimation(columns[5], 0.7, 0.8, currentTime[0], timeForAnimation[0]));

                // Етап 2
                storyboard.Children.Add(AddAnimation(columns[2], 2.3, 2.5, currentTime[1], timeForAnimation[1]));
                storyboard.Children.Add(AddAnimation(columns[5], 0.8, 0.6, currentTime[1], timeForAnimation[1]));

                // Етап 3
                storyboard.Children.Add(AddAnimation(columns[0], 0.7, 1.0, currentTime[2], timeForAnimation[2]));
                storyboard.Children.Add(AddAnimation(columns[1], 1.6, 0, currentTime[2], timeForAnimation[2]));
                storyboard.Children.Add(AddAnimation(columns[2], 2.5, 0, currentTime[2], timeForAnimation[2]));
                storyboard.Children.Add(AddAnimation(columns[4], 0, 3, currentTime[2], timeForAnimation[2]));
                storyboard.Children.Add(AddAnimation(columns[5], 0.6, 1.4, currentTime[2], timeForAnimation[2]));

                // Етап 4
                storyboard.Children.Add(AddAnimation(columns[0], 1.0, 1.2, currentTime[3], timeForAnimation[3]));
                storyboard.Children.Add(AddAnimation(columns[5], 1.4, 1.0, currentTime[3], timeForAnimation[3]));

                // Етап 5
                storyboard.Children.Add(AddAnimation(columns[0], 1.2, 1.4, currentTime[4], timeForAnimation[4]));
                storyboard.Children.Add(AddAnimation(columns[5], 1.0, 1.1, currentTime[4], timeForAnimation[4]));

                // Етап 6
                storyboard.Children.Add(AddAnimation(columns[0], 1.4, 1.3, currentTime[5], timeForAnimation[5]));
                storyboard.Children.Add(AddAnimation(columns[5], 1.1, 1.2, currentTime[5], timeForAnimation[5]));

                // Етап 7
                storyboard.Children.Add(AddAnimation(columns[5], 1.2, 1.1, currentTime[6], timeForAnimation[6]));
                storyboard.Children.Add(AddAnimation(columns[0], 1.3, 1.2, currentTime[6], timeForAnimation[6]));
            }
            else
            {
                // Етап 7
                storyboard.Children.Add(AddAnimation(columns[0], 1.2, 1.3, currentTime[0], timeForAnimation[6]));
                storyboard.Children.Add(AddAnimation(columns[5], 1.1, 1.2, currentTime[0], timeForAnimation[6]));

                // Етап 6
                storyboard.Children.Add(AddAnimation(columns[5], 1.2, 1.1, currentTime[1], timeForAnimation[5]));
                storyboard.Children.Add(AddAnimation(columns[0], 1.3, 1.4, currentTime[1], timeForAnimation[5]));

                // Етап 5
                storyboard.Children.Add(AddAnimation(columns[5], 1.1, 1.0, currentTime[2], timeForAnimation[4]));
                storyboard.Children.Add(AddAnimation(columns[0], 1.4, 1.2, currentTime[2], timeForAnimation[4]));

                // Етап 4
                storyboard.Children.Add(AddAnimation(columns[5], 1.0, 1.4, currentTime[3], timeForAnimation[3]));
                storyboard.Children.Add(AddAnimation(columns[0], 1.2, 1.0, currentTime[3], timeForAnimation[3]));

                // Етап 3
                storyboard.Children.Add(AddAnimation(columns[5], 1.4, 0.6, currentTime[4], timeForAnimation[2]));
                storyboard.Children.Add(AddAnimation(columns[4], 3, 0, currentTime[4], timeForAnimation[2]));
                storyboard.Children.Add(AddAnimation(columns[2], 0, 2.5, currentTime[4], timeForAnimation[2]));
                storyboard.Children.Add(AddAnimation(columns[1], 0, 1.6, currentTime[4], timeForAnimation[2]));
                storyboard.Children.Add(AddAnimation(columns[0], 1.0, 0.7, currentTime[4], timeForAnimation[2]));

                // Етап 2
                storyboard.Children.Add(AddAnimation(columns[5], 0.6, 0.8, currentTime[5], timeForAnimation[1]));
                storyboard.Children.Add(AddAnimation(columns[2], 2.5, 2.3, currentTime[5], timeForAnimation[1]));

                // Етап 1
                storyboard.Children.Add(AddAnimation(columns[5], 0.8, 0.7, currentTime[6], timeForAnimation[0]));
                storyboard.Children.Add(AddAnimation(columns[2], 2.3, 2.4, currentTime[6], timeForAnimation[0]));
            }

            storyboard.Begin();
            if (pageState == PageState.BigInterface) pageState = PageState.Registration;
            else pageState = PageState.BigInterface;
            isAnimatingBigInterface = false;
            await Task.Delay(TimeSpan.FromSeconds(currentTime[6] + 0.15));
        }

        private bool isAnimatingBussiness = false;
        private async Task ToBussinessAsync()
        {
            if (isAnimatingBussiness) return;
            isAnimatingBussiness = true;

            double[] timeForAnimation = { 0.08, 0.08, 0.15, 0.08, 0.08, 0.1, 0.1 };
            ColumnDefinition[] columns = { MainGrid.ColumnDefinitions[0], MainGrid.ColumnDefinitions[1], MainGrid.ColumnDefinitions[2], MainGrid.ColumnDefinitions[3], MainGrid.ColumnDefinitions[4], MainGrid.ColumnDefinitions[6], MainGrid.ColumnDefinitions[5] };

            Storyboard storyboard = new Storyboard();

            double[] currentTime = new double[timeForAnimation.Length];
            currentTime[0] = 0;
            for (int i = 1; i < timeForAnimation.Length; i++)
            {
                currentTime[i] = currentTime[i - 1] + timeForAnimation[i - 1];
            }

            // Етап 7
            storyboard.Children.Add(AddAnimation(columns[0], 1.2, 1.3, currentTime[0], timeForAnimation[6]));
            storyboard.Children.Add(AddAnimation(columns[5], 1.1, 1.2, currentTime[0], timeForAnimation[6]));

            // Етап 6
            storyboard.Children.Add(AddAnimation(columns[5], 1.2, 1.1, currentTime[1], timeForAnimation[5]));
            storyboard.Children.Add(AddAnimation(columns[0], 1.3, 1.4, currentTime[1], timeForAnimation[5]));

            // Етап 5
            storyboard.Children.Add(AddAnimation(columns[5], 1.1, 1.0, currentTime[2], timeForAnimation[4]));
            storyboard.Children.Add(AddAnimation(columns[0], 1.4, 1.2, currentTime[2], timeForAnimation[4]));

            // Етап 4
            storyboard.Children.Add(AddAnimation(columns[5], 1.0, 1.4, currentTime[3], timeForAnimation[3]));
            storyboard.Children.Add(AddAnimation(columns[0], 1.2, 1.0, currentTime[3], timeForAnimation[3]));

            // Етап 3
            storyboard.Children.Add(AddAnimation(columns[5], 1.4, 0.6, currentTime[4], timeForAnimation[2]));
            storyboard.Children.Add(AddAnimation(columns[4], 3, 0, currentTime[4], timeForAnimation[2]));
            storyboard.Children.Add(AddAnimation(columns[2], 0, 2.5, currentTime[4], timeForAnimation[2]));
            storyboard.Children.Add(AddAnimation(columns[1], 0, 1.6, currentTime[4], timeForAnimation[2]));
            storyboard.Children.Add(AddAnimation(columns[0], 1.0, 0.7, currentTime[4], timeForAnimation[2]));
            storyboard.Children.Add(AddAnimation(columns[3], 1.7, 0, currentTime[4], timeForAnimation[2]));
            storyboard.Children.Add(AddAnimation(columns[6], 0, 1.7, currentTime[4], timeForAnimation[2]));

            // Етап 2
            storyboard.Children.Add(AddAnimation(columns[5], 0.6, 0.8, currentTime[5], timeForAnimation[1]));
            storyboard.Children.Add(AddAnimation(columns[2], 2.5, 2.3, currentTime[5], timeForAnimation[1]));

            // Етап 1
            storyboard.Children.Add(AddAnimation(columns[5], 0.8, 0.7, currentTime[6], timeForAnimation[0]));
            storyboard.Children.Add(AddAnimation(columns[2], 2.3, 2.4, currentTime[6], timeForAnimation[0]));

            storyboard.Begin();
            pageState = PageState.Bussiness;

            await Task.Delay(TimeSpan.FromSeconds(currentTime[6] + 0.15));
            isAnimatingBussiness = false;
            Bussiness.RenderTransform = bussinessTransform;
        }

        private GridLengthAnimation AddAnimation(ColumnDefinition column, double from, double to, double startTime, double duration)
        {
            var animation = new GridLengthAnimation
            {
                From = new GridLength(from, GridUnitType.Star),
                To = new GridLength(to, GridUnitType.Star),
                Duration = TimeSpan.FromSeconds(duration),
                BeginTime = TimeSpan.FromSeconds(startTime),
                FillBehavior = FillBehavior.HoldEnd
            };

            Storyboard.SetTarget(animation, column);
            Storyboard.SetTargetProperty(animation, new PropertyPath(ColumnDefinition.WidthProperty));
            return animation;
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
        #endregion

        #region Фон
        private void UpdateCanvasSize()
        {
            AnimationCanvas.Width = ActualWidth;
            AnimationCanvas.Height = ActualHeight;
        }

        private async Task CreateAnimatedEllipsesAsync()
        {
            int numberOfEllipses = 4;

            while (true)
            {
                if (AnimationCanvas.Children.Count < numberOfEllipses)
                {
                    double rowCount = Math.Ceiling(Math.Sqrt(numberOfEllipses));
                    double colCount = Math.Ceiling((double)numberOfEllipses / rowCount);

                    double horizontalSpacing = AnimationCanvas.Width / colCount;
                    double verticalSpacing = AnimationCanvas.Height / rowCount;

                    Ellipse ellipse = CreateEllipse(horizontalSpacing, verticalSpacing, colCount, rowCount);
                    AnimationCanvas.Children.Add(ellipse);
                    AnimateEllipse(ellipse);

                    _ = RemoveEllipseAfterTimeAsync(ellipse, 15000);
                }

                await Task.Delay(random.Next(4000, 17000));
            }
        }

        private async Task RemoveEllipseAfterTimeAsync(Ellipse ellipse, int lifetime)
        {
            await Task.Delay(lifetime - 2000);

            DoubleAnimation fadeOut = new DoubleAnimation(ellipse.Opacity, 0, TimeSpan.FromSeconds(2));
            ellipse.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            await Task.Delay(2000);

            AnimationCanvas.Dispatcher.Invoke(() =>
            {
                if (AnimationCanvas.Children.Contains(ellipse))
                {
                    AnimationCanvas.Children.Remove(ellipse);
                }
            });
        }

        private Ellipse CreateEllipse(double horizontalSpacing, double verticalSpacing, double colCount, double rowCount)
        {
            int radius = random.Next(150, 400);
            Ellipse ellipse = new Ellipse
            {
                Width = radius,
                Height = radius,
                Fill = new SolidColorBrush(Colors.White),
                //Fill = new SolidColorBrush(Color.FromArgb(100, (byte)random.Next(180, 256), (byte)random.Next(100, 200), (byte)random.Next(150, 255))),
                Opacity = 0,
                Effect = new BlurEffect { Radius = 30 }
            };

            double posX = random.NextDouble() * AnimationCanvas.Width;
            double posY = random.NextDouble() * AnimationCanvas.Height;

            Canvas.SetLeft(ellipse, posX);
            Canvas.SetTop(ellipse, posY);

            return ellipse;
        }

        private void AnimateEllipse(Ellipse ellipse)
        {
            DoubleAnimation fadeIn = new DoubleAnimation(0, 0.1, TimeSpan.FromSeconds(2));
            ellipse.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            double fromX = Canvas.GetLeft(ellipse);
            double toX = random.NextDouble() * (AnimationCanvas.Width - ellipse.Width);
            DoubleAnimation animX = new DoubleAnimation(fromX, toX, TimeSpan.FromSeconds(40))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            ellipse.BeginAnimation(Canvas.LeftProperty, animX);

            double fromY = Canvas.GetTop(ellipse);
            double toY = random.NextDouble() * (AnimationCanvas.Height - ellipse.Height);
            DoubleAnimation animY = new DoubleAnimation(fromY, toY, TimeSpan.FromSeconds(40))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            ellipse.BeginAnimation(Canvas.TopProperty, animY);

            ScaleTransform scaleTransform = new ScaleTransform(1, 1, ellipse.Width / 2, ellipse.Height / 2);
            ellipse.RenderTransform = scaleTransform;

            DoubleAnimation scaleAnim = new DoubleAnimation(1, 0.8, TimeSpan.FromSeconds(25))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }
        #endregion

        #region DataBase

        private void RegistrationProcces()
        {
            string query = @"INSERT INTO [dbo].[User] ([Login], [Email], [PasswordHash], [Phone]) VALUES (@login, @email, @password, @phone)";
            State state = UserPassport.Validation(false);
            if (state == State.successful)
            {
                //Реферал не підключений
                query = @"INSERT INTO [dbo].[User] ([Login], [Name], [Surname], [Gender] , [Citizenship], [BirthDate], [Passport], [PassportDate], [Email], [Phone], [PasswordHash]) VALUES (@login, @name, @surname, @gender, @citizenship, @birthDate, @passport, @passportDate, @email, @phone, @password)";
            }

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@login", login.Text);
                command.Parameters.AddWithValue("@email", email.Text);
                command.Parameters.AddWithValue("@password", GetPasswordHash(password.Password));
                if (state == State.successful) command.Parameters.AddWithValue("@name", UserPassport.Namee);
                if (state == State.successful) command.Parameters.AddWithValue("@surname", UserPassport.Surname);
                if (state == State.successful) command.Parameters.AddWithValue("@gender", (int)UserPassport.gender);
                if (state == State.successful) command.Parameters.AddWithValue("@citizenship", FindCountryId(UserPassport.Citizenship));
                if (state == State.successful) command.Parameters.AddWithValue("@birthDate", UserPassport.PersonalDate);
                if (state == State.successful) command.Parameters.AddWithValue("@passport", UserPassport.Passport);
                if (state == State.successful) command.Parameters.AddWithValue("@passportDate", UserPassport.PassportDate);

                if (string.IsNullOrWhiteSpace(phone.Text))
                {
                    command.Parameters.AddWithValue("@phone", DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@phone", phone.Text);
                }

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RegistrationBussProcces()
        {
            string logo = "";
            if (icon.Visibility == Visibility.Visible)
            {
                var projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
                var logoPath = System.IO.Path.Combine(projectDirectory, "Data", "Images", "TempIcon.svg");
                logo = File.ReadAllText(logoPath);
            }

            string query = @"INSERT INTO [dbo].[Airline] ([Name], [CountryID], [Email], [Logo], [PasswordHash]) VALUES (@firmName, @country, @email, @logo, @password)";
            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@firmName", firmName.Text);
                command.Parameters.AddWithValue("@country", FindCountryId(Country));
                command.Parameters.AddWithValue("@email", emailBuss.Text);
                command.Parameters.AddWithValue("@logo", logo);
                command.Parameters.AddWithValue("@password", GetPasswordHash(passwordBuss.Password));
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CheckEmailAvailability(string email)
        {
            string query = "SELECT dbo.check_email_availability(@Email)";

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@Email", email);
                return (bool)command.ExecuteScalar();
            }
        }

        private bool CheckPhoneAvailability(string phone)
        {
            string query = "SELECT dbo.check_phone_availability(@phone)";

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@phone", phone);
                return (bool)command.ExecuteScalar();
            }
        }

        private bool CheckLoginAvailability(string login)
        {
            string query = "SELECT dbo.check_login_availability(@login)";

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@login", login);
                return (bool)command.ExecuteScalar();
            }
        }

        private int FindCountryId(string countryName)
        {
            string query = @"SELECT TOP 1 ID FROM Country WHERE Name LIKE @searchText";
            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@searchText", $"{countryName}");

                try
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return -1;
        }

        private void CountryUpdate()
        {
            if (DataBase.GetConnection().State != System.Data.ConnectionState.Open)
                return;

            if (Country == "")
            {
                CountryHint = "";
                countryButton.Visibility = Visibility.Hidden;
                return;
            }

            string query = @"SELECT TOP 1 Name FROM Country WHERE Name LIKE @searchText";

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@searchText", $"{Country}%");

                try
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string read = reader.GetString(0);
                            CountryHint = Country + read.Substring(Country.Length);
                        }
                        else countryButton.Visibility = Visibility.Hidden;
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (CountryHint == Country) countryButton.Visibility = Visibility.Hidden;
        }

        public string GetPasswordHash(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
        #endregion
        #endregion

        enum PageState
        {
            Login,
            Registration,
            BigInterface,
            Bussiness
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class GridLengthAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(GridLength);

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register(nameof(From), typeof(GridLength), typeof(GridLengthAnimation));

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register(nameof(To), typeof(GridLength), typeof(GridLengthAnimation));

        public GridLength From
        {
            get => (GridLength)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public GridLength To
        {
            get => (GridLength)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (animationClock.CurrentProgress == null) return From;

            double fromValue = From.Value;
            double toValue = To.Value;
            double progress = animationClock.CurrentProgress.Value;

            return new GridLength(fromValue + (toValue - fromValue) * progress, GridUnitType.Star);
        }

        protected override Freezable CreateInstanceCore() => new GridLengthAnimation();
    }

}
