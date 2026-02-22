using PriorAuthSystem.Domain.Common;

namespace PriorAuthSystem.Domain.Events;

public sealed class PriorAuthAppealDeniedEvent : BaseDomainEvent
{
    public override string EventType => "PriorAuthAppealDenied";
    public Guid PriorAuthorizationRequestId { get; }
    public string ReviewerId { get; }

    public PriorAuthAppealDeniedEvent(Guid priorAuthorizationRequestId, string reviewerId)
    {
        PriorAuthorizationRequestId = priorAuthorizationRequestId;
        ReviewerId = reviewerId;
    }
}
