using System;
namespace D8.Maui.Components.Gestures;

public class SingleAxisPanUpdatedEventArgs : PanUpdatedEventArgs
{
    public SingleAxisPanUpdatedEventArgs(
        GestureStatus type,
        int gestureId,
        PanAxis? panAxis = null,
        SwipeDirection? swipeDirection = null,
        Point? newCoordinates = null)
            : base(type, gestureId)
    {
        NewCoordinates = newCoordinates;
        PanAxis = panAxis;
        SwipeDirection = swipeDirection;
    }

    public Point? NewCoordinates { get; private set; }
    public PanAxis? PanAxis { get; private set; }
    public SwipeDirection? SwipeDirection { get; private set; }
}

