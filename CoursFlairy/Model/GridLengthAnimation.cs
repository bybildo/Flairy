using System.Windows.Media.Animation;
using System.Windows;

namespace CoursFlairy.Model
{
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
