using CoursFlairy.Data;
using CoursFlairy.View;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;


namespace CoursFlairy.ViewModel
{
    class MainWindowVM : INotifyPropertyChanged
    {
        MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
        public MainWindowVM()
        {
            if (mainWindow == null)
                throw new InvalidOperationException("mainWindow not found");

            Icon_MouseDown = new RelayCommand(_ => Icon_MouseDownF());
            Account_MouseDown = new RelayCommand(_ => Account_MouseDownF());
            Close_MouseDown = new RelayCommand(_ => Close_MouseDownF());
            Сollapse_MouseDown = new RelayCommand(_ => Сollapse_MouseDownF());
        }

        private void Account_MouseDownF()
        {
            MessageBox.Show("Account");
        }

        private void Close_MouseDownF()
        {
            DataBase.Close();
            mainWindow.Close();
        }

        private void Сollapse_MouseDownF()
        {
            mainWindow.WindowState = System.Windows.WindowState.Minimized;
        }

        private void Icon_MouseDownF()
        {
            if (mainWindow.PageManager == null)
                throw new InvalidOperationException("frame not found");
            mainWindow.PageManager.Navigate(new SearchPage());
        }

        public ICommand Close_MouseDown { get; private set; }
        public ICommand Сollapse_MouseDown { get; private set; }
        public ICommand Icon_MouseDown { get; private set; }
        public ICommand Account_MouseDown { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
