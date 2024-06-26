using System;
using D8.Maui.Components.Gestures.Models;

namespace D8.Maui.Components.Gestures;


public class NavigationSwipeGestureRecognizer : PanGestureRecognizerBase
{
	public NavigationSwipeGestureRecognizer()
	{
	}

	public event EventHandler<NavigationSwipeEventArgs>? SwipeCompleted = null;
	public event EventHandler<NavigationSwipeEventArgs>? SwipeCancelled = null;
	public event EventHandler<NavigationSwipingEventArgs>? Swiping = null;

    #region Observable Properties

    public static BindableProperty BreakoverPercentageProperty = BindableProperty.Create(nameof(BreakoverPercentage), typeof(double), typeof(NavigationSwipeGestureRecognizer), 0.2d);
    public double BreakoverPercentage
    {
        get => (double)GetValue(BreakoverPercentageProperty);
        set => SetValue(BreakoverPercentageProperty, value);
    }

    public BindableProperty CanSwipeBackProperty = BindableProperty.Create(nameof(CanSwipeBack), typeof(bool), typeof(NavigationSwipeGestureRecognizer), true);
	public bool CanSwipeBack
	{
		get => (bool)GetValue(CanSwipeBackProperty);
		set => SetValue(CanSwipeBackProperty, value);
	}

    public BindableProperty CanSwipeForwardProperty = BindableProperty.Create(nameof(CanSwipeForward), typeof(bool), typeof(NavigationSwipeGestureRecognizer));
    public bool CanSwipeForward
    {
        get => (bool)GetValue(CanSwipeForwardProperty);
        set => SetValue(CanSwipeForwardProperty, value);
    }

	public BindableProperty TouchTargetWidthProperty = BindableProperty.Create(nameof(TouchTargetWidth), typeof(int), typeof(NavigationSwipeGestureRecognizer), 10);
	public int TouchTargetWidth
	{
		get => (int)GetValue(TouchTargetWidthProperty);
		set => SetValue(TouchTargetWidthProperty, value);
	}



	#endregion

	protected override bool OnPanStarting(Point touchLocation)
	{
        var parent = Parent as VisualElement;
        if (parent == null)
            return false;

        var page = getRootElement(parent);
		MinimumTranslationX = page.Width * -1;
		MaximumTranslationX = page.Width * 2;

		MinimumTranslationY = 0;
		MaximumTranslationY = 0;


        if (CanSwipeBack && touchLocation.X <= TouchTargetWidth)
			return true;

		if (CanSwipeForward && touchLocation.X >= parent.Width - TouchTargetWidth)
			return true;

		return false;



        VisualElement getRootElement(VisualElement element)
        {
            var parent = element.Parent as VisualElement;
            if (parent == null)
                return element;

            return getRootElement(parent);
        }
    }

    protected override PanAllowed OnPanning(VerticalDirection verticalDirection, HorizontalDirection horizontalDirection, Point newTranslationValues, PanAllowed panAllowed)
    {
		if (CanSwipeBack && horizontalDirection == HorizontalDirection.Right && panAllowed.MoveHorizontal)
			return new PanAllowed(false, true);

		if (CanSwipeForward && horizontalDirection == HorizontalDirection.Left && panAllowed.MoveHorizontal)
			return new PanAllowed(false, true);

		return new PanAllowed(false, false);
    }

    protected override void OnPanned(VerticalDirection verticalDirection, HorizontalDirection horizontalDirection, Point newTranslationValues)
    {
		//AnimationTarget.TranslationX = newTranslationValues.X;
		if (CanSwipeBack && horizontalDirection == HorizontalDirection.Right)
			Swiping?.Invoke(this, new NavigationSwipingEventArgs(NavigationDirection.Back, newTranslationValues.X));
		else if (CanSwipeForward && horizontalDirection == HorizontalDirection.Left)
            Swiping?.Invoke(this, new NavigationSwipingEventArgs(NavigationDirection.Forward, newTranslationValues.X));
    }

	protected override void OnPanCompleted(VerticalPanStatistics verticalStatistics, HorizontalPanStatistics horizontalStatistics)
	{
		if (CanSwipeBack && horizontalStatistics.Direction == HorizontalDirection.Right)
		{
			if (horizontalStatistics.PercentageTowardBoundary >= BreakoverPercentage)
			{
                SwipeCompleted?.Invoke(
					this,
					new NavigationSwipeEventArgs(
						NavigationDirection.Back,
						horizontalStatistics.PercentageTowardBoundary < 1 ? false : true));
            }
			else
			{
                SwipeCancelled?.Invoke(
                    this,
                    new NavigationSwipeEventArgs(
                        NavigationDirection.Back,
                        horizontalStatistics.PercentageTowardBoundary < 1 ? false : true));
            }
                
			// Invoke cancelled event
        }

		else if (CanSwipeForward && horizontalStatistics.Direction == HorizontalDirection.Left)
		{
			if (horizontalStatistics.PercentageTowardBoundary >= BreakoverPercentage)
			{
                SwipeCompleted?.Invoke(
                    this,
                    new NavigationSwipeEventArgs(
                        NavigationDirection.Forward,
                        horizontalStatistics.PercentageTowardBoundary < 1 ? false : true));

            }
            else
            {
                SwipeCancelled?.Invoke(
                    this,
                    new NavigationSwipeEventArgs(
                        NavigationDirection.Forward,
                        horizontalStatistics.PercentageTowardBoundary < 1 ? false : true));
            }
        }

		//base.OnPanCompleted(verticalStatistics, horizontalStatistics);
	}

}

