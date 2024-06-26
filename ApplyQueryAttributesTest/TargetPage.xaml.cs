namespace ApplyQueryAttributesTest;

public partial class TargetPage : ContentPage
{
	private readonly TargetViewModel _viewModel;

	public TargetPage(TargetViewModel viewModel)
	{
		InitializeComponent();

		_viewModel = viewModel;
		BindingContext = _viewModel;
	}
}
