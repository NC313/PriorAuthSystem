using PriorAuthSystem.Mobile.Views;

namespace PriorAuthSystem.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("detail", typeof(DetailPage));
	}
}
