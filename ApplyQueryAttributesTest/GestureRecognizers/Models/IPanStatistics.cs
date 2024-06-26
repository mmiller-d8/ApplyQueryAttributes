using System;
namespace D8.Maui.Components.Gestures.Models;

public interface IPanStatistics
{
    double Distance { get; }
    double DistanceRemaining { get; }
    double MillisecondsPerUnit { get; }
    double PercentageTowardBoundary { get; }
}

