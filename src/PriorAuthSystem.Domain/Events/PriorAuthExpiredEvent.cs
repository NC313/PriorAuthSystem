using PriorAuthSystem.Domain.Common;

namespace PriorAuthSystem.Domain.Events;

public sealed class PriorAuthExpiredEvent : BaseDomainEvent
{
    public override string EventType => "PriorAuthExpired";
    public Guid PriorAuthorizationRequestId { get; }

    public PriorAuthExpiredEvent(Guid priorAuthorizationRequestId)
    {
        PriorAuthorizationRequestId = priorAuthorizationRequestId;
    }
}
