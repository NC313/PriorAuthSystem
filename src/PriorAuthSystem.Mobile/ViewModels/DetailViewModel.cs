using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriorAuthSystem.Mobile.Models;
using PriorAuthSystem.Mobile.Services;

namespace PriorAuthSystem.Mobile.ViewModels;

[QueryProperty(nameof(Id), "id")]
public partial class DetailViewModel(PriorAuthApiService apiService) : ObservableObject
{
    [ObservableProperty]
    private string? _id;

    [ObservableProperty]
    private PriorAuthDto? _auth;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    partial void OnIdChanged(string? value)
    {
        if (Guid.TryParse(value, out _))
            LoadAuthCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAuthAsync()
    {
        if (!Guid.TryParse(Id, out var guid))
            return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            Auth = await apiService.GetByIdAsync(guid);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load details: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
