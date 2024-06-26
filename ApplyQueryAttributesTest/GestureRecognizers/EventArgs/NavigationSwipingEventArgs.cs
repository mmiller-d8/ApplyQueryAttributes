using D8.Maui.Components.Gestures.Models;

namespace D8.Maui.Components.Gestures;

public class NavigationSwipingEventArgs : EventArgs
{

    public NavigationSwipingEventArgs(NavigationDirection direction, double translationX)
    {
        Direction = direction;
        TranslationX = translationX;
    }

    public NavigationDirection Direction { get; init; }
    public double TranslationX { get; init; }
}