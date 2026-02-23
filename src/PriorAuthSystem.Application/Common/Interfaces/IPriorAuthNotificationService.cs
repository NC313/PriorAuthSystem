namespace PriorAuthSystem.Application.Common.Interfaces;

public interface IPriorAuthNotificationService
{
    Task SendStatusUpdate(Guid requestId, string newStatus);
}
