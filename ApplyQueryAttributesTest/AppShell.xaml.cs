namespace ApplyQueryAttributesTest;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(GlobalRoutes.TargetPage, typeof(TargetPage));

    }
}

