using System;
namespace D8.Maui.Components.Gestures.Models;

public record VerticalPanStatistics(
    VerticalDirection Direction,
    double Distance,
    double DistanceRemaining,
    double MillisecondsPerUnit,
    double PercentageTowardBoundary) : IPanStatistics;

