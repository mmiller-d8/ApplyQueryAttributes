using D8.Maui.Components.Gestures.Models;

namespace D8.Maui.Extensions;

public static class VisualElementExtensions
{

    public static ContentPage? GetContentPage(this VisualElement self) =>
        GetRootElementInternal(self) as ContentPage;

    public static VisualElement GetRootElement(this VisualElement self)
    {
        var parent = GetRootElementInternal(self);

        if (parent is ContentPage)
        {
            var page = (ContentPage)parent;
            return page.Content;
        }
        else
            return parent;
    }

    public static Layout? GetFirstLayout(this VisualElement self)
    {
        // In case there is no layout in the hierarchy
        var parent = self.Parent;
        if (parent == null)
            return self as Layout;

        var layout = self.Parent as Layout;
        if (layout == null)
            return GetFirstLayout(self);

        return layout;
    }

    public static Size GetSize(this View self, bool includeMargin)
    {
        if (!includeMargin)
            return new Size(self.Width, self.Height);

        var width =
            self.Width +
            self.Margin.Left +
            self.Margin.Right;

        var height =
            self.Height +
            self.Margin.Top +
            self.Margin.Bottom;

        return new Size(width, height);
    }

    public static Size GetAvailableSpace(this Layout self)
    {
        var width =
            self.Width -
            self.Padding.Left -
            self.Padding.Right;

        var height =
            self.Height -
            self.Padding.Top -
            self.Padding.Bottom;

        return new Size(width, height);
    }

    public static IEnumerable<VisualElement> GetAncestors(this VisualElement self)
    {
        VisualElement? element = self;

        while (element != null)
        {
            yield return element;
            element = element.Parent as VisualElement;
        }
    }

    public static Point GetAbsolutePosition(this VisualElement self)
    {
        var ancestors = self.GetAncestors();
        var x = ancestors.Sum(ancestor => ancestor.X);
        var y = ancestors.Sum(ancestor => ancestor.Y);

        return new Point(x, y);
    }


    private static VisualElement GetRootElementInternal(VisualElement element)
    {
        var parent = element.Parent as VisualElement;
        if (parent == null)
            return element;

        return GetRootElementInternal(parent);
    }

    public static VisualElementPosition GetPosition(this VisualElement element, VisualElement parent) =>
        element.GetPosition(parent.Bounds);
    
    public static VisualElementPosition GetPosition(this VisualElement element, Rect parentBounds) =>
        new VisualElementPosition(element, parentBounds);

}

//public record VisualElementPosition(Rect ParentBounds, Rect Bounds, Rect AdjustedBounds, Point Scale, Point Translation, Point MinimumTranslation, Point MaximumTranslation);