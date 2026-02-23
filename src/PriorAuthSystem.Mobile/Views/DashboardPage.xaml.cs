using PriorAuthSystem.Mobile.ViewModels;

namespace PriorAuthSystem.Mobile.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DashboardViewModel vm)
            vm.LoadPendingCommand.Execute(null);
    }
}
