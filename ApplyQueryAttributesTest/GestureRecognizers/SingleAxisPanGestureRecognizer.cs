using System;
using D8.Maui.Components.Gestures.Models;
using Microsoft.Maui;

namespace D8.Maui.Components.Gestures;

public class SingleAxisPanGestureRecognizer : DragGestureRecognizer
{
    private enum AxisRestriction
    {
        Horizontal,
        Vertical
    }

    private AxisRestriction? _restrictedAxis = null;

    #region Observable Properties

    public static BindableProperty AllowedAxisProperty = BindableProperty.Create(nameof(AllowedAxis), typeof(PanAxis), typeof(SingleAxisPanGestureRecognizer));
    public PanAxis? AllowedAxis
    {
        get => (PanAxis)GetValue(AllowedAxisProperty);
        set => SetValue(AllowedAxisProperty, value);
    }

    public static BindableProperty MinimumPositionProperty = BindableProperty.Create(nameof(MinimumPosition), typeof(double), typeof(SingleAxisPanGestureRecognizer), double.NaN);
    public double MinimumPosition
    {
        get => (double)GetValue(MinimumPositionProperty);
        set => SetValue(MinimumPositionProperty, value);
    }

    public static BindableProperty MaximumPositionProperty = BindableProperty.Create(nameof(MaximumPosition), typeof(double), typeof(SingleAxisPanGestureRecognizer), double.NaN);
    public double MaximumPosition
    {
        get => (double)GetValue(MaximumPositionProperty);
        set => SetValue(MaximumPositionProperty, value);
    }

    #endregion

    #region Overridden Methods

    protected override PanAllowed OnPanning(VerticalDirection verticalDirection, HorizontalDirection horizontalDirection, Point newTranslationValues, PanAllowed panAllowed)
    {
        var newTranslationX = Math.Abs(newTranslationValues.X);
        var newTranslationY = Math.Abs(newTranslationValues.Y);

        if (AllowedAxis == PanAxis.Vertical || (AllowedAxis == PanAxis.Either && _restrictedAxis == AxisRestriction.Vertical))
            return new PanAllowed(isPanAllowed(newTranslationY), false);
        else if (AllowedAxis == PanAxis.Horizontal || (AllowedAxis == PanAxis.Either && _restrictedAxis == AxisRestriction.Horizontal))
            return new PanAllowed(false, isPanAllowed(newTranslationX));
        else 
        {
            if (newTranslationX > newTranslationY)
            {
                _restrictedAxis = AxisRestriction.Horizontal;
                return new PanAllowed(false, isPanAllowed(newTranslationX));
            }
            else
            {
                _restrictedAxis = AxisRestriction.Vertical;
                return new PanAllowed(isPanAllowed(newTranslationY), false);
            }
        }


        bool isPanAllowed(double newTranslationValue)
        {
            if (MinimumPosition != default && MinimumPosition != double.NaN && MinimumPosition > newTranslationValue)
                return false;
            else if (MaximumPosition != default && MaximumPosition != double.NaN && MaximumPosition < newTranslationValue)
                return false;

            return true;

            
        }
    }

    protected override void OnPanned(VerticalDirection verticalDirection, HorizontalDirection horizontalDirection, Point newTranslationValues)
    {
        base.OnPanned(verticalDirection, horizontalDirection, newTranslationValues);

        AnimationTarget.TranslationY = newTranslationValues.Y;
        AnimationTarget.TranslationX = newTranslationValues.X;
    }

    protected override void OnPanCancelled()
    {
        _restrictedAxis = null;
        base.OnPanCancelled();
    }

    protected override void OnPanCompleted(VerticalPanStatistics verticalStatistics, HorizontalPanStatistics horizontalStatistics)
    {
        _restrictedAxis = null;
        base.OnPanCompleted(verticalStatistics, horizontalStatistics);
    }

    #endregion 

}



