using D8.Maui.Extensions;
using D8.Maui.Components.Gestures.Models;
using System.Diagnostics;

namespace D8.Maui.Components.Gestures;



public class DragGestureRecognizer : PanGestureRecognizerBase
{
    #region Observable Properties

    public static BindableProperty BoundingViewProperty = BindableProperty.Create(nameof(BoundingView), typeof(VisualElement), typeof(DragGestureRecognizer));

    /// <summary>
    /// The visual element to use as boundaries for the dragged item
    /// </summary>
    public VisualElement BoundingView
    {
        get => (VisualElement)GetValue(BoundingViewProperty);
        set => SetValue(BoundingViewProperty, value);
    }

    public static BindableProperty KeepInBoundsProperty = BindableProperty.Create(nameof(KeepInBounds), typeof(bool), typeof(DragGestureRecognizer), true);
    public bool KeepInBounds
    {
        get => (bool)GetValue(KeepInBoundsProperty);
        set => SetValue(KeepInBoundsProperty, value);
    }


    public static BindableProperty UsePageAsBoundsProperty = BindableProperty.Create(nameof(UsePageAsBounds), typeof(bool), typeof(PanGestureRecognizerBase));
    /// <summary>
    /// Use the entire page as bounds.  This value is ignored if BoundingView is set
    /// </summary>
    public bool UsePageAsBounds
    {
        get => (bool)GetValue(UsePageAsBoundsProperty);
        set => SetValue(UsePageAsBoundsProperty, value);
    }

    #endregion 

    protected override void OnPanStarted()
    {
        Debug.WriteLine("DragGestureRecognizer.OnPanStarted");

        if (BoundingView == null) 
        {
            var page = getRootElement(AnimationTarget);
            if (UsePageAsBounds && page != null) 
                BoundingView = page;
            else 
            {
                var grandParent = Parent.Parent as VisualElement;
                if (grandParent != null)
                    BoundingView = grandParent;
                else
                    BoundingView = (VisualElement)Parent;
            }
        }
        else if (UsePageAsBounds)
        {
            if (BoundingView is not ContentPage)
                throw new Exception("Bounding view must be null or a ContentPage when UsePageAsBounds is true");
        }

        SetBoundaries();

        base.OnPanStarted();
    }

    private VisualElement getRootElement(VisualElement element)
    {
        var parent = element.Parent as VisualElement;
        if (parent == null)
            return element;

        return getRootElement(parent);
    }

    
    private void SetBoundaries()
    {
        if (BoundingView == null)
            throw new Exception("Bounding view is not set");

        Rect bounds;

        if (UsePageAsBounds)
        {
            var screenHeight = getFullPageHeight();
            bounds = new Rect(0, BoundingView.Y * -1, BoundingView.Width, screenHeight + BoundingView.Y);
        }
        else 
            bounds = BoundingView.Bounds;


        // if (BoundingView != null)
        //     bounds = BoundingView.Bounds;
        // else if (UsePageAsBounds && page != null)
        //     bounds = getFullPageBounds();
        // else if (grandParent != null)
        //     bounds = grandParent.Bounds;
        // else
        //     bounds = ((VisualElement)Parent).Bounds;

        var targetInfo = AnimationTarget.GetPosition(bounds);
        var displayBounds = targetInfo.DisplayBounds;

        MinimumTranslationX = targetInfo.MinimumTranslation.X;
        MaximumTranslationX = targetInfo.MaximumTranslation.X;

        MinimumTranslationY = targetInfo.MinimumTranslation.Y;
        MaximumTranslationY = targetInfo.MaximumTranslation.Y;

        if (displayBounds.Height <= bounds.Height && !KeepInBounds)
        {
            MinimumTranslationY = targetInfo.MinimumTranslation.Y - displayBounds.Height - Math.Abs(bounds.Top);
            MaximumTranslationY = MaximumTranslationY + displayBounds.Height;// + bounds.Bottom;
        }

        if (displayBounds.Width <= bounds.Width && !KeepInBounds)
        {
            MinimumTranslationX = targetInfo.MinimumTranslation.X - displayBounds.Width;
            MaximumTranslationX = MaximumTranslationX + displayBounds.Width;
        }

        #region Local Methods

        
        // Rect getFullPageBounds()
        // {
        //     var page = getRootElement(AnimationTarget) as ContentPage;
        //     if (page == null)
        //         return new Rect(0, 0, 0, 0);

        //     var screenHeight = getFullPageHeight();

        //     var result = new Rect(0, page.Y * -1, page.Width, screenHeight + page.Y);
        //     return result;
        // }

        double getFullPageHeight()
        {
            var fullHeight = DeviceDisplay.Current.MainDisplayInfo.Height;
            var density = DeviceDisplay.Current.MainDisplayInfo.Density;

            return fullHeight / density;
        }

        // VisualElement getRootElement(VisualElement element)
        // {
        //     var parent = element.Parent as VisualElement;
        //     if (parent == null)
        //         return element;

        //     return getRootElement(parent);
        // }

        

        #endregion 
    }
}



