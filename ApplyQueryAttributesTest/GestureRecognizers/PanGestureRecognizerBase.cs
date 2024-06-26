using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using D8.Maui.Components.Gestures.Models;
using D8.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Platform;

namespace D8.Maui.Components.Gestures;

public record PanEventArgs(VerticalDirection VerticalDirection, HorizontalDirection HorizontalDirection, Point NewTranslationValues);
public record PanCompletedEventArgs(VerticalPanStatistics VerticalStatistics, HorizontalPanStatistics HorizontalStatistics);

public class PanGestureRecognizerBase : Microsoft.Maui.Controls.PanGestureRecognizer
{
    private record ParentPosition(double StartingPosition, double FinishPosition, double Distance, double MillisecondsPerPoint, double TotalDistance);
    protected record PanAllowed(bool MoveVertical, bool MoveHorizontal);

    private DateTime? _panStart;
    private Point _initialTouchLocation { get; set; } = new Point(double.MinValue, double.MinValue);
    private Point _previousTouchLocation { get; set; } = new Point(double.MinValue, double.MinValue);
    private bool _ignorePan = false;


    // These are the TranslationX and Y values that existed when the very first time the user panned.
    // Hopefully this is always zero, but you never know.  So, let's not assume
    protected double InitialTranslationX { get; private set; } = -1;
    protected double InitialTranslationY { get; private set; } = -1;

    

    //This contains the TranslationX and Y values each time the user starts panning
    protected Point PanStartingTranslations { get; private set; } = new Point(0, 0);

    protected bool WriteDiagnostics { get; set; } = true;


    public double MinimumTranslationX { get; protected set; } = double.MinValue;
    public double MinimumTranslationY { get; protected set; } = double.MinValue;
    public double MaximumTranslationX { get; protected set; } = double.MaxValue;
    public double MaximumTranslationY { get; protected set; } = double.MaxValue;


    public event EventHandler? PanStarted;
    public event EventHandler<PanEventArgs>? Panned;
    public event EventHandler<PanCompletedEventArgs>? PanCompleted;
    public event EventHandler? PanCancelled;

    public PanGestureRecognizerBase()
    {
        PanUpdated += PanGestureRecognizer_PanUpdated;
    }

    protected override void OnParentSet()
    {
        if (AnimationTarget == null)
        {
            var parent = Parent as VisualElement;
            if (parent == null)
                throw new Exception("Parent does not inherit from VisualElement");

            AnimationTarget = parent;
        }
    
        base.OnParentSet();
    }




    #region Observable Properties

    public static BindableProperty AnimationTargetProperty = BindableProperty.Create(nameof(AnimationTargetProperty), typeof(VisualElement), typeof(PanGestureRecognizerBase));
    public VisualElement AnimationTarget
    {
        get => (VisualElement)GetValue(AnimationTargetProperty);
        set => SetValue(AnimationTargetProperty, value);
    }

    public static BindableProperty IsEnabledProperty = BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(PanGestureRecognizerBase), true);
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    #endregion

    #region Pan Handlers

    

    private void HandlePanCancelled(PanUpdatedEventArgs e)
    {
        Debug.WriteLine("Pan Cancelled");

        if (_ignorePan)
            return;

        OnPanCancelled();

        PanCancelled?.Invoke(AnimationTarget, new EventArgs());
    }

    private void HandlePanCompleted(PanUpdatedEventArgs e)
    {
        Debug.WriteLine("Pan Completed");

        if (_ignorePan)
            return;


        var currentTime = DateTime.Now;
        var milliseconds = (currentTime - _panStart!.Value).TotalMilliseconds;

        var horizontalDirection = getHorizontalDirection();
        var horizontalPosition = getHorizontalPosition();

        var verticalDirection = getVerticalDirection();
        var verticalPosition = getVerticalPosition();

        var verticalStatistics = getVerticalPanStatistics(verticalDirection, verticalPosition);
        var horizontalStatistics = getHorizontalPanStatistics(horizontalDirection, horizontalPosition);

        OnPanCompleted(verticalStatistics, horizontalStatistics);

        PanCompleted?.Invoke(AnimationTarget, new PanCompletedEventArgs(verticalStatistics, horizontalStatistics));

        //Console.WriteLine("----------------------------------------------");
        //Console.WriteLine($"horizontal percentage: {horizontalStatistics.PercentageTowardBoundary}");
        //Console.WriteLine($"vertical percentage: {verticalStatistics.PercentageTowardBoundary}");

        #region Local Methods

        HorizontalDirection getHorizontalDirection()
        {
            if (AnimationTarget.TranslationX > PanStartingTranslations.X)
                return HorizontalDirection.Right;
            else if (AnimationTarget.TranslationX < PanStartingTranslations.X)
                return HorizontalDirection.Left;
            else
                return HorizontalDirection.None;
        }

        ParentPosition getHorizontalPosition()
        {
            var startingPosition = AnimationTarget.X + PanStartingTranslations.X;
            var finishPosition = AnimationTarget.X + AnimationTarget.TranslationX;
            var distance = Math.Abs(startingPosition - finishPosition);
            var msPerPoint = milliseconds / distance;

            if (msPerPoint > 5) msPerPoint = 5;

            var overallStartingPosition = AnimationTarget.X + InitialTranslationX;
            var totalDistance = Math.Abs(overallStartingPosition - finishPosition);

            return new ParentPosition(startingPosition, finishPosition, distance, msPerPoint > 3 ? 3 : msPerPoint, totalDistance);
        }

        VerticalDirection getVerticalDirection()
        {
            if (AnimationTarget.TranslationY > PanStartingTranslations.Y)
                return VerticalDirection.Down;
            else if (AnimationTarget.TranslationY < PanStartingTranslations.Y)
                return VerticalDirection.Up;
            else
                return VerticalDirection.None;
        }

        ParentPosition getVerticalPosition()
        {
            var startingPosition = AnimationTarget.Y + PanStartingTranslations.Y;
            var finishPosition = AnimationTarget.Y + AnimationTarget.TranslationY;
            var distance = Math.Abs(startingPosition - finishPosition);
            var msPerPoint = milliseconds / distance;

            var overallStartingPosition = AnimationTarget.Y + InitialTranslationY;
            var totalDistance = Math.Abs(overallStartingPosition - finishPosition);

            return new ParentPosition(startingPosition, finishPosition, distance, msPerPoint > 3 ? 3 : msPerPoint, totalDistance);
        }

        HorizontalPanStatistics getHorizontalPanStatistics(HorizontalDirection direction, ParentPosition horizontalPosition)
        {
            if (horizontalDirection == HorizontalDirection.Left || (horizontalDirection == HorizontalDirection.None && horizontalPosition.FinishPosition < 0))
            {
                var allowedTravelDistance = Math.Abs(MinimumTranslationX - InitialTranslationX);
                var percentageComplete = horizontalPosition.TotalDistance / allowedTravelDistance;
                var distanceRemaining = allowedTravelDistance - horizontalPosition.TotalDistance;

                //Console.WriteLine($"Percentage Complete: {percentageComplete}");
                return new HorizontalPanStatistics(horizontalDirection, horizontalPosition.Distance, distanceRemaining, horizontalPosition.MillisecondsPerPoint, percentageComplete);
            }
            else if (horizontalDirection == HorizontalDirection.Right || (horizontalDirection == HorizontalDirection.None && horizontalPosition.FinishPosition > 0))
            {
                var allowedTravelDistance = Math.Abs(MaximumTranslationX - InitialTranslationX);
                var percentageComplete = horizontalPosition.TotalDistance / allowedTravelDistance;
                var distanceRemaining = allowedTravelDistance - horizontalPosition.TotalDistance;

                //Console.WriteLine($"Percentage Complete: {percentageComplete}");
                return new HorizontalPanStatistics(horizontalDirection, horizontalPosition.Distance, distanceRemaining, horizontalPosition.MillisecondsPerPoint, percentageComplete);
            }
            else
                return new HorizontalPanStatistics(direction, 0, 0, 0, 1);
        }

        VerticalPanStatistics getVerticalPanStatistics(VerticalDirection direction, ParentPosition verticalPosition)
        {
            if (direction == VerticalDirection.Up || (verticalDirection == VerticalDirection.None && verticalPosition.FinishPosition < 0))
            {
                var allowedTravelDistance = Math.Abs(MinimumTranslationY - InitialTranslationY);
                var percentageComplete = verticalPosition.TotalDistance / allowedTravelDistance;
                var distanceRemaining = allowedTravelDistance - verticalPosition.TotalDistance;

                //Console.WriteLine($"Percentage Complete: {percentageComplete}");
                return new VerticalPanStatistics(direction, verticalPosition.Distance, distanceRemaining, verticalPosition.MillisecondsPerPoint, percentageComplete);
            }
            else if (direction == VerticalDirection.Down || (verticalDirection == VerticalDirection.None && verticalPosition.FinishPosition > 0))
            {
                var allowedTravelDistance = Math.Abs(MaximumTranslationY - InitialTranslationY);
                var percentageComplete = verticalPosition.TotalDistance / allowedTravelDistance;
                var distanceRemaining = allowedTravelDistance - verticalPosition.TotalDistance;

                //Console.WriteLine($"Percentage Complete: {percentageComplete}");
                return new VerticalPanStatistics(direction, verticalPosition.Distance, distanceRemaining, verticalPosition.MillisecondsPerPoint, percentageComplete);
            }
            else
                return new VerticalPanStatistics(direction, 0, 0, 0, 1);
        }

        #endregion 
    }

    private void HandlePanRunning(PanUpdatedEventArgs e)
    {
        //Debug.WriteLine("Panning");

        if (_ignorePan)
            return;

        _previousTouchLocation = new Point(e.TotalX, e.TotalY);

        //var translations = getUpdatedTranslations(e);
        var translations = getUpdatedTranslations(e);

        var verticalDirection = getVerticalDirection(e.TotalY);
        var horizontalDirection = getHorizontalDirection(e.TotalX);

        // Let derived classes have the last say on whether or not to actually update TranslationX and TranslationY
        var shouldPan = new PanAllowed(verticalDirection != VerticalDirection.None, horizontalDirection != HorizontalDirection.None);
        shouldPan = OnPanning(verticalDirection, horizontalDirection, translations, shouldPan);


        if (!shouldPan.MoveVertical)
            translations.Y = AnimationTarget.TranslationY;

        if (!shouldPan.MoveHorizontal)
            translations.X = AnimationTarget.TranslationX;

        AnimationTarget.TranslationY = translations.Y;
        AnimationTarget.TranslationX = translations.X;


        //WriteDiagnosticMessage($"Translation: {translations.X}, {translations.Y}");

        if (shouldPan.MoveHorizontal || shouldPan.MoveVertical)
        {
            OnPanned(verticalDirection, horizontalDirection, translations);
            Panned?.Invoke(AnimationTarget, new PanEventArgs(verticalDirection, horizontalDirection, translations));
        }

        #region Local Methods

        


        // This gets the new TranslationX and TranslationY values based on where the user is touching.
        // It will also limit those values based on the bounding area 
        Point getUpdatedTranslations(PanUpdatedEventArgs e)
        {
            var panInfo = GetAnimationTargetInfo();
            var translations = getNewTranslations(e, panInfo.Scale);

            // Debug.WriteLine($"Actual Position: X={panInfo.Bounds.X}, Y={panInfo.Bounds.Y}, W={panInfo.Bounds.Width}, H={panInfo.Bounds.Height}");
            // Debug.WriteLine($"Adjusted Position: X={panInfo.AdjustedBounds.X}, Y={panInfo.AdjustedBounds.Y}, W={panInfo.AdjustedBounds.Width}, H={panInfo.AdjustedBounds.Height}");
            // Debug.WriteLine($"Translations: {AnimationTarget.TranslationX}, {AnimationTarget.TranslationY}");
            // Debug.WriteLine($"Visible Position: {panInfo.VisibleBounds.X}, {panInfo.VisibleBounds.Y}, {panInfo.VisibleBounds.Width}, {panInfo.VisibleBounds.Height}");



            // Debug.WriteLine($"Center: {panInfo.AdjustedBounds.Center.X}, {panInfo.AdjustedBounds.Center.Y}");
            //Debug.WriteLine($"Recorded Translations: {translations.X}, {translations.Y}");
            
            //Debug.WriteLine($"Adjusted Visible Position: {panInfo.AdjustedVisibleBounds.X}, {panInfo.AdjustedVisibleBounds.Y}, {panInfo.AdjustedVisibleBounds.Width}, {panInfo.AdjustedVisibleBounds.Height}");
            //Debug.WriteLine($"Visible Position: {(panInfo.AdjustedBounds.X + AnimationTarget.TranslationX) * -1}, {(panInfo.AdjustedBounds.Y + AnimationTarget.TranslationY) * -1}");
            //Debug.WriteLine($"Visible Position: {panInfo.Bounds.X + panInfo.AdjustedBounds.Width + AnimationTarget.TranslationX}, {panInfo.Bounds.Y + panInfo.AdjustedBounds.Height + AnimationTarget.TranslationY}");
            
            // var visibleX = (panInfo.Bounds.X + panInfo.AdjustedBounds.X + AnimationTarget.TranslationX) * -1;
            // var visibleY = (panInfo.Bounds.Y + panInfo.AdjustedBounds.Y + AnimationTarget.TranslationY) * -1;
            // var visibleRect = new Rect(visibleX, visibleY, panInfo.ParentBounds.Width, panInfo.ParentBounds.Height);

            //Debug.WriteLine($"Visible Rect: {visibleRect.X}, {visibleRect.Y}, {visibleRect.Width}, {visibleRect.Height}");

            //Debug.WriteLine("");

            return applyTranslationLimits(translations, panInfo);
        }

        // This converts the touch position to TranslationX and TranslationY values
        Point getNewTranslations(PanUpdatedEventArgs args, Point scale)
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                return new Point(
                    (args.TotalX * scale.X) + AnimationTarget.TranslationX,
                    (args.TotalY * scale.Y) + AnimationTarget.TranslationY);
            }
            else
            {
                return new Point(
                    (args.TotalX * scale.X) + PanStartingTranslations.X,
                    (args.TotalY * scale.Y) + PanStartingTranslations.Y);
            }
        }

        Point applyTranslationLimits(Point translations, VisualElementPosition panInfo) 
        {
            if (translations.X < MinimumTranslationX)
                translations.X = MinimumTranslationX;
            else if (translations.X > MaximumTranslationX)
                translations.X = MaximumTranslationX;

            if (translations.Y < MinimumTranslationY)
                translations.Y = MinimumTranslationY;
            else if (translations.Y > MaximumTranslationY)
                translations.Y = MaximumTranslationY;

            return translations;
        }

        // This compares the current touch position to the initial one to determine the direction of travel
        VerticalDirection getVerticalDirection(double totalY)
        {
            if (totalY < _initialTouchLocation.Y)
                return VerticalDirection.Up;

            if (totalY > _initialTouchLocation.Y)
                return VerticalDirection.Down;

            return VerticalDirection.None;
        }

        // This compares the current touch position to the initial one to determine the direction of travel
        HorizontalDirection getHorizontalDirection(double totalX)
        {
            if (totalX < _initialTouchLocation.X)
                return HorizontalDirection.Left;

            if (totalX > _initialTouchLocation.X)
                return HorizontalDirection.Right;

            return HorizontalDirection.None;
        }

        #endregion 
    }

    private void HandlePanStart(PanUpdatedEventArgs e)
    {
        // This will be used to calculate the velocity of movement
        _panStart = DateTime.Now;

        //WriteDiagnosticMessage("Pan Started");

        // Record the translation values the very first time the user pans
        if (InitialTranslationX < 0)
            InitialTranslationX = AnimationTarget.TranslationX;

        if (InitialTranslationY < 0)
            InitialTranslationY = AnimationTarget.TranslationY;

        // Record where in space the animation target started
        PanStartingTranslations = new Point(
            AnimationTarget.TranslationX, 
            AnimationTarget.TranslationY);



        // var panInfo = GetAdjustedAnimationTargetBounds();

        // WriteDiagnosticMessage("--------------------------------------------------------------");
        // WriteDiagnosticMessage($"Scale: {panInfo.Scale.X }, {panInfo.Scale.Y}");
        // WriteDiagnosticMessage($"Starting Position: {panInfo.Bounds.X}, {panInfo.Bounds.Y}");
        // WriteDiagnosticMessage($"Scaled Starting Position: {panInfo.AdjustedBounds.X}, {panInfo.AdjustedBounds.Y}");
        // WriteDiagnosticMessage($"Starting Translation: {StartingTranslationX}, {StartingTranslationY}");
        // WriteDiagnosticMessage($"Size: {panInfo.AdjustedBounds.Width}, {panInfo.AdjustedBounds.Height}");

        // Initialally, the initial and previous locations are the same because it's the first touch, so it hasn't moved yet
        _initialTouchLocation = new Point(e.TotalX, e.TotalY);
        _previousTouchLocation = new Point(e.TotalX, e.TotalY);

        // Let derived classes control whether or not to actually start panning
        _ignorePan = !OnPanStarting(_initialTouchLocation);

        if (_ignorePan)
            return;
            

        OnPanStarted();
        PanStarted?.Invoke(AnimationTarget, new EventArgs());
    }

    #endregion 

    #region Protected Virtual Methods

    protected virtual void OnPanCompleted(VerticalPanStatistics verticalStatistics, HorizontalPanStatistics horizontalStatistics) { }

    protected virtual void OnPanCancelled() { }

    protected virtual void OnPanStarted() { }

    protected virtual bool OnPanStarting(Point touchLocation) => true;

    

    protected virtual PanAllowed OnPanning(
        VerticalDirection verticalDirection, 
        HorizontalDirection 
        horizontalDirection, 
        Point newTranslationValues, 
        PanAllowed panAllowed) =>
            panAllowed;

    protected virtual void OnPanned(
        VerticalDirection verticalDirection,
        HorizontalDirection horizontalDirection,
        Point newTranslationValues)
    { }

    #endregion 

    

    protected VisualElementPosition GetAnimationTargetInfo(VisualElement? boundingView = null) 
    {
        if (boundingView == null)
            boundingView = AnimationTarget.Parent as VisualElement;

        if (boundingView == null)
            throw new Exception("Bounding view does not inherit from VisualElement");

        return AnimationTarget.GetPosition(boundingView.Bounds);
        
    }

    #region Event Handlers

    private void PanGestureRecognizer_PanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        Debug.WriteLine("Pan Updated");

        if (AnimationTarget == null)
            return;

        if (!IsEnabled)
            return;

        if (e.StatusType == GestureStatus.Started)
            HandlePanStart(e);
        else if (e.StatusType == GestureStatus.Running)
            HandlePanRunning(e);
        else if (e.StatusType == GestureStatus.Completed)
            HandlePanCompleted(e);
        else
            HandlePanCancelled(e);
    }

    #endregion 

    private void WriteDiagnosticMessage(string message)
    {
        if (WriteDiagnostics)
            Debug.WriteLine(message);
    }


}


