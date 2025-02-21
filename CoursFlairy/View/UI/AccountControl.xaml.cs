using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using Microsoft.Data.SqlClient;
using static CoursFlairy.ViewModel.ResourceColor;
using System.Windows.Media.Animation;
using System.Windows.Media;
using CoursFlairy.Data;
using System.Text.RegularExpressions;
using CoursFlairy.ViewModel;
using System.Text;
using System.Security.Cryptography;

namespace CoursFlairy.View.UI
{
    /// <summary>
    /// Interaction logic for AccountControl.xaml
    /// </summary>
    public partial class AccountControl : UserControl
    {
        public AccountControl()
        {
            InitializeComponent();
        }

        public event EventHandler RegistonUser_MouseDown;

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

        #region Вхід
        private void LoginProcces()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            string? forquery = null;

            if (Regex.IsMatch(login.Text, emailPattern))
            {
                if (!CheckEmailAvailability(login.Text))
                {
                    forquery = "Email";
                }
                else
                {
                    mainWindow.GlobalMessage.Show("Даний email не зареєстрований", 3);
                    BorderShackingAnimation(SearchButton);
                    return;
                }
            }

            if (String.IsNullOrWhiteSpace(forquery) && Regex.IsMatch(login.Text, phonePattern))
            {
                if (!CheckPhoneAvailability(login.Text))
                {
                    forquery = "Phone";
                }
                else
                {
                    mainWindow.GlobalMessage.Show("Даний телефон не зареєстрований", 3);
                    BorderShackingAnimation(SearchButton);
                    return;
                }
            }

            if (String.IsNullOrWhiteSpace(forquery))
            {
                if (!CheckLoginAvailability(login.Text))
                    forquery = "Login";
                else
                {
                    mainWindow.GlobalMessage.Show("Користувача не знайдено", 3);
                    BorderShackingAnimation(SearchButton);
                    return;
                }
            }

            string query = $"SELECT 1 FROM [dbo].[User] WHERE {forquery} LIKE @login AND PasswordHash = @password";
            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@login", login.Text);
                command.Parameters.AddWithValue("@password", GetPasswordHash(password.Password));

                try
                {
                    var result = command.ExecuteScalar();

                    if (result != null)
                    {
                        mainWindow.GlobalMessage.Show("Успішний вхід", 3);
                    }
                    else
                    {
                        mainWindow.GlobalMessage.Show("Невірний логін або пароль", 3);
                        BorderShackingAnimation(SearchButton);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void ToBigInterface()
        {
            LoginGrid.RowDefinitions.Clear();
            LoginGrid.ColumnDefinitions.Clear();
            MainGrid.RowDefinitions.Clear();

            int[] rowHeights = { 12, 25, 16, 11, 4, 20, 8, 11, 4, 20, 3, 8, 70, 18, 3, 7, 15 };
            int[] columnWidht = { 1, 8, 1 };

            foreach (int height in rowHeights)
            {
                LoginGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(height, GridUnitType.Star) });
            }

            foreach (int width in columnWidht)
            {
                LoginGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width, GridUnitType.Star) });
            }
        }

        private void RegistrationUser_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow.PageManager == null)
                throw new InvalidOperationException("frame not found");

            if (mainWindow.PageManager.Content is not RegistrationPage)
                mainWindow.PageManager.Navigate(new RegistrationPage());
            else ((RegistrationPage)mainWindow.PageManager.Content).ToRegistration();

            RegistonUser_MouseDown?.Invoke(this, EventArgs.Empty);
        }

        private void LoginUser_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            string loginText = login.Text;
            string passwordText = password.Password;

            if (string.IsNullOrWhiteSpace(loginText))
            {
                mainWindow.GlobalMessage.Show("Введіть логін, пошту або телефон", 3);
                BorderShackingAnimation(SearchButton);
                return;
            }

            if (!(Regex.IsMatch(loginText, loginPattern) && loginText.Count(c => char.IsLetter(c) && c < 128) >= 2 || Regex.IsMatch(loginText, emailPattern) || Regex.IsMatch(loginText, phonePattern)))
            {
                mainWindow.GlobalMessage.Show("Введіть коректно логін", 3);
                BorderShackingAnimation(SearchButton);
                return;
            }

            if (string.IsNullOrEmpty(passwordText))
            {
                mainWindow.GlobalMessage.Show("Введіть пароль", 3);
                BorderShackingAnimation(SearchButton);
                return;
            }

            if (passwordText.Length < 8)
            {
                mainWindow.GlobalMessage.Show("Пароль має містити мінімум 8 символів", 3);
                BorderShackingAnimation(SearchButton);
                return;
            }

            LoginProcces();
        }

        private void KeyControl(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Space || ((Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V)))
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up)
            {
                login.Focus();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Down)
            {
                password.Focus();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter)
            {
                if (password.Password.Length == 0)
                    password.Focus();
                else Keyboard.ClearFocus();
                return;
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

            SearchButtonUpdate();
        }

        private void password_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (((PasswordBox)sender).Password.Length == 0)
            {
                PasswordHint.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordHint.Visibility = Visibility.Hidden;
                if (((PasswordBox)sender).Password.Length >= 8)
                    BorderAnimation(passwordBorder, false);
                else BorderAnimation(passwordBorder);
            }
        }

        private const string loginPattern = @"^[a-zA-Z0-9_]*$";
        private void login_TextChanged(object sender, TextChangedEventArgs e)
        {
            int letterCount = ((TextBox)sender).Text.Count(c => char.IsLetter(c) && c < 128);


            if ((letterCount > 1 && Regex.IsMatch(((TextBox)sender).Text, loginPattern)) || Regex.IsMatch(((TextBox)sender).Text, emailPattern) || Regex.IsMatch(((TextBox)sender).Text, phonePattern))
            {
                BorderAnimation(loginBorder, false);
            }
            else
            {
                BorderAnimation(loginBorder);
            }
        }

        private void SearchButtonUpdate()
        {
            if (SearchButton.Background is not SolidColorBrush brush || brush.IsFrozen)
                SearchButton.Background = brush = new SolidColorBrush(MainColor20.Color);

            bool Valid = !((Regex.IsMatch(login.Text, loginPattern) && login.Text.Count(c => char.IsLetter(c) && c < 128) >= 2) || Regex.IsMatch(login.Text, emailPattern) || Regex.IsMatch(login.Text, phonePattern));

            if (password.Password.Length >= 8 && !Valid)
            {
                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor100.Color, TimeSpan.FromSeconds(0.15)));
            }
            else
                brush.BeginAnimation(SolidColorBrush.ColorProperty,
                    new ColorAnimation(MainColor20.Color, TimeSpan.FromSeconds(0.15)));
        }

        private void Remember_Checked(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
            {
                RememberText.Foreground = MainColor100;
                RememberText.Opacity = 0.7;
            }
            else
            {
                RememberText.Foreground = HintColor;
                RememberText.Opacity = 0.9;
            }
        }

        private void BorderShackingAnimation(Border border)
        {
            if (border == null) return;

            ThicknessAnimationUsingKeyFrames animation = new ThicknessAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(500),
                RepeatBehavior = new RepeatBehavior(1)
            };

            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(5, 0, -5, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(50))));
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(-5, 0, 5, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(4, 0, -4, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150))));
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(-4, 0, 4, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(2, 0, -2, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(250))));
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(-2, 0, 2, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(new Thickness(0, 0, 0, 0), KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(350))));

            border.BeginAnimation(Border.MarginProperty, animation);
        }

        private void RegistrationUser_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Foreground = MainColor100;
            ((TextBlock)sender).Opacity = 0.7;
        }

        private void RegistrationUser_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Foreground = HintColor;
            ((TextBlock)sender).Opacity = 0.8;
        }

        private const string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        private bool CheckEmailAvailability(string email)
        {
            string query = "SELECT dbo.check_email_availability(@Email)";

            using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
            {
                command.Parameters.AddWithValue("@Email", email);
                return (bool)command.ExecuteScalar();
            }
        }

        private const string phonePattern = @"^\+?[1-9]\d{6,14}$";
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
    }
}
