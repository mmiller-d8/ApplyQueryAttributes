using System;
using D8.Maui.Components.Gestures.Models;

namespace D8.Maui.Components.Gestures;

public class NavigationSwipeEventArgs : EventArgs
{
    public NavigationSwipeEventArgs(NavigationDirection direction, bool isAnimationComplete)
    {
        Direction = direction;
        IsAnimationComplete = false;
    }

    public NavigationDirection Direction { get; init; }
    public bool IsAnimationComplete { get; init; }
}

