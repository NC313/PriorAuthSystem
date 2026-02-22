using PriorAuthSystem.Domain.Common;
using PriorAuthSystem.Domain.Enums;

namespace PriorAuthSystem.Domain.Events;

public sealed class PriorAuthDeniedEvent : BaseDomainEvent
{
    public override string EventType => "PriorAuthDenied";
    public Guid PriorAuthorizationRequestId { get; }
    public string ReviewerId { get; }
    public DenialReason Reason { get; }

    public PriorAuthDeniedEvent(Guid priorAuthorizationRequestId, string reviewerId, DenialReason reason)
    {
        PriorAuthorizationRequestId = priorAuthorizationRequestId;
        ReviewerId = reviewerId;
        Reason = reason;
    }
}
