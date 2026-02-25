using Microsoft.AspNetCore.SignalR;
using PriorAuthSystem.API.Hubs;
using PriorAuthSystem.Application.Common.Interfaces;

namespace PriorAuthSystem.API.Services;

public sealed class PriorAuthNotificationService(IHubContext<PriorAuthHub> hubContext) : IPriorAuthNotificationService
{
    public async Task SendStatusUpdate(Guid requestId, string newStatus)
    {
        await hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", requestId, newStatus);
    }
}
