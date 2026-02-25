using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriorAuthSystem.Mobile.Models;
using PriorAuthSystem.Mobile.Services;

namespace PriorAuthSystem.Mobile.ViewModels;

public partial class DashboardViewModel(PriorAuthApiService apiService) : ObservableObject
{
    public ObservableCollection<PriorAuthSummaryDto> PendingAuths { get; } = [];

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task LoadPendingAsync()
    {
        ErrorMessage = null;

        try
        {
            var items = await apiService.GetPendingAsync();

            PendingAuths.Clear();
            foreach (var item in items)
                PendingAuths.Add(item);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load authorizations: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task GoToDetailAsync(Guid id)
    {
        await Shell.Current.GoToAsync($"detail?id={id}");
    }
}
