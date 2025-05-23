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
        private decimal _amount;
        private ICommand _payCommand;

        public PaymentPage(decimal amount)
        {
            InitializeComponent();
            Amount = amount;
            PayCommand = new RelayCommand(async _ => await ProcessPayment(), _ => CanProcessPayment());
            DataContext = this;
        }

        public PaymentPage()
        {
            InitializeComponent();
            Amount = 0.0m;
            PayCommand = new RelayCommand(async _ => await ProcessPayment(), _ => CanProcessPayment());
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

        public ICommand PayCommand
        {
            get => _payCommand;
            set
            {
                _payCommand = value;
                OnPropertyChanged(nameof(PayCommand));
            }
        }

        private bool CanProcessPayment()
        {
            if (string.IsNullOrWhiteSpace(CardNumber) || CardNumber.Length < 16)
                return false;

            if (string.IsNullOrWhiteSpace(ExpiryDate) || !IsValidExpiryDate(ExpiryDate))
                return false;

            if (string.IsNullOrWhiteSpace(Cvv) || Cvv.Length != 3)
                return false;

            return true;
        }

        private async Task ProcessPayment()
        {
            IsProcessing = true;

            try
            {
                // Симулюємо обробку платежу
                await Task.Delay(2000);

                // Переходимо на сторінку успішного платежу
                NavigationService?.Navigate(new ConfirmationPage(Amount));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Payment failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
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

        private void CardNumberTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox)sender;
            var text = textBox.Text.Replace(" ", "");
            
            if (text.Length > 0)
            {
                // Форматуємо текст групами по 4 цифри
                var formattedText = string.Empty;
                for (int i = 0; i < text.Length; i++)
                {
                    if (i > 0 && i % 4 == 0)
                    {
                        formattedText += " ";
                    }
                    formattedText += text[i];
                }

                // Зберігаємо позицію курсора
                int cursorPosition = textBox.CaretIndex;
                // Якщо ми додали пробіл перед поточною позицією, зсуваємо курсор
                int spacesBeforeCursor = formattedText.Substring(0, Math.Min(cursorPosition, formattedText.Length))
                                                    .Count(c => c == ' ');
                int originalSpacesBeforeCursor = textBox.Text.Substring(0, Math.Min(cursorPosition, textBox.Text.Length))
                                                        .Count(c => c == ' ');
                
                textBox.Text = formattedText;
                textBox.CaretIndex = Math.Min(cursorPosition + (spacesBeforeCursor - originalSpacesBeforeCursor), formattedText.Length);
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = (TextBox)sender;
                
                // Визначаємо наступний елемент для фокусу
                if (textBox == CardNumberTextBox)
                {
                    ExpiryTextBox.Focus();
                    e.Handled = true;
                }
                else if (textBox == ExpiryTextBox)
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
            if (currentTextBox == ExpiryTextBox)
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
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void ExpiryValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var futureText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            // Перевіряємо, чи введено число
            if (!new Regex("[0-9]").IsMatch(e.Text))
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
