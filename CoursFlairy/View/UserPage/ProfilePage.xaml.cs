using System;
using System.Windows;
using System.Windows.Controls;
using CoursFlairy.Data;
using CoursFlairy.Model;
using Microsoft.Data.SqlClient;
using CoursFlairy.Model.Enum;
using System.Text.RegularExpressions;

namespace CoursFlairy.View.UserPage
{
    public partial class ProfilePage : Page
    {
        private PassportInfo _originalData;
        private bool _isEditingPassport = false;
        private bool _isEditingEmail = false;
        private string _currentEmail;
        private string _currentReferralCode;
        private const string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        public ProfilePage()
        {
            InitializeComponent();
            LoadUserData();
            LoadReferralData();
            SetEditingState(false);
            SetEmailEditingState(false);
        }

        private void LoadUserData()
        {
            try
            {
                if (DataBase.GetConnection().State != System.Data.ConnectionState.Open)
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow.GlobalMessage.Show("Помилка підключення до бази даних");
                    return;
                }
                int citizenship = -1;

                string query = @"SELECT Gender, Name, Surname, BirthDate, Passport, PassportDate, Citizenship, Email 
                               FROM [User] WHERE ID = @UserId";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@UserId", CurrentAccount.id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Gender gender = Gender.none;
                            string name = "";
                            string surname = "";
                            DateTime birthDate = DateTime.MinValue;
                            string passport = "";
                            DateTime passportDate = DateTime.MinValue;
                            string email = "";

                            if (!reader.IsDBNull(0)) gender = (Gender)reader.GetInt32(0);
                            if (!reader.IsDBNull(1)) name = reader.GetString(1);
                            if (!reader.IsDBNull(2)) surname = reader.GetString(2);
                            if (!reader.IsDBNull(3)) birthDate = reader.GetDateTime(3);
                            if (!reader.IsDBNull(4)) passport = reader.GetString(4);
                            if (!reader.IsDBNull(5)) passportDate = reader.GetDateTime(5);
                            if (!reader.IsDBNull(6)) citizenship = reader.GetInt32(6);
                            if (!reader.IsDBNull(7)) email = reader.GetString(7);

                            _originalData = new PassportInfo(
                                gender,
                                name,
                                surname,
                                birthDate,
                                passport,
                                passportDate,
                                citizenship
                            );

                            _currentEmail = email;
                            emailTextBox.Text = email;

                            // Set the data to the UserPassportData control
                            userPassportData.gender = _originalData.gender;
                            userPassportData.UpdateColor();
                            userPassportData.Namee = _originalData.firstName;
                            userPassportData.Surname = _originalData.lastName;

                            if (birthDate != DateTime.MinValue)
                            {
                                userPassportData.PersonalDay = _originalData.birthDate.Day.ToString("00");
                                userPassportData.PersonalMonth = _originalData.birthDate.Month.ToString("00");
                                userPassportData.PersonalYear = _originalData.birthDate.Year.ToString();
                            }

                            userPassportData.Passport = _originalData.passportNumber;

                            if (passportDate != DateTime.MinValue)
                            {
                                userPassportData.PassportDay = _originalData.passportDate.Day.ToString("00");
                                userPassportData.PassportMonth = _originalData.passportDate.Month.ToString("00");
                                userPassportData.PassportYear = _originalData.passportDate.Year.ToString();
                            }
                        }
                    }
                }

                if (citizenship != -1)
                {
                    LoadCitizenship(_originalData.CitizentshipID);
                }
            }
            catch (Exception ex)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show($"Помилка завантаження даних: {ex.Message}");
            }
        }

        private void LoadCitizenship(int citizenshipId)
        {
            try
            {
                string query = "SELECT Name FROM Country WHERE ID = @CitizenshipId";
                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@CitizenshipId", citizenshipId);
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        userPassportData.Citizenship = result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження громадянства: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetEditingState(bool isEditing)
        {
            _isEditingPassport = isEditing;
            editButton.Visibility = isEditing ? Visibility.Collapsed : Visibility.Visible;
            saveButton.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;
            cancelButton.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;

            // Enable/disable UserPassportData controls
            userPassportData.IsEnabled = isEditing;
        }

        private void SetEmailEditingState(bool isEditing)
        {
            _isEditingEmail = isEditing;
            editEmailButton.Visibility = isEditing ? Visibility.Collapsed : Visibility.Visible;
            saveEmailButton.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;
            cancelEmailButton.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;
            emailTextBox.IsEnabled = isEditing;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            SetEditingState(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (userPassportData.Validation() == State.successful)
            {
                SaveUserData();
                SetEditingState(false);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Restore original data
            LoadUserData();
            SetEditingState(false);
        }

        private void SaveUserData()
        {
            try
            {
                var passportInfo = userPassportData.GetPassportInfo();
                if (passportInfo == null) return;

                string query = @"UPDATE [User] 
                               SET Gender = @Gender,
                                   Name = @Name,
                                   Surname = @Surname,
                                   BirthDate = @BirthDate,
                                   Passport = @Passport,
                                   PassportDate = @PassportDate,
                                   Citizenship = @Citizenship
                               WHERE ID = @UserId";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@UserId", CurrentAccount.id);
                    command.Parameters.AddWithValue("@Gender", (int)passportInfo.gender);
                    command.Parameters.AddWithValue("@Name", passportInfo.firstName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Surname", passportInfo.lastName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@BirthDate", passportInfo.birthDate != DateTime.MinValue ? (object)passportInfo.birthDate : DBNull.Value);
                    command.Parameters.AddWithValue("@Passport", passportInfo.passportNumber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PassportDate", passportInfo.passportDate != DateTime.MinValue ? (object)passportInfo.passportDate : DBNull.Value);
                    command.Parameters.AddWithValue("@Citizenship", passportInfo.CitizentshipID > 0 ? (object)passportInfo.CitizentshipID : DBNull.Value);

                    command.ExecuteNonQuery();
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow.GlobalMessage.Show("Дані успішно оновлено");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void SaveEmailData()
        {
            SqlConnection connection = null;
            try
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                connection = DataBase.GetConnection();

                // Update email in User table only
                string updateUserQuery = @"UPDATE [User] 
                                         SET Email = @Email
                                         WHERE ID = @UserId";

                using (SqlCommand command = new SqlCommand(updateUserQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", CurrentAccount.id);
                    command.Parameters.AddWithValue("@Email", emailTextBox.Text);
                    command.ExecuteNonQuery();

                    _currentEmail = emailTextBox.Text;
                    mainWindow.GlobalMessage.Show("Email успішно оновлено");
                }
            }
            catch (Exception ex)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show($"Помилка оновлення email: {ex.Message}");
            }
            finally
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void EditEmailButton_Click(object sender, RoutedEventArgs e)
        {
            SetEmailEditingState(true);
        }

        private void SaveEmailButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            // Validate email
            if (string.IsNullOrWhiteSpace(emailTextBox.Text))
            {
                mainWindow.GlobalMessage.Show("Будь ласка, введіть email");
                return;
            }

            if (!Regex.IsMatch(emailTextBox.Text, emailPattern))
            {
                mainWindow.GlobalMessage.Show("Будь ласка, введіть коректний email");
                return;
            }

            // Check if email has changed
            if (emailTextBox.Text != _currentEmail)
            {
                // Check if new email is available
                if (!CheckEmailAvailability(emailTextBox.Text))
                {
                    mainWindow.GlobalMessage.Show("Цей email вже використовується");
                    return;
                }

                bool? result = mainWindow.GlobalMessage.ShowConfirm("Ви впевнені, що хочете змінити Email?");
                if (result == true)
                {
                    SaveEmailData();
                    SetEmailEditingState(false);
                }
                else
                {
                    emailTextBox.Text = _currentEmail;
                    SetEmailEditingState(false);
                }
            }
            else
            {
                SetEmailEditingState(false);
            }
        }

        private void CancelEmailButton_Click(object sender, RoutedEventArgs e)
        {
            emailTextBox.Text = _currentEmail;
            SetEmailEditingState(false);
        }

        private bool CheckEmailAvailability(string email)
        {
            SqlConnection connection = null;
            try
            {
                connection = DataBase.GetConnection();
                string query = "SELECT COUNT(*) FROM [User] WHERE Email = @Email AND ID != @UserId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@UserId", CurrentAccount.id);
                    int count = (int)command.ExecuteScalar();
                    return count == 0;
                }
            }
            catch (Exception ex)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.GlobalMessage.Show($"Помилка перевірки email: {ex.Message}");
                return false;
            }
            finally
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void LoadReferralData()
        {
            try
            {
                // Генеруємо реферальний код на основі ID користувача (псевдогенерація)
                _currentReferralCode = ReferralSystem.GetUserReferralCode(CurrentAccount.id);
                
                referralCodeTextBox.Text = _currentReferralCode;
                generateReferralButton.Visibility = Visibility.Collapsed; // Приховуємо кнопку генерації, бо код завжди є

                // Завантажуємо статистику рефералів
                int referralCount = ReferralSystem.GetReferralCount(CurrentAccount.id);
                referralStatsText.Text = $"Кількість запрошених: {referralCount}";
            }
            catch (Exception ex)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.GlobalMessage.Show($"Помилка завантаження реферальних даних: {ex.Message}");
                
                referralCodeTextBox.Text = "Помилка завантаження";
                generateReferralButton.Visibility = Visibility.Visible;
            }
        }

        private void GenerateReferralButton_Click(object sender, RoutedEventArgs e)
        {
            // Цей метод більше не потрібен, оскільки код генерується автоматично
            // Залишаємо для сумісності, але просто перезавантажуємо дані
            LoadReferralData();
            
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.GlobalMessage.Show("Реферальний код завжди доступний для вашого акаунту!", 2);
        }

        private void CopyReferralButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentReferralCode))
                {
                    Clipboard.SetText(_currentReferralCode);
                    
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.GlobalMessage.Show("Реферальний код скопійовано в буфер обміну!", 2);
                }
            }
            catch (Exception ex)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.GlobalMessage.Show($"Помилка копіювання: {ex.Message}", 3);
            }
        }
    }
}