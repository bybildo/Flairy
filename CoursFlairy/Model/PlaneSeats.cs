using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using CoursFlairy.Model.Enum;
using static CoursFlairy.ViewModel.ResourceColor;

namespace CoursFlairy.Model
{
    public class PlaneSeats : INotifyPropertyChanged
    {
        public PlaneBrushes SeatClass { get; set; }
        public PlaneBrushes CurrentClass { get; set; }
        public string Row { get; set; }
        public int SeatNumber { get; set; }

        private double _width;
        private double _height;
        private bool _isSelected;
        private RotateTransform _pathAngleTransform = new RotateTransform { Angle = 90 };

        public PlaneSeats(PlaneBrushes seatClass, string row, int seatNumber, double size, Classes curentClass)
        {
            SeatClass = seatClass;
            Row = row;
            SeatNumber = seatNumber;
            _width = size;
            _height = size;
            _isSelected = false;
            SetClass(curentClass);
        }

        public string SeatText { get { if (SeatClass == PlaneBrushes.wc) return "wc"; return $"{Row}{SeatNumber}"; } }
        public double WidthElement { get => _width; set => _width = value; }
        public double HeightElement { get { if (SeatClass == PlaneBrushes.enter || SeatClass == PlaneBrushes.empty) return _height / 2; return _height; } set => _height = value; }
        public Visibility PathVisiblity { get { if (SeatClass == PlaneBrushes.enter) return Visibility.Visible; return Visibility.Collapsed; } }
        public RotateTransform PathAngleTransform { get => _pathAngleTransform; set => _pathAngleTransform = value; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(BorderColor));
                    OnPropertyChanged(nameof(PlaneLayoutColor));
                    OnPropertyChanged(nameof(OriginalSeatColor));
                }
            }
        }

        public bool CanBeSelected
        {
            get
            {
                return SeatClass != PlaneBrushes.wc && 
                       SeatClass != PlaneBrushes.empty && 
                       SeatClass != PlaneBrushes.enter &&
                       (CurrentClass == PlaneBrushes.empty || CurrentClass == SeatClass);
            }
        }

        public void ToggleSelection()
        {
            if (!CanBeSelected && IsSelected)
            {
                IsSelected = false;
            }
            else if (CanBeSelected)
            {
                IsSelected = !IsSelected;
            }
        }

        public SolidColorBrush PlaneLayoutColor
        {
            get
            {
                if (IsSelected && CanBeSelected)
                {
                    return SelectedSeatColor;
                }

                return BorderColor;
            }
        }

        public SolidColorBrush OriginalSeatColor
        {
            get
            {
                switch (SeatClass)
                {
                    case PlaneBrushes.first:
                        return FirstColor;
                    case PlaneBrushes.bussiness:
                        return BusinessColor;
                    case PlaneBrushes.econom:
                        return EconomColor;
                    case PlaneBrushes.wc:
                        return WCColor;
                    default:
                        return EmptyColor;
                }
            }
        }

        public SolidColorBrush BorderColor
        {
            get
            {
                if (CurrentClass != PlaneBrushes.empty)
                {
                    if (CurrentClass != SeatClass && SeatClass != PlaneBrushes.wc && SeatClass != PlaneBrushes.empty && SeatClass != PlaneBrushes.enter) 
                        return HintColor;
                }

                return OriginalSeatColor;
            }
        }

        public override string ToString()
        {
            return Row + "-" + SeatNumber + "-" + (int)SeatClass;
        }

        public Classes GetClass()
        {
            switch (SeatClass)
            {
                case PlaneBrushes.econom:
                    return Classes.Econom;
                case PlaneBrushes.bussiness:
                    return Classes.Bussiness;
                case PlaneBrushes.first:
                    return Classes.First;
                default: return Classes.None;
            }
        }

        public void SetClass(Classes classes)
        {
            switch (classes)
            {
                case Classes.Econom:
                    CurrentClass = PlaneBrushes.econom;
                    break;
                case Classes.Bussiness:
                    CurrentClass = PlaneBrushes.bussiness;
                    break;
                case Classes.First:
                    CurrentClass = PlaneBrushes.first;
                    break;
                default: CurrentClass = PlaneBrushes.empty; break;
            }
            OnPropertyChanged(nameof(BorderColor));
            OnPropertyChanged(nameof(PlaneLayoutColor));
            OnPropertyChanged(nameof(OriginalSeatColor));
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