using System;
using System.Diagnostics;
using D8.Maui.Components.Gestures.Models;

namespace D8.Maui.Components.Gestures;

public class ScrollGestureRecognizer : DragGestureRecognizer
{

    protected override void OnPanStarted()
    {
        Debug.WriteLine("ScrollGestureRecognizer.OnPanStarted");

        base.OnPanStarted();
    }

    protected override PanAllowed OnPanning(VerticalDirection verticalDirection, HorizontalDirection horizontalDirection, Point newTranslationValues, PanAllowed panAllowed)
    {
        Debug.WriteLine("ScrollGestureRecognizer.OnPanning");

        var target = GetAnimationTargetInfo();

        bool allowVertical = verticalDirection != VerticalDirection.None && target.DisplayBounds.Height > BoundingView.Height && panAllowed.MoveVertical;
        bool allowHorizontal = horizontalDirection != HorizontalDirection.None && target.DisplayBounds.Width > BoundingView.Width && panAllowed.MoveHorizontal;

        //Debug.WriteLine($"AllowVertical: {allowVertical}, AllowHorizontal: {allowHorizontal}");

        return base.OnPanning(verticalDirection, horizontalDirection, newTranslationValues, new PanAllowed(allowVertical, allowHorizontal));
    }

}


