using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using CoursFlairy.View.UI;
using CoursFlairy.Data;
using CoursFlairy.Model.Enum;
using Microsoft.Data.SqlClient;
using System.Collections;
using CoursFlairy.Model;
using System.Data;
using System.Diagnostics;

namespace CoursFlairy.View.ClientPage
{
    /// <summary>
    /// Interaction logic for PaymentPage.xaml
    /// </summary>
    public partial class PaymentPage : Page, INotifyPropertyChanged
    {
        private string _cardNumber;
        private string _expiryDate;
        private string _cvv;
        private bool _isProcessing;
        private bool _isPaymentSuccessful;
        private decimal _amount;
        private ICommand _payCommand;
        private ICommand _returnHomeCommand;
        private bool isFormatting = false;
        public event EventHandler<bool> PaymentCompleted;

        public PaymentPage(double amount)
        {
            InitializeComponent();
            _amount = (decimal)amount;
            Amount = (decimal)amount;
            PayCommand = new RelayCommand(ProcessPayment, _ => CanProcessPayment());
            ReturnHomeCommand = new RelayCommand(_ => { ReturnHome(); return Task.CompletedTask; });
            DataContext = this;
        }

        public PaymentPage()
        {
            InitializeComponent();
            Amount = 0.0m;
            PayCommand = new RelayCommand(ProcessPayment, _ => CanProcessPayment());
            ReturnHomeCommand = new RelayCommand(_ => { ReturnHome(); return Task.CompletedTask; });
            DataContext = this;
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        public string CardNumber
        {
            get => _cardNumber;
            set
            {
                _cardNumber = value;
                OnPropertyChanged(nameof(CardNumber));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ExpiryDate
        {
            get => _expiryDate;
            set
            {
                _expiryDate = value;
                OnPropertyChanged(nameof(ExpiryDate));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Cvv
        {
            get => _cvv;
            set
            {
                _cvv = value;
                OnPropertyChanged(nameof(Cvv));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                _isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
            }
        }

        public bool IsPaymentSuccessful
        {
            get => _isPaymentSuccessful;
            set
            {
                _isPaymentSuccessful = value;
                OnPropertyChanged(nameof(IsPaymentSuccessful));
            }
        }

        public ICommand PayCommand
        {
            get => _payCommand;
            set
            {
                _payCommand = value;
                OnPropertyChanged(nameof(PayCommand));
            }
        }

        public ICommand ReturnHomeCommand
        {
            get => _returnHomeCommand;
            set
            {
                _returnHomeCommand = value;
                OnPropertyChanged(nameof(ReturnHomeCommand));
            }
        }

        private bool CanProcessPayment()
        {
            // Перевіряємо номер картки (без пробілів, має бути точно 16 цифр)
            var cardNumberWithoutSpaces = CardNumber?.Replace(" ", "") ?? "";
            if (string.IsNullOrWhiteSpace(cardNumberWithoutSpaces) || cardNumberWithoutSpaces.Length != 16)
                return false;

            if (string.IsNullOrWhiteSpace(ExpiryDate) || !IsValidExpiryDate(ExpiryDate))
                return false;

            if (string.IsNullOrWhiteSpace(Cvv) || Cvv.Length != 3)
                return false;

            return true;
        }

        private async Task ProcessPayment(object parameter)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            try
            {
                // Показуємо процес обробки
                IsProcessing = true;
                IsPaymentSuccessful = false;
                CommandManager.InvalidateRequerySuggested();

                // Імітація обробки платежу
                await Task.Delay(2000);

                // Показуємо успішне підтвердження
                IsProcessing = false;
                IsPaymentSuccessful = true;
                CommandManager.InvalidateRequerySuggested();

                // Чекаємо ще 3 секунди перед поверненням на головну
                await Task.Delay(3000);

                var parent = FindParent<SelectPage>(this);
                List<int> ticketId = new List<int>();
                for (int i = 0; i < parent.Clients.Count; i++)
                {
                    int id = AddNewClient(parent.Clients[i], parent.Email);
                    int ticket = AddTicket(id, parent.flightId, parent.SelectedSeats[i], parent.SelectedSeatsCode[i], parent.passengerClasses[i], parent.Prices[i]);
                    ticketId.Add(ticket);
                }
                parent.TicketId = ticketId;

                // Додаємо бали користувачу якщо він авторизований
                if (CurrentAccount.id != -1 && CurrentAccount.accountType == AccountType.User)
                {
                    try
                    {
                        // Рахуємо 1% від загальної суми
                        decimal pointsToAdd = Math.Floor(Amount * 0.01m);
                        
                        string updatePointsQuery = @"
                            UPDATE [User] 
                            SET Points = ISNULL(Points, 0) + @PointsToAdd 
                            WHERE ID = @UserId;
                            
                            SELECT Points 
                            FROM [User] 
                            WHERE ID = @UserId;";

                        using (SqlCommand command = new SqlCommand(updatePointsQuery, DataBase.GetConnection()))
                        {
                            command.Parameters.AddWithValue("@UserId", CurrentAccount.id);
                            command.Parameters.AddWithValue("@PointsToAdd", pointsToAdd);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    int totalPoints = reader.GetInt32(0);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mainWindow?.GlobalMessage.Show($"Помилка при нарахуванні балів: {ex.Message}", 3);
                    }
                }

                parent.FillPath(6);
                parent.CurentPage = 6;
                parent.PageManager.Navigate(new TicketPage(parent.TicketId));

                // Simulate successful payment
                PaymentCompleted?.Invoke(this, true);
                
                mainWindow?.GlobalMessage.Show("Оплата пройшла успішно!", 3);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при обробці платежу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                IsProcessing = false;
                IsPaymentSuccessful = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private int AddTicket(int clientId, int flightId, string seat, string seatCode, Classes classe, decimal price)
        {
            try
            {
                var connection = DataBase.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Ticket (ClientID, FlightID, SeatCode, Seat, Class, Price, Baggage)
                    VALUES (@ClientID, @FlightID, @SeatCode, @Seat, @Class, @Price, @Baggage);
                    SELECT SCOPE_IDENTITY();";

                command.Parameters.AddWithValue("@ClientID", clientId);
                command.Parameters.AddWithValue("@FlightID", flightId);
                command.Parameters.AddWithValue("@SeatCode", seatCode);
                command.Parameters.AddWithValue("@Seat", seat);
                command.Parameters.AddWithValue("@Class", GetIDFromClases(classe));
                command.Parameters.AddWithValue("@Price", price);

                var parent = FindParent<SelectPage>(this);
                int index = parent.SelectedSeats.IndexOf(seat);
                command.Parameters.AddWithValue("@Baggage", parent.HasBaggage[index]);

                var result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні квитка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        private int AddNewClient(PassportInfo passportInfo, string email)
        {
            try
            {
                string query = @"
            INSERT INTO [dbo].[Client] (
                [Name], 
                [Surname], 
                [Citizenship], 
                [Passport], 
                [PassportDate], 
                [Email], 
                [Gender], 
                [BirthDate]
            ) 
            VALUES (
                @Name, 
                @Surname, 
                @Citizenship, 
                @Passport, 
                @PassportDate, 
                @Email, 
                @Gender, 
                @BirthDate
            );
            SELECT SCOPE_IDENTITY();";

                using (SqlCommand command = new SqlCommand(query, DataBase.GetConnection()))
                {
                    command.Parameters.AddWithValue("@Surname", passportInfo.firstName);
                    command.Parameters.AddWithValue("@Name", passportInfo.lastName);
                    command.Parameters.AddWithValue("@Citizenship", passportInfo.CitizentshipID);
                    command.Parameters.AddWithValue("@Passport", passportInfo.passportNumber);
                    command.Parameters.AddWithValue("@PassportDate", passportInfo.passportDate);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Gender", (int)passportInfo.gender);
                    command.Parameters.AddWithValue("@BirthDate", passportInfo.birthDate);

                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        private int GetIDFromClases(Classes classes)
        {
            switch (classes)
            {
                case Classes.Econom:
                    return 3;
                case Classes.Bussiness:
                    return 2;
                case Classes.First:
                    return 1;
                default:
                    return 0;
            }
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                child = VisualTreeHelper.GetParent(child);
                if (child is T parent)
                {
                    return parent;
                }
            }
            return null;
        }

        private void ReturnHome()
        {
            while (NavigationService?.CanGoBack == true)
            {
                //NavigationService.GoBack();
            }
        }

        private bool IsValidExpiryDate(string expiryDate)
        {
            if (!Regex.IsMatch(expiryDate, @"^\d{2}/\d{2}$"))
                return false;

            var parts = expiryDate.Split('/');
            if (parts.Length != 2)
                return false;

            if (!int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
                return false;

            if (month < 1 || month > 12)
                return false;

            var currentYear = DateTime.Now.Year % 100;
            var currentMonth = DateTime.Now.Month;

            if (year < currentYear || (year == currentYear && month < currentMonth))
                return false;

            return true;
        }

        private void CardNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isFormatting) return;

            var textBox = (TextBox)sender;
            var cursorPosition = textBox.CaretIndex;
            var currentText = textBox.Text;
            
            // Видаляємо всі пробіли та залишаємо тільки цифри
            var digitsOnly = new string(currentText.Where(char.IsDigit).ToArray());
            
            // Обмежуємо до 16 цифр
            if (digitsOnly.Length > 16)
            {
                digitsOnly = digitsOnly.Substring(0, 16);
            }

            // Форматуємо у групи по 4 цифри
            var formattedText = "";
            for (int i = 0; i < digitsOnly.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                {
                    formattedText += " ";
                }
                formattedText += digitsOnly[i];
            }

            // Якщо форматований текст не змінився, не робимо нічого
            if (currentText == formattedText)
            {
                CardNumber = digitsOnly;
                return;
            }

            // Рахуємо позицію курсора на основі кількості цифр
            int digitsBeforeCursor = 0;
            for (int i = 0; i < Math.Min(cursorPosition, currentText.Length); i++)
            {
                if (char.IsDigit(currentText[i]))
                {
                    digitsBeforeCursor++;
                }
            }
            
            // Знаходимо нову позицію курсора у форматованому тексті
            int newCursorPosition = 0;
            int digitCount = 0;
            for (int i = 0; i < formattedText.Length; i++)
            {
                if (char.IsDigit(formattedText[i]))
                {
                    digitCount++;
                    if (digitCount > digitsBeforeCursor)
                    {
                        newCursorPosition = i;
                        break;
                    }
                }
                if (digitCount == digitsBeforeCursor)
                {
                    newCursorPosition = i + 1;
                }
            }
            
            // Якщо курсор був в самому кінці, ставимо його в кінець
            if (cursorPosition >= currentText.Length)
            {
                newCursorPosition = formattedText.Length;
            }

            // Встановлюємо прапорець форматування
            isFormatting = true;
            
            // Оновлюємо текст і позицію курсора
            textBox.Text = formattedText;
            textBox.CaretIndex = Math.Min(newCursorPosition, formattedText.Length);
            
            // Знімаємо прапорець форматування  
            isFormatting = false;

            // Оновлюємо властивість (тільки цифри для валідації)
            CardNumber = digitsOnly;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Заборонити пробіли у всіх полях
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter)
            {
                var textBox = (TextBox)sender;

                // Визначаємо наступний елемент для фокусу
                if (textBox == CardNumberTextBox)
                {
                    ExpiryDateTextBox.Focus();
                    e.Handled = true;
                }
                else if (textBox == ExpiryDateTextBox)
                {
                    CvvTextBox.Focus();
                    e.Handled = true;
                }
                else if (textBox == CvvTextBox && CanProcessPayment())
                {
                    PayCommand.Execute(null);
                    e.Handled = true;
                }
            }

            // Для поля терміну дії
            var currentTextBox = (TextBox)sender;
            if (currentTextBox == ExpiryDateTextBox)
            {
                // Якщо натиснуто Backspace і курсор стоїть після "/", видаляємо також "/"
                if (e.Key == Key.Back && currentTextBox.CaretIndex == 3 && currentTextBox.Text.Contains("/"))
                {
                    currentTextBox.Text = currentTextBox.Text.Substring(0, 2);
                    currentTextBox.CaretIndex = 2;
                    e.Handled = true;
                }
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Заборонити пробіли та будь-які не-цифрові символи
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text) || e.Text == " ";
        }

        private void CardNumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var currentTextWithoutSpaces = textBox.Text.Replace(" ", "");
            
            // Заборонити пробіли та будь-які не-цифрові символи
            if (!new Regex("[0-9]").IsMatch(e.Text) || e.Text == " ")
            {
                e.Handled = true;
                return;
            }

            // Обмежити до 16 цифр максимум
            if (currentTextWithoutSpaces.Length >= 16)
            {
                e.Handled = true;
                return;
            }

            e.Handled = false;
        }

        private void ExpiryDateValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var futureText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            // Заборонити пробіли та перевірити, чи введено число
            if (!new Regex("[0-9]").IsMatch(e.Text) || e.Text == " ")
            {
                e.Handled = true;
                return;
            }

            // Перевіряємо максимальну довжину
            if (futureText.Length > 5)
            {
                e.Handled = true;
                return;
            }

            // Автоматично додаємо "/"
            if (textBox.Text.Length == 2 && !textBox.Text.Contains("/"))
            {
                textBox.Text += "/";
                textBox.CaretIndex = textBox.Text.Length;
            }

            e.Handled = false;
        }

        private void CvvValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            
            // Заборонити пробіли та будь-які не-цифрові символи
            if (!new Regex("[0-9]").IsMatch(e.Text) || e.Text == " ")
            {
                e.Handled = true;
                return;
            }

            // Обмежити до 3 цифр для CVV
            if (textBox.Text.Length >= 3)
            {
                e.Handled = true;
                return;
            }

            e.Handled = false;
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<object, Task> _execute;
        private readonly Predicate<object> _canExecute;
        private bool _isExecuting;

        public RelayCommand(Func<object, Task> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute(parameter));
        }

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            try
            {
                await _execute(parameter);
            }
            finally
            {
                _isExecuting = false;
            }
            CommandManager.InvalidateRequerySuggested();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
