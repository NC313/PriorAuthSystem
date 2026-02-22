using PriorAuthSystem.Domain.Common;

namespace PriorAuthSystem.Domain.Events;

public sealed class PriorAuthSubmittedEvent : BaseDomainEvent
{
    public override string EventType => "PriorAuthSubmitted";
    public Guid PriorAuthorizationRequestId { get; }

    public PriorAuthSubmittedEvent(Guid priorAuthorizationRequestId)
    {
        PriorAuthorizationRequestId = priorAuthorizationRequestId;
    }
}
