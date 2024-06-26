using System;
using System.Diagnostics;
using D8.Maui.Components.Gestures.Models;

namespace D8.Maui.Components.Gestures;

public class SwipeGestureRecognizer : DragGestureRecognizer
{

    private enum AxisRestriction
    {
        Horizontal,
        Vertical
    }

    private AxisRestriction? _restrictedAxis = null;

    public SwipeGestureRecognizer()
    {
        KeepInBounds = false;
    }

    public event EventHandler<SwipeCompletedEventArgs>? SwipeCompleted = null;
    public event EventHandler? SwipeCancelled = null;


    #region Observable Properties 

    public static BindableProperty DirectionProperty = BindableProperty.Create(nameof(Direction), typeof(Direction), typeof(SwipeGestureRecognizer), Direction.Horizontal);
    public Direction Direction
    {
        get => (Direction)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    public static BindableProperty BreakoverPercentageProperty = BindableProperty.Create(nameof(BreakoverPercentage), typeof(double), typeof(SwipeGestureRecognizer), 0.4d);
    public double BreakoverPercentage
    {
        get => (double)GetValue(BreakoverPercentageProperty);
        set => SetValue(BreakoverPercentageProperty, value);
    }

    public BindableProperty BreakoverAnimationSpeedProperty = BindableProperty.Create(nameof(BreakoverAnimationSpeed), typeof(int), typeof(SwipeGestureRecognizer), 250);
    public int BreakoverAnimationSpeed
    {
        get => (int)GetValue(BreakoverAnimationSpeedProperty);
        set => SetValue(BreakoverAnimationSpeedProperty, value);
    }

    public BindableProperty UseBreakoverVelocityProperty = BindableProperty.Create(nameof(UseBreakoverVelocity), typeof(bool), typeof(SwipeGestureRecognizer), true);
    public bool UseBreakoverVelocity
    {
        get => (bool)GetValue(UseBreakoverVelocityProperty);
        set => SetValue(UseBreakoverVelocityProperty, value);
    }

    public BindableProperty ReturnIfIncompleteProperty = BindableProperty.Create(nameof(ReturnIfIncomplete), typeof(bool), typeof(SwipeGestureRecognizer), true);
    public bool ReturnIfIncomplete
    {
        get => (bool)GetValue(ReturnIfIncompleteProperty);
        set => SetValue(ReturnIfIncompleteProperty, value);
    }



    #endregion

    protected override PanAllowed OnPanning(VerticalDirection verticalDirection, HorizontalDirection horizontalDirection, Point newTranslationValues, PanAllowed panAllowed)
    {
        // Putting the ones most important to D8 at the top.  Speed impact is minimal, but still

        if (Direction == Direction.HorizontalOrUp)
        {
            if (_restrictedAxis == null)
            {
                if (Math.Abs(newTranslationValues.X) > Math.Abs(newTranslationValues.Y))
                    _restrictedAxis = AxisRestriction.Horizontal;
                else
                    _restrictedAxis = AxisRestriction.Vertical;
            }

            if (_restrictedAxis == AxisRestriction.Horizontal && horizontalDirection != HorizontalDirection.None)
                return new PanAllowed(false, true);

            if (_restrictedAxis == AxisRestriction.Vertical && verticalDirection == VerticalDirection.Up)
                return new PanAllowed(true, false);
        }

        if (_restrictedAxis != null)
            return new PanAllowed(false, false);

        if (Direction == Direction.Up)
        {
            // Need to account for returning it to zero if you've already moved down
            if (verticalDirection == VerticalDirection.Down && newTranslationValues.Y >= InitialTranslationY)
            {
                // When you move quickly, it looks like the underlying PanGestureRecognizer is skipping frames to keep up, so it's
                // reporting a value that is too big/small depending on direction
                AnimationTarget.TranslationY = 0;
                return new PanAllowed(false, false);
            }
                

            return new PanAllowed(true, false);
        }

        if (Direction == Direction.Horizontal)
        {
            if (horizontalDirection == HorizontalDirection.None)
                return new PanAllowed(false, false);
            
            return new PanAllowed(false, true);
        }

        if (Direction == Direction.Down)
        {
            // Need to account for returning it to zero if you've already moved up
            if (verticalDirection == VerticalDirection.Up && newTranslationValues.Y <= InitialTranslationY)
            {
                AnimationTarget.TranslationY = 0;
                return new PanAllowed(false, false);
            }

            return new PanAllowed(true, false);
        }

        if (Direction == Direction.Vertical)
        {
            if (verticalDirection == VerticalDirection.None)
                return new PanAllowed(false, false);

            return new PanAllowed(true, false);
        }

        if (Direction == Direction.Left)
        {
            if (horizontalDirection == HorizontalDirection.Right && newTranslationValues.X >= InitialTranslationX)
            {
                AnimationTarget.TranslationX = 0;
                return new PanAllowed(false, false);
            }
                
            return new PanAllowed(false, true);
        }

        if (Direction == Direction.Right)
        {
            if (horizontalDirection == HorizontalDirection.Left && newTranslationValues.X <= InitialTranslationX)
            {
                AnimationTarget.TranslationX = 0;
                return new PanAllowed(false, false);
            }

            return new PanAllowed(false, true);
        }

        // This should never happen, but I want to be explicit in the if statements, so the compiler needs it
        return base.OnPanning(verticalDirection, horizontalDirection, newTranslationValues, panAllowed);
    }

    protected override void OnPanned(VerticalDirection verticalDirection, HorizontalDirection horizontalDirection, Point newTranslationValues)
    {
        base.OnPanned(verticalDirection, horizontalDirection, newTranslationValues);

        AnimationTarget.TranslationY = newTranslationValues.Y;
        AnimationTarget.TranslationX = newTranslationValues.X;
    }

    protected override void OnPanCompleted(VerticalPanStatistics verticalStatistics, HorizontalPanStatistics horizontalStatistics)
    {
        const double millisecondsLimit = 0.75;
        const uint minimumVelocity = 50;

        _restrictedAxis = null;

        if (isHorizontal(Direction) && horizontalStatistics.Direction != HorizontalDirection.None)
        {
            if (hitsBreakoverCondition(horizontalStatistics.PercentageTowardBoundary, horizontalStatistics.MillisecondsPerUnit))
            {
                var animationSpeed = calculateAnimationSpeed(horizontalStatistics.MillisecondsPerUnit, horizontalStatistics.DistanceRemaining);

                if (horizontalStatistics.Direction == HorizontalDirection.Left)
                    animateCloseHorizontal(SwipeDirection.Left, MinimumTranslationX, animationSpeed);
                else
                    animateCloseHorizontal(SwipeDirection.Right, MaximumTranslationX, animationSpeed);
            }
            else if (ReturnIfIncomplete)
            {
                var animationSpeed = calculateAnimationSpeed(horizontalStatistics.MillisecondsPerUnit, horizontalStatistics.Distance);

                AnimationTarget.TranslateTo(0, PanStartingTranslations.X, animationSpeed);
                SwipeCancelled?.Invoke(AnimationTarget, new EventArgs());
            }
            //todo: need to raise completed event if they don't break over

        }
        else if (isVertical(Direction) && verticalStatistics.Direction != VerticalDirection.None)
        {
            if (hitsBreakoverCondition(verticalStatistics.PercentageTowardBoundary, verticalStatistics.MillisecondsPerUnit))
            {
                var animationSpeed = calculateAnimationSpeed(verticalStatistics.MillisecondsPerUnit, verticalStatistics.DistanceRemaining);

                if (verticalStatistics.Direction == VerticalDirection.Up)
                    animateCloseVertical(SwipeDirection.Up, MinimumTranslationY, animationSpeed);
                else
                    animateCloseVertical(SwipeDirection.Down, MaximumTranslationY, animationSpeed);
            }
            else if (ReturnIfIncomplete)
            {
                var animationSpeed = calculateAnimationSpeed(verticalStatistics.MillisecondsPerUnit, verticalStatistics.Distance);
                AnimationTarget.TranslateTo(PanStartingTranslations.X, 0, animationSpeed);

                SwipeCancelled?.Invoke(AnimationTarget, new EventArgs());
            }
        }

        base.OnPanCompleted(verticalStatistics, horizontalStatistics);



        #region Local Methods

        void animateCloseHorizontal(SwipeDirection direction, double translationX, uint animationSpeed)
        {
            var slideAnimation = new Animation(v => AnimationTarget.TranslationX = v, AnimationTarget.TranslationX, translationX);
            slideAnimation.Commit(AnimationTarget, "SwipeAnimation", 30, animationSpeed, finished: (d, b) =>
            {
                SwipeCompleted?.Invoke(AnimationTarget, new SwipeCompletedEventArgs(direction));
            });
        }

        void animateCloseVertical(SwipeDirection direction, double translationY, uint animationSpeed)
        {
            var slideAnimation = new Animation(v => AnimationTarget.TranslationY = v, AnimationTarget.TranslationY, translationY);
            slideAnimation.Commit(AnimationTarget, "SwipeAnimation", 30, animationSpeed, finished: (d, b) =>
            {
                SwipeCompleted?.Invoke(AnimationTarget, new SwipeCompletedEventArgs(direction));
            });
        }

        uint calculateAnimationSpeed(double millisecondsPerUnit, double distance)
        {
            var velocitySpeed = (uint)(millisecondsPerUnit * distance);
            if (velocitySpeed < minimumVelocity) velocitySpeed = minimumVelocity;

            return UseBreakoverVelocity ? velocitySpeed : (uint)BreakoverAnimationSpeed;
        }

        bool hitsBreakoverCondition(double percentage, double millisecondsPerUnit) =>
            BreakoverPercentage >= 0 && (percentage > BreakoverPercentage || millisecondsPerUnit < millisecondsLimit);

        bool isHorizontal(Direction direction) =>
            direction == Direction.Horizontal ||
            direction == Direction.Left ||
            direction == Direction.Right ||
            direction == Direction.HorizontalOrUp;

        bool isVertical(Direction direction) =>
            direction == Direction.Vertical ||
            direction == Direction.Up ||
            direction == Direction.Down ||
            direction == Direction.HorizontalOrUp;

        #endregion 
    }

    protected override void OnPanCancelled()
    {
        _restrictedAxis = null;
        base.OnPanCancelled();
    }

   

}



