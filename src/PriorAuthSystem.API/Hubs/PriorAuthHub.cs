using Microsoft.AspNetCore.SignalR;

namespace PriorAuthSystem.API.Hubs;

public class PriorAuthHub : Hub
{
    public async Task SendStatusUpdate(Guid requestId, string newStatus)
    {
        await Clients.All.SendAsync("ReceiveStatusUpdate", requestId, newStatus);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
