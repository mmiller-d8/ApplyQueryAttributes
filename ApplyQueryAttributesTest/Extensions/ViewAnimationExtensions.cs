using System;
using Microsoft.Maui.Platform;
namespace D8.Maui.Extensions;


public enum SlideDirection
{
    Left,
    Right,
    Down,
    Up
}

public static class ViewAnimationExtensions
{
    public static void CancelAnimation(this VisualElement self, string name) =>
        self.AbortAnimation(name);

    #region Color Animations

    public static Task<bool> ColorTo(
        this VisualElement self,
        Color fromColor,
        Color toColor,
        Action<Color> onApplyAnimation,
        uint animationLength = 250,
        Easing? easing = null,
        string name = nameof(ColorTo))
    {
        Func<double, Color> transform = (t) =>
            Color.FromRgba(fromColor.Red + t * (toColor.Red - fromColor.Red),
                           fromColor.Green + t * (toColor.Green - fromColor.Green),
                           fromColor.Blue + t * (toColor.Blue - fromColor.Blue),
                           fromColor.Alpha + t * (toColor.Alpha - fromColor.Alpha));
        return ColorAnimation(self, name, transform, onApplyAnimation, animationLength, easing);
    }

    private static Task<bool> ColorAnimation(
        VisualElement element,
        string name,
        Func<double, Color> transform,
        Action<Color> onApplyAnimation,
        uint animationLength,
        Easing? easing)
    {
        easing = easing ?? Easing.Linear;
        var taskCompletionSource = new TaskCompletionSource<bool>();

        if (onApplyAnimation == null)
            onApplyAnimation = (c) => { };

        element.Animate(name, transform, onApplyAnimation, 16, animationLength, easing, (v, c) => taskCompletionSource.SetResult(c));
        return taskCompletionSource.Task;
    }

    #endregion 

    #region Slide Animations

    public static void SlideIn(
        this VisualElement element,
        SlideDirection direction = SlideDirection.Left,
        uint animationLength = 250,
        Easing? easing = null,
        Action? onCompletion = null,
        string name = nameof(SlideIn))
    {
        var screenWidth = GetScreenWidth(element);
        var screenHeight = GetScreenHeight(element);
        Animation animation;

        double startingX = direction switch
        {
            SlideDirection.Left => screenWidth,
            SlideDirection.Right => screenWidth * -1,
            _ => element.TranslationX
        };

        double startingY = direction switch
        {
            SlideDirection.Up => screenHeight,
            //SlideDirection.Down => (screenHeight + element.Height) * -1,
            SlideDirection.Down => (element.Bounds.Top + element.Height) * -1,
            _ => element.TranslationY
        };

        element.TranslationX = startingX;
        element.TranslationY = startingY;


        if (direction == SlideDirection.Left || direction == SlideDirection.Right)
            animation = new Animation(v => element.TranslationX = v, startingX, 0);
        else
            animation = new Animation(v => element.TranslationY = v, startingY, 0);

        animation.Commit(element, name, 16, animationLength, easing, (b, d) =>
        {
            onCompletion?.Invoke();
        });
    }


    public static void SlideOut(
        this VisualElement element,
        SlideDirection direction = SlideDirection.Left,
        uint animationLength = 250,
        Easing? easing = null,
        Action? onCompletion = null,
        string name = nameof(SlideOut))
    {
        var screenWidth = GetScreenWidth(element);
        var screenHeight = GetScreenHeight(element);
        Animation animation;

        double targetX = direction switch
        {
            SlideDirection.Left => screenWidth * -1,
            SlideDirection.Right => screenWidth,
            _ => element.TranslationY
        };

        double targetY = direction switch
        {
            //SlideDirection.Up => (screenHeight + element.Height * -1),
            //todo: this is a hack to get the slide out to work correctly.  We need to get the actual unuseable screen height
            SlideDirection.Up => ((element.Bounds.Top + element.Height) * -1) - 60,
            SlideDirection.Down => screenHeight,
            _ => element.TranslationY 
        };

        if (direction == SlideDirection.Left || direction == SlideDirection.Right)
            animation = new Animation(v => element.TranslationX = v, element.TranslationX, targetX);
        else
            animation = new Animation(v => element.TranslationY = v, element.TranslationY, targetY);

        animation.Commit(element, name, 12, animationLength, easing, (b, d) =>
        {
            // At least on the simulator, if you navigate back, anything with a TranslationX < 0 will show
            // for a moment after the page slides out.  Same thing if you slide the page down and anything is
            // above it.
            if (direction == SlideDirection.Left)
                element.TranslationX = screenWidth;
            else if (direction == SlideDirection.Up)
                element.TranslationY = screenHeight;

            onCompletion?.Invoke();
            
        });
    }

    #endregion


    #region Screen Dimensions

    public static double GetScreenWidth(this VisualElement element)
    {
        var fullWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
        var density = DeviceDisplay.Current.MainDisplayInfo.Density;

        return fullWidth / density;
    }

    public static double GetScreenHeight(this VisualElement element)
    {
        var fullHeight = DeviceDisplay.Current.MainDisplayInfo.Height;
        var density = DeviceDisplay.Current.MainDisplayInfo.Density;

        return fullHeight / density;
    }

    #endregion 
}

