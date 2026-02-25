using System.Net.Http.Json;
using PriorAuthSystem.Mobile.Models;

namespace PriorAuthSystem.Mobile.Services;

public sealed class PriorAuthApiService(HttpClient httpClient)
{
    public async Task<List<PriorAuthSummaryDto>> GetPendingAsync() =>
        await httpClient.GetFromJsonAsync<List<PriorAuthSummaryDto>>("pending")
        ?? [];

    public async Task<PriorAuthDto?> GetByIdAsync(Guid id) =>
        await httpClient.GetFromJsonAsync<PriorAuthDto>($"{id}");
}
