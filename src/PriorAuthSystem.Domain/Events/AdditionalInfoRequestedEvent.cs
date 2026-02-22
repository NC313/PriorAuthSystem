using PriorAuthSystem.Domain.Common;

namespace PriorAuthSystem.Domain.Events;

public sealed class AdditionalInfoRequestedEvent : BaseDomainEvent
{
    public override string EventType => "AdditionalInfoRequested";
    public Guid PriorAuthorizationRequestId { get; }
    public string RequestedBy { get; }

    public AdditionalInfoRequestedEvent(Guid priorAuthorizationRequestId, string requestedBy)
    {
        PriorAuthorizationRequestId = priorAuthorizationRequestId;
        RequestedBy = requestedBy;
    }
}
