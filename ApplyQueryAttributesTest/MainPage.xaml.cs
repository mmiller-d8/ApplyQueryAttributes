using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using D8.Maui.Components.Gestures;

namespace ApplyQueryAttributesTest;

public partial class MainPage : ContentPage
{
	private int _initialZIndex;
	
	private Dictionary<string, object> _routeParameters = new()
	{
		{ "MyParam", "MyParamValue" }
	};

	public MainPage()
	{
		InitializeComponent();
	}

    private void TapGestureRecognizer_Tapped(object? sender, TappedEventArgs e)
    {
		if (sender is not VisualElement visualElement)
            return;

        if (IsZIndexSetEnabled)
		{
			Debug.WriteLine("Initial Z Index: " + visualElement.ZIndex);
			_initialZIndex = visualElement.ZIndex;
			visualElement.ZIndex = 1000;
		}
    }

    public static readonly BindableProperty IsZInexSetEnabledProperty = BindableProperty.Create(nameof(IsZIndexSetEnabled), typeof(bool), typeof(MainPage), false);
	public bool IsZIndexSetEnabled 
	{
		get => (bool)GetValue(IsZInexSetEnabledProperty);
		set => SetValue(IsZInexSetEnabledProperty, value);
	}

	private void DragGestureRecognizer_PanCompleted(object? sender, PanCompletedEventArgs e)
    {
        if (sender is not VisualElement visualElement)
            return;


		if (IsZIndexSetEnabled)
        	visualElement.ZIndex = _initialZIndex;
    }

    private void DragGestureRecognizer_PanStarted(object? sender, EventArgs e)
    {
        if (sender is not VisualElement visualElement)
            return;

        

		// if (IsZIndexSetEnabled)
		// {
		// 	Debug.WriteLine("Initial Z Index: " + visualElement.ZIndex);
		// 	_initialZIndex = visualElement.ZIndex;
		// 	visualElement.ZIndex = 1000;
		// }
        
    }
    
}


