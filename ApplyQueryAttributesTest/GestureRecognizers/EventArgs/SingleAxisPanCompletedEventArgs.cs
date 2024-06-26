using System;
namespace D8.Maui.Components.Gestures;

public class SingleAxisPanCompletedEventArgs
{
    public SingleAxisPanCompletedEventArgs() { }

    public SingleAxisPanCompletedEventArgs(
        double pannedDistance,
        double distanceRemaining,
        double millisecondsPerUnit,
        SwipeDirection direction)
    {
        PannedDistance = pannedDistance;
        DistanceRemaining = distanceRemaining;
        MillisecondsPerUnit = millisecondsPerUnit;
        Direction = direction;

        var estimatedTime = (millisecondsPerUnit * distanceRemaining);
        if (estimatedTime < 0)
            estimatedTime = 0;

        TimeToCompletion = (uint)estimatedTime;
    }

    public double PannedDistance { get; init; }
    public double DistanceRemaining { get; init; }
    public double MillisecondsPerUnit { get; init; }
    public SwipeDirection Direction { get; init; }
    public uint TimeToCompletion { get; private set; }
}
