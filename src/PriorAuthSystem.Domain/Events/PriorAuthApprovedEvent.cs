using PriorAuthSystem.Domain.Common;

namespace PriorAuthSystem.Domain.Events;

public sealed class PriorAuthApprovedEvent : BaseDomainEvent
{
    public override string EventType => "PriorAuthApproved";
    public Guid PriorAuthorizationRequestId { get; }
    public string ReviewerId { get; }

    public PriorAuthApprovedEvent(Guid priorAuthorizationRequestId, string reviewerId)
    {
        PriorAuthorizationRequestId = priorAuthorizationRequestId;
        ReviewerId = reviewerId;
    }
}
