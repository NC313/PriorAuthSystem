using PriorAuthSystem.Mobile.ViewModels;

namespace PriorAuthSystem.Mobile.Views;

public partial class DetailPage : ContentPage
{
    public DetailPage(DetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
