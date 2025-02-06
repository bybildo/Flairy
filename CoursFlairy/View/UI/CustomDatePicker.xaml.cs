using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static CoursFlairy.ViewModel.ResourceColor;

namespace CoursFlairy.View.UI
{
    public partial class CustomDatePicker : UserControl, INotifyPropertyChanged
    {
        private List<DayStruct> _previewDays = new List<DayStruct>();
        private Visibility _leftArrow = Visibility.Hidden;
        private Visibility _rightArrow = Visibility.Visible;
        private bool _backInterface = false;

        private DateTime CurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        private Dictionary<DateTime, List<DayStruct>> MonthDays = new Dictionary<DateTime, List<DayStruct>>();

        private string _checkedDays = "";
        private string _checkedBackDays = "";

        public event EventHandler DaysSelect;
        public event EventHandler DaysUnselect;

        public CustomDatePicker()
        {
            InitializeComponent();
            LoadDays();
        }

        #region Властивості
        public List<DayStruct> PreviewDays
        {
            get => _previewDays;
            set
            {
                _previewDays = value;
                OnPropertyChanged(nameof(PreviewDays));
            }
        }

        public string Month
        {
            get => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(CurrentMonth.ToString("MMMM"));
        }

        private DateTime FarLeftMonth = DateTime.Now;
        public Visibility LeftArrow
        {
            get
            {
                if (CurrentMonth < FarLeftMonth)
                    _leftArrow = Visibility.Hidden;
                else
                    _leftArrow = Visibility.Visible;

                return _leftArrow;
            }
        }

        public Visibility RightArrow
        {
            get
            {
                if (CurrentMonth < DateTime.Now.AddMonths(11))
                    _rightArrow = Visibility.Visible;
                else
                    _rightArrow = Visibility.Hidden;

                return _rightArrow;
            }
        }

        public string CheckedDays
        {
            get => _checkedDays;
            set
            {
                _checkedDays = value;
                OnPropertyChanged(nameof(CheckedDays));

                if (!string.IsNullOrEmpty(_checkedDays))
                    DaysSelect?.Invoke(this, EventArgs.Empty);
                else
                    DaysUnselect?.Invoke(this, EventArgs.Empty);
            }
        }

        public string CheckedBackDays
        {
            get => _checkedBackDays;
            set
            {
                _checkedBackDays = value;
                OnPropertyChanged(nameof(CheckedBackDays));
            }
        }
        #endregion

        #region Методи

        #region Інтерфейсу
        private void LeftArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            LoadDays();
            OnPropertyChanged(nameof(LeftArrow));
            OnPropertyChanged(nameof(RightArrow));
            OnPropertyChanged(nameof(Month));
        }

        private void RightArrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            LoadDays();
            OnPropertyChanged(nameof(LeftArrow));
            OnPropertyChanged(nameof(RightArrow));
            OnPropertyChanged(nameof(Month));
        }

        private void Month_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool isTransparent = PreviewDays.FirstOrDefault(day => day.Date.Month == CurrentMonth.Month && day.Date < CurrentMonth.AddMonths(1) && day.Date > DateTime.Now.AddDays(-1))?.CheckedBackground is SolidColorBrush brush && brush.Color == Colors.Transparent;

            foreach (var day in PreviewDays.Where(day => day.Date.Month == CurrentMonth.Month && day.Date < CurrentMonth.AddMonths(1) && day.Date > DateTime.Now.AddDays(-1) && day.CanChange == true))
            {
                day.CheckedBackground = new SolidColorBrush(isTransparent ? MainColor100.Color : Colors.Transparent);
            }

            MarginUpdate();
            UpdateCheckedDays();
        }

        private void DayOfWeek_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UIElement element)
            {
                int column = Grid.GetColumn(element);
                bool isTransparent = PreviewDays.FirstOrDefault(day => day.Date.Month == CurrentMonth.Month && day.Date < CurrentMonth.AddMonths(1) && day.Date > DateTime.Now.AddDays(-1) && day.Date.AddDays(-1).DayOfWeek == (DayOfWeek)column)?.CheckedBackground is SolidColorBrush brush && brush.Color == Colors.Transparent;

                foreach (var day in PreviewDays.Where(day => (int)day.Date.AddDays(-1).DayOfWeek == column && day.Date > DateTime.Now.AddDays(-1)))
                {
                    day.CheckedBackground = new SolidColorBrush(isTransparent ? MainColor100.Color : Colors.Transparent);
                }
            }

            MarginUpdate();
            UpdateCheckedDays();
        }
        #endregion

        #region Класу
        private void MarginUpdate()
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();

            for (int i = 0; i < PreviewDays.Count; i++)
            {
                if (PreviewDays[i].CheckedBackground.Color != Colors.Transparent)
                {
                    if (counts.Count > 0 && i == counts.Last().Key + counts.Last().Value && (int)PreviewDays[i].Date.DayOfWeek != 1)
                    {
                        counts[counts.Last().Key]++;
                        continue;
                    }
                    counts.Add(i, 1);
                }
            }

            foreach (var day in PreviewDays)
            {
                day.CheckedMargin = new Thickness(0);
            }

            counts = counts.Where(pair => pair.Value > 1).ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (var item in counts)
            {
                for (int i = item.Key; i < item.Key + item.Value; i++)
                {
                    if (i == item.Key)
                        PreviewDays[i].CheckedMargin = new Thickness(0, 0, -4.5, 0);

                    else if (i == item.Key + item.Value - 1)
                        PreviewDays[i].CheckedMargin = new Thickness(-4.5, 0, 0, 0);

                    else PreviewDays[i].CheckedMargin = new Thickness(-4.5, 0, -4.5, 0);
                }
            }

            ColorUpdate();
        }

        private void AdditionalMarginUpdate()
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();

            for (int i = 0; i < PreviewDays.Count; i++)
            {
                if (PreviewDays[i].AdditionalBackground.Color != Colors.Transparent)
                {
                    if (counts.Count > 0 && i == counts.Last().Key + counts.Last().Value && (int)PreviewDays[i].Date.DayOfWeek != 1)
                    {
                        counts[counts.Last().Key]++;
                        continue;
                    }
                    counts.Add(i, 1);
                }
            }

            foreach (var day in PreviewDays)
            {
                day.AdditionalMargin = new Thickness(0);
            }

            counts = counts.Where(pair => pair.Value > 1).ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (var item in counts)
            {
                for (int i = item.Key; i < item.Key + item.Value; i++)
                {
                    if (i == item.Key)
                        PreviewDays[i].AdditionalMargin = new Thickness(0, 0, -4.5, 0);

                    else if (i == item.Key + item.Value - 1)
                        PreviewDays[i].AdditionalMargin = new Thickness(-4.5, 0, 0, 0);

                    else PreviewDays[i].AdditionalMargin = new Thickness(-4.5, 0, -4.5, 0);
                }
            }
        }

        private void ColorUpdate()
        {
            var firstDayMonth = PreviewDays.Where(el => el.Text == "1").FirstOrDefault()?.Date.Month;

            if (firstDayMonth == null) return;

            foreach (var previewDay in PreviewDays)
            {
                if (previewDay.AdditionalBackground.Color != Colors.Transparent)
                {
                    previewDay.DayForeground = White;
                    continue;
                }

                if (previewDay.CanChange == false)
                {
                    if (previewDay.AdditionalBackground.Color == Colors.Transparent)
                        previewDay.DayForeground = MainColor10;
                    continue;
                }

                if (previewDay.CheckedBackground.Color == Colors.Transparent)
                {
                    if (previewDay.Date.Month != firstDayMonth)
                        previewDay.DayForeground = DatePickerGray;
                    else previewDay.DayForeground = MainColor100;
                    continue;
                }
            }
        }

        private void LoadDays()
        {
            if (!MonthDays.ContainsKey(CurrentMonth))
                AddMonth(CurrentMonth);

            UpdatePreviewDays(CurrentMonth);
        }

        private void AddMonth(DateTime month)
        {
            List<DayStruct> result = new List<DayStruct>();

            for (int day = 1; day <= DateTime.DaysInMonth(month.Year, month.Month); day++)
            {
                DateTime date = new DateTime(month.Year, month.Month, day);
                result.Add(new DayStruct(date, month));
            }

            MonthDays.Add(month, result);
        }

        private void UpdatePreviewDays(DateTime month)
        {
            var days = MonthDays[month];

            List<DayStruct> result = new List<DayStruct>();

            int dayOfWeek = (int)days[0].Date.DayOfWeek;
            int daysToSubtract = dayOfWeek == 0 ? 6 : dayOfWeek - 1;

            for (int i = 0; i < daysToSubtract; i++)
            {
                if (MonthDays.ContainsKey(month.AddMonths(-1)))
                    result.Add(MonthDays[month.AddMonths(-1)][MonthDays[month.AddMonths(-1)].Count - 1 * (daysToSubtract - i)]);
                else result.Add(new DayStruct(days[0].Date.AddDays(-1 * (daysToSubtract - i)), month));
            }

            result.AddRange(days);

            if (!MonthDays.ContainsKey(month.AddMonths(1)))
                AddMonth(month.AddMonths(1));

            dayOfWeek = (int)days[days.Count - 1].Date.DayOfWeek;
            for (int i = 1; i <= 7 - dayOfWeek; i++)
            {
                result.Add(MonthDays[month.AddMonths(1)][i - 1]);
            }

            PreviewDays = result;
            MarginUpdate();
        }

        private void UpdateCheckedDays()
        {
            List<DateTime> selectedDays = GetSelectedDays();

            StringBuilder result = new StringBuilder();

            if (selectedDays.Count > 0)
            {
                DateTime rangeStart = selectedDays[0];
                DateTime rangeEnd = rangeStart;

                for (int i = 1; i < selectedDays.Count; i++)
                {
                    if (selectedDays[i] == rangeEnd.AddDays(1))
                    {
                        rangeEnd = selectedDays[i];
                    }
                    else
                    {
                        result.Append(result.Length > 0 ? ", " : "").Append(rangeStart == rangeEnd ? rangeStart.ToString("MM.dd") : $"{rangeStart:MM.dd}-{rangeEnd:MM.dd}");
                        rangeStart = rangeEnd = selectedDays[i];
                    }
                }

                result.Append(result.Length > 0 ? ", " : "").Append(rangeStart == rangeEnd ? rangeStart.ToString("MM.dd") : $"{rangeStart:MM.dd}-{rangeEnd:MM.dd}");
            }

            if (!_backInterface)
                CheckedDays = result.ToString();
            else CheckedBackDays = result.ToString();
        }

        #region Вибір дня
        private bool _isSelecting;
        private bool _isAdding;
        private DayStruct _startDay;
        private HashSet<DayStruct> _initiallySelectedDays = new HashSet<DayStruct>();

        private void DayChecked_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is FrameworkElement element) || !(element.DataContext is DayStruct day) || day.Date <= DateTime.Now.AddDays(-1) || day.CanChange == false) return;

            _isSelecting = true;
            _startDay = day;
            _initiallySelectedDays.Clear();
            _initiallySelectedDays.UnionWith(PreviewDays.Where(d => d.CheckedBackground.Color != Colors.Transparent));

            _isAdding = day.CheckedBackground.Color == Colors.Transparent;
            day.CheckedBackground = new SolidColorBrush(_isAdding ? MainColor100.Color : Colors.Transparent);
            Mouse.Capture(sender as IInputElement);

            MarginUpdate();
        }

        private void DayChecked_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting || !(sender is ItemsControl itemsControl)) return;

            Point position = e.GetPosition(itemsControl);
            if (!(VisualTreeHelper.HitTest(itemsControl, position)?.VisualHit is FrameworkElement element) || !(element.DataContext is DayStruct day) || day.Date <= DateTime.Now.AddDays(-1) || day.CanChange == false) return;

            var startIndex = PreviewDays.IndexOf(_startDay);
            var endIndex = PreviewDays.IndexOf(day);

            PreviewDays.ForEach(d => d.CheckedBackground = new SolidColorBrush(_initiallySelectedDays.Contains(d) ? MainColor100.Color : Colors.Transparent));
            for (int i = Math.Min(startIndex, endIndex); i <= Math.Max(startIndex, endIndex); i++)
            {
                PreviewDays[i].CheckedBackground = new SolidColorBrush(_isAdding ? MainColor100.Color : Colors.Transparent);
            }

            MarginUpdate();
        }

        private void DayChecked_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelecting) return;

            _isSelecting = false;
            Mouse.Capture(null);

            MarginUpdate();
            UpdateCheckedDays();
        }
        #endregion

        #endregion

        #region Зв'язку

        public List<DateTime> GetSelectedDays()
        {
            var result = new HashSet<DateTime>();

            foreach (var keyValue in MonthDays)
            {
                foreach (var day in keyValue.Value)
                {
                    if (day.CheckedBackground.Color != Colors.Transparent)
                    {
                        result.Add(day.Date);
                    }
                }
            }

            return result.OrderBy(date => date).ToList();
        }

        private Dictionary<DateTime, List<DayStruct>> _beckUpMonth;
        private DateTime _beckUpMonthPage;

        public void WayBackInterface()
        {
            if (_backInterface) return;

            _beckUpMonth = MonthDays.ToDictionary(entry => entry.Key, entry => entry.Value.Select(day => (DayStruct)day.Clone()).ToList());

            int indexOfLast = -1;
            DateTime indexOfMonth = DateTime.MinValue;

            foreach (var month in MonthDays.Values)
            {
                for (int i = 0; i < month.Count; i++)
                {
                    if (month[i].CheckedBackground.Color != Colors.Transparent)
                    {
                        month[i].CheckedBackground = new SolidColorBrush(Colors.Transparent);
                        month[i].AdditionalBackground = new SolidColorBrush(Colors.Red);
                        indexOfLast = i;
                        indexOfMonth = month[i].Date;
                    }
                }
            }

            if (indexOfLast != -1)
            {
                FarLeftMonth = indexOfMonth;
                DateTime month = new DateTime(indexOfMonth.Year, indexOfMonth.Month, 1);
                _beckUpMonthPage = month;

                for (int i = 0; i < indexOfLast; i++)
                {
                    MonthDays[month][i].CanChange = false;
                }

                CurrentMonth = month;
                LoadDays();
                month = month.AddMonths(-1);

                if (MonthDays.ContainsKey(month))
                    for (int i = 1; i < MonthDays[month].Count; i++)
                        MonthDays[month][i].CanChange = false;

            }

            OnPropertyChanged(nameof(LeftArrow));
            OnPropertyChanged(nameof(RightArrow));
            OnPropertyChanged(nameof(Month));
            AdditionalMarginUpdate();
            _backInterface = true;

            ColorUpdate();
        }

        public void WayForwardInterface()
        {
            if (!_backInterface || _beckUpMonth == null) return;

            CurrentMonth = _beckUpMonthPage;
            MonthDays = _beckUpMonth;
            LoadDays();

            FarLeftMonth = DateTime.Now;
            OnPropertyChanged(nameof(LeftArrow));
            OnPropertyChanged(nameof(RightArrow));
            OnPropertyChanged(nameof(Month));
            MarginUpdate();
            _backInterface = false;
            CheckedBackDays = "";

            ColorUpdate();
        }

        #endregion
        #endregion

        public class DayStruct : INotifyPropertyChanged, ICloneable
        {
            public bool CanChange = true;
            private string _text;
            private DateTime _date;
            private SolidColorBrush _dayForeground;
            private SolidColorBrush _checkedBackground;
            private SolidColorBrush _additionalBackground = new SolidColorBrush(Colors.Transparent);
            private Thickness _checkedMargin = new Thickness(0);
            private Thickness _additionalMargin = new Thickness(0);

            public DayStruct(DateTime date, DateTime currentMonth)
            {
                Date = date;
                Text = date.Day.ToString();
                CheckedBackground = new SolidColorBrush(Colors.Transparent);
                DayForeground = MainColor100;
                if (Date.Month != currentMonth.Month && DayForeground.Color == MainColor100.Color)
                    DayForeground.Color = MainColor10.Color;
            }

            public DayStruct(bool CanChange, string _text, DateTime _date, SolidColorBrush _dayForeground, SolidColorBrush _checkedBackground, SolidColorBrush _additionalBackgroundm, Thickness _checkedMargin, Thickness _additionalMargin)
            {
                this.CanChange = CanChange;
                this._text = _text;
                this._date = _date;
                this._dayForeground = _dayForeground;
                this._checkedBackground = _checkedBackground;
                this._additionalBackground = _additionalBackgroundm;
                this._checkedMargin = _checkedMargin;
                this._additionalMargin = _additionalMargin;
            }

            public string Text
            {
                get => _text;
                set
                {
                    if (_text != value)
                    {
                        _text = value;
                        OnPropertyChanged(nameof(Text));
                    }
                }
            }

            public DateTime Date
            {
                get => _date;
                set
                {
                    if (_date != value)
                    {
                        _date = value;
                        OnPropertyChanged(nameof(Date));
                    }
                }
            }

            public SolidColorBrush DayForeground
            {
                get => _dayForeground;
                set
                {
                    if (_dayForeground != value)
                    {
                        if (Date < DateTime.Now.AddDays(-1))
                            _dayForeground = MainColor10;

                        else _dayForeground = value;
                        OnPropertyChanged(nameof(DayForeground));
                    }
                }
            }

            public SolidColorBrush CheckedBackground
            {
                get => _checkedBackground;
                set
                {
                    if (_checkedBackground != value)
                    {
                        _checkedBackground = value;

                        if (value.Color == Colors.Transparent)
                            DayForeground = MainColor100;
                        else
                            DayForeground = White;

                        OnPropertyChanged(nameof(CheckedBackground));
                    }
                }
            }

            public Thickness CheckedMargin
            {
                get => _checkedMargin;
                set
                {
                    if (_checkedMargin != value)
                    {
                        _checkedMargin = value;
                        OnPropertyChanged(nameof(CheckedMargin));
                    }
                }
            }

            public SolidColorBrush AdditionalBackground
            {
                get => _additionalBackground;
                set
                {
                    if (_additionalBackground != value)
                    {
                        if (value.Color == Colors.Transparent)
                            _additionalBackground = value;
                        else
                        {
                            _additionalBackground = DatePickerAdditionalBackground;
                            DayForeground = White;
                        }
                        OnPropertyChanged(nameof(AdditionalBackground));
                    }
                }
            }

            public Thickness AdditionalMargin
            {
                get => _additionalMargin;
                set
                {
                    if (_additionalMargin != value)
                    {
                        _additionalMargin = value;
                        OnPropertyChanged(nameof(AdditionalMargin));
                    }
                }
            }

            public object Clone()
            {
                return new DayStruct(CanChange, _text, _date, _dayForeground, _checkedBackground, _additionalBackground, _checkedMargin, _additionalMargin);
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
