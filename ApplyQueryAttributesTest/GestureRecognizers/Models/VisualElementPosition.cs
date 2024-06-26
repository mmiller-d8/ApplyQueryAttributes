using System.Diagnostics;

namespace D8.Maui.Components.Gestures.Models;

// If the scaled width plus X is greater than the parent width, then the image should not be able to move to the left anymore.
// If X is 0, then the image should not be able to move to the right anymore.

// If the scaled height plus Y is greater than the parent height, then the image should not be able to move up anymore.
// If Y is 0, then the image should not be able to move down anymore.


public class VisualElementPosition
{
    public Rect BoundingArea { get; private set; }
    public Rect Bounds { get; private set; }
    public Rect RelativeBounds { get; private set; }
    public Rect DisplayBounds { get; private set; }
    public Rect VisibleBounds { get; private set; }

    public Point Scale { get; private set; }
    public Point Translation { get; private set; }
    public Point VisibleAnchor { get; private set; }

    public Point MinimumTranslation { get; private set; }
    public Point MaximumTranslation { get; private set; } 


    public VisualElementPosition(VisualElement element, Rect boundingArea)
    {
        //_element = element;
        Initialize(element, boundingArea);
    }

    public VisualElementPosition(VisualElement element, VisualElement parent)
    {
        //_element = element;
        Initialize(element, parent.Bounds);
    }


    private void Initialize(VisualElement element, Rect boundingArea)
    {
        var startTime = DateTime.Now;

        BoundingArea = boundingArea;
        Bounds = GetBounds(element, boundingArea);

        Scale = GetScale(element);
        Translation = new Point(element.TranslationX, element.TranslationY);

        RelativeBounds = GetRelativeBounds(element);
        DisplayBounds = GetDisplayPosition(element, Bounds, RelativeBounds, boundingArea);
        VisibleBounds = GetVisibleBounds(DisplayBounds, boundingArea, Scale);


        

        var translationLimits = getTranslationLimits(element, DisplayBounds, boundingArea, Scale);

        MinimumTranslation = new Point(translationLimits.Minimum.X, translationLimits.Minimum.Y);
        MaximumTranslation = new Point(translationLimits.Maximum.X, translationLimits.Maximum.Y);

        VisibleAnchor = new Point(
            VisibleBounds.Center.X / Bounds.Width,
            VisibleBounds.Center.Y / Bounds.Height
        );



        // Debug.WriteLine($"ParentBounds X:{BoundingArea.X}, Y:{BoundingArea.Y}, W:{BoundingArea.Width}, H:{BoundingArea.Height}");
        // Debug.WriteLine($"Bounds X:{Bounds.X}, Y:{Bounds.Y}, W:{Bounds.Width}, H:{Bounds.Height}");
        // Debug.WriteLine($"Scale: X:{Scale.X}, Y:{Scale.Y}");
        // Debug.WriteLine($"Display X:{DisplayBounds.X}, Y:{DisplayBounds.Y}, W:{DisplayBounds.Width}, H:{DisplayBounds.Height}");
        // Debug.WriteLine($"Visible: X:{VisibleBounds.X}, Y:{VisibleBounds.Y}, W:{VisibleBounds.Width}, H:{VisibleBounds.Height}");


        // Debug.WriteLine($"Translation: X:{Translation.X}, Y:{Translation.Y}");
        // Debug.WriteLine($"Min Translation: X:{MinimumTranslation.X}, Y:{MinimumTranslation.Y} - Max Translation: X:{MaximumTranslation.X}, Y:{MaximumTranslation.Y}");
        // Debug.WriteLine($"Visible Anchor: X:{VisibleAnchor.X}, Y:{VisibleAnchor.Y}");
        //Debug.WriteLine("");

        

        //Debug.WriteLine($"Calculating position: {(DateTime.Now - startTime).TotalMilliseconds} ms");
    }

    private static Rect GetBounds(VisualElement element, Rect boundingArea) 
    {
        var actualParent = element.Parent as VisualElement;
        

        if (actualParent != null && boundingArea == actualParent.Bounds)
            return new Rect(element.X, element.Y, element.Width, element.Height);

        var x = element.X - boundingArea.X;
        var y = element.Y - boundingArea.Y;
        var width = element.Width;
        var height = element.Height;

        return new Rect(x, y, width, height);

    }

    private static Rect GetDisplayPosition(VisualElement element, Rect actualBounds, Rect relativeBounds, Rect parentBounds)
    {
        // parentBounds doesn't actually have to be the real parent.  It's really just the comparison bounds.

        var x = actualBounds.X + relativeBounds.X;
        var y = actualBounds.Y + relativeBounds.Y;

        return new Rect(x, y, relativeBounds.Width, relativeBounds.Height);
    }

    private static Rect GetRelativeBounds(VisualElement element) 
    {
        var scale = GetScale(element);
        var width = element.Width * scale.X;
        var height = element.Height * scale.Y;

        var x = element.TranslationX + (element.Width * element.AnchorX) - (width * element.AnchorX);
        var y = element.TranslationY + (element.Height * element.AnchorY) - (height * element.AnchorY);

        return new Rect(x, y, width, height);
    }

    private static (Point Minimum, Point Maximum) getTranslationLimits(VisualElement element, Rect displayBounds, Rect parentBounds, Point scale) 
    {
        // Get the minimum and maximum translation values that allow the image to be visible.
        // Basically, if the image is bigger than the screen, then the edges should not be able to move inside of the screen.
        // If the image is smaller than the screen, then the image should be able to move freely inside of the screen, but not outside of it.
        var scaledWidth = element.Width * scale.X;
        var anchorXModifier = (element.Width - scaledWidth) * element.AnchorX;
        var maxTranslationX = (element.X + scaledWidth - parentBounds.Width) * -1 - anchorXModifier;
        var minTranslationX = element.X * -1 - anchorXModifier;

        var scaledHeight = element.Height * scale.Y;
        var anchorYModifier = (element.Height - scaledHeight) * element.AnchorY;
        var maxTranslationY = (element.Y + scaledHeight - parentBounds.Height) * -1 - anchorYModifier;
        var minTranslationY = element.Y * -1 - anchorYModifier;

        

        (double Min, double Max) xValues;
        (double Min, double Max) yValues;

        if (displayBounds.Width < parentBounds.Width)
            xValues = new(minTranslationX, maxTranslationX);
        else 
            xValues = new(maxTranslationX, minTranslationX);

        if (displayBounds.Height < parentBounds.Height)
            yValues = new(minTranslationY, maxTranslationY);
        else
            yValues = new(maxTranslationY, minTranslationY);

        return (new Point(xValues.Min, yValues.Min), new Point(xValues.Max, yValues.Max));
    }

    private static Rect GetVisibleBounds(Rect displayBounds, Rect parentBounds, Point scale)
    {
        double x = 0;
        double y = 0;
        double width = 0;
        double height = 0;

        if (displayBounds.X > 0)
        {
            x = 0;

            if (displayBounds.X + displayBounds.Width < parentBounds.Width)
                width = displayBounds.Width;
            else 
                width = Math.Max(0, parentBounds.Width - displayBounds.X);
        }
        else if (displayBounds.X <= 0)
        {
            x = displayBounds.X * -1;

            if (x > displayBounds.Width)
                x = 0;

            if (displayBounds.X + displayBounds.Width > parentBounds.Width)
                width = parentBounds.Width;
            else 
                width = Math.Max(0, displayBounds.X + displayBounds.Width);
        }

        if (displayBounds.Y > 0)
        {
            y = 0;

            if (displayBounds.Y + displayBounds.Height < parentBounds.Height)
                height = displayBounds.Height;
            else 
                height = Math.Max(0, parentBounds.Height - displayBounds.Y);
        }
        else if (displayBounds.Y <= 0)
        {
            y = displayBounds.Y * -1;

            if (y > displayBounds.Height)
                y = 0;

            //height = Math.Max(0, displayBounds.Height + displayBounds.Top);

            if (displayBounds.Y + displayBounds.Height > parentBounds.Height)
                height = parentBounds.Height;
            else 
                height = Math.Max(0, displayBounds.Y + displayBounds.Height);
        }

        //return new Rect(x, y, width, height);

        // Shouldn't happen, but...
        var scaleX = scale.X == 0 ? 1 : scale.X;
        var scaleY = scale.Y == 0 ? 1 : scale.Y;

        var result = new Rect(
            x / scaleX, 
            y / scaleY, 
            width / scaleX, 
            height / scaleY);

        return result;
    }


    private static Point GetScale(VisualElement element) => 
        new Point(
            element.Scale != 1 ? element.Scale : element.ScaleX,
            element.Scale != 1 ? element.Scale : element.ScaleY);
}


public class VisualElementPositionX
{
    private VisualElement _element;

    public Rect ParentBounds { get; private set; } 
    public Rect Bounds { get; private set; }
    public Rect AdjustedBounds { get; private set; } 


    public Rect VisibleBounds { get; private set; }


    public Rect AdjustedVisibleBounds { get; private set; }

    
    public Point Scale { get; set; }
    public Point Translation { get; set; }
    public Point MinimumTranslation { get; private set; }
    public Point MaximumTranslation { get; private set; } 
    public Point VisibleAnchor { get; private set; }

    public VisualElementPositionX(VisualElement element, Rect parentBounds)
    {
        _element = element;
        Initialize(element, parentBounds);
    }

    public VisualElementPositionX(VisualElement element, VisualElement parent)
    {
        _element = element;
        Initialize(element, parent.Bounds);
    }

    



    private void Initialize(VisualElement element, Rect parentBounds)
    {
        var scale = GetScale(element);

        // Get the scaled size of the element
        var width = element.Width * scale.X;
        var height = element.Height * scale.Y;

        var x = (parentBounds.Width - width) * 0.5;
        var y = (parentBounds.Height - height) * 0.5;
        
        var adjustedBounds = new Rect(x, y, width, height);

        ParentBounds = parentBounds;
        Bounds = element.Bounds;
        AdjustedBounds = adjustedBounds;

        Update();
    }
    
    private void Update() 
    {
        Scale = GetScale(_element);
        Translation = new Point(_element.TranslationX, _element.TranslationY);
        
        var translationLimits = getTranslationLimits();
        
        MinimumTranslation = new Point(translationLimits.Minimum.X, translationLimits.Minimum.Y);
        MaximumTranslation = new Point(translationLimits.Maximum.X, translationLimits.Maximum.Y);

        VisibleBounds = getVisibleBounds();

        // Debug.WriteLine($"Visible Center: X:{VisibleBounds.Center.X}, Y:{VisibleBounds.Center.Y}");

        VisibleAnchor = new Point(
            VisibleBounds.Center.X / Bounds.Width,
            VisibleBounds.Center.Y / Bounds.Height
        );
        
        // Debug.WriteLine($"Visible Anchor: X:{Math.Round(VisibleAnchor.X, 3)}, Y:{Math.Round(VisibleAnchor.Y, 3)}");
        // Debug.WriteLine($"Actual Anchor: X:{Math.Round(_element.AnchorX, 3)}, Y:{Math.Round(_element.AnchorY, 3)}");
        // Debug.WriteLine("");
        
        (Point Minimum, Point Maximum) getTranslationLimits() 
        {
            // Get the minimum and maximum translation values that allow the image to be visible.
            // Basically, if the image is bigger than the screen, then the edges should not be able to move inside of the screen.
            // If the image is smaller than the screen, then the image should be able to move freely inside of the screen, but not outside of it.
            var scaledWidth = _element.Width * Scale.X;
            var anchorXModifier = (_element.Width - scaledWidth) * _element.AnchorX;
            var maxTranslationX = (_element.X + scaledWidth - ParentBounds.Width) * -1 - anchorXModifier;
            var minTranslationX = _element.X * -1 - anchorXModifier;

            var scaledHeight = _element.Height * Scale.Y;
            var anchorYModifier = (_element.Height - scaledHeight) * _element.AnchorY;
            var maxTranslationY = (_element.Y + scaledHeight - ParentBounds.Height) * -1 - anchorYModifier;
            var minTranslationY = _element.Y * -1 - anchorYModifier;

            //return (new Point(minTranslationX, minTranslationY), new Point(maxTranslationX, maxTranslationY));

            (double Min, double Max) xValues;
            (double Min, double Max) yValues;

            if (AdjustedBounds.Width < ParentBounds.Width)
                xValues = new(minTranslationX, maxTranslationX);
            else 
                xValues = new(maxTranslationX, minTranslationX);

            if (AdjustedBounds.Height < ParentBounds.Height)
                yValues = new(minTranslationY, maxTranslationY);
            else
                yValues = new(maxTranslationY, minTranslationY);

            return (new Point(xValues.Min, yValues.Min), new Point(xValues.Max, yValues.Max));
        }

        Rect getVisibleBounds()
        {
            var testX = AdjustedBounds.X / Scale.X;
            var testY = AdjustedBounds.Y / Scale.Y;

            var testTranslationX = Translation.X / Scale.X;
            var testTranslationY = Translation.Y / Scale.Y;

            var anchorXAdjustment = (Bounds.Width - AdjustedBounds.Width) * _element.AnchorX;
            var anchorYAdjustment = (Bounds.Height - AdjustedBounds.Height) * _element.AnchorY;

            Debug.WriteLine($"Test ::  X: {testX}, Y: {testY}");
            Debug.WriteLine($"Test Translation :: X: {testTranslationX}, Y: {testTranslationY}");


            Debug.WriteLine($"Anchor Adjustment :: X: {anchorXAdjustment}, Y: {anchorYAdjustment}");
            Debug.WriteLine($"Adjusted :: X:{AdjustedBounds.X}, Y:{AdjustedBounds.Y}, W:{AdjustedBounds.Width}, H:{AdjustedBounds.Height}");
            Debug.WriteLine($"Translation :: X:{Translation.X}, Y:{Translation.Y}");

            Debug.WriteLine("");


            var adjustedVisibleX = AdjustedBounds.X + Translation.X;
            var adjustedVisibleY = AdjustedBounds.Y + Translation.Y;

            var adjustedVisibleWidth = Math.Min(Bounds.Width, ParentBounds.Width);
            var adjustedVisibleHeight = Math.Min(Bounds.Height, ParentBounds.Height);

            var visibleX = adjustedVisibleX / Scale.X;
            var visibleY = adjustedVisibleY / Scale.Y;
            var visibleWidth = adjustedVisibleWidth / Scale.X;
            var visibleHeight = adjustedVisibleHeight / Scale.Y;

            return new Rect(visibleX, visibleY, visibleWidth, visibleHeight);
        }


        // Rect getVisibleBoundsX() 
        // {
        //     // var adjustedVisibleX = Math.Abs(AdjustedBounds.X + Translation.X);
        //     // var adjustedVisibleY = Math.Abs(AdjustedBounds.Y + Translation.Y);

        //     var adjustedVisibleX = Math.Abs(AdjustedBounds.X);
        //     var adjustedVisibleY = Math.Abs(AdjustedBounds.Y);

        //     var adjustedVisibleWidth = Math.Min(Bounds.Width, ParentBounds.Width);
        //     var adjustedVisibleHeight = Math.Min(Bounds.Height, ParentBounds.Height);

        //     var visibleX = adjustedVisibleX / Scale.X;
        //     var visibleY = adjustedVisibleY / Scale.Y;
        //     var visibleWidth = adjustedVisibleWidth / Scale.X;
        //     var visibleHeight = adjustedVisibleHeight / Scale.Y;

        //     // Debug.WriteLine($"Adjusted Visible Bounds :: X:{adjustedVisibleX}, Y:{adjustedVisibleY}, W:{adjustedVisibleWidth}, H:{adjustedVisibleHeight}");
        //     // Debug.WriteLine($"Visible Bounds :: X:{visibleX}, Y:{visibleY}, W:{visibleWidth}, H:{visibleHeight}");
            
        //     //Debug.WriteLine("");

        //     return new Rect(visibleX, visibleY, visibleWidth, visibleHeight);
        // }
    }

    private static Point GetScale(VisualElement element) => 
        new Point(
            element.Scale != 1 ? element.Scale : element.ScaleX,
            element.Scale != 1 ? element.Scale : element.ScaleY);
    
}
