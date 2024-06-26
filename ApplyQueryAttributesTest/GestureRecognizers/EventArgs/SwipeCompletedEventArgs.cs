using System;
namespace D8.Maui.Components.Gestures;

public class SwipeCompletedEventArgs
{
    public SwipeCompletedEventArgs(SwipeDirection swipeDirection)
    {
        Direction = swipeDirection;
    }

    public SwipeDirection Direction { get; private set; }
}

