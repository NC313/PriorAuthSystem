using PriorAuthSystem.Domain.Common;

namespace PriorAuthSystem.Domain.Events;

public sealed class PriorAuthAppealApprovedEvent : BaseDomainEvent
{
    public override string EventType => "PriorAuthAppealApproved";
    public Guid PriorAuthorizationRequestId { get; }
    public string ReviewerId { get; }

    public PriorAuthAppealApprovedEvent(Guid priorAuthorizationRequestId, string reviewerId)
    {
        PriorAuthorizationRequestId = priorAuthorizationRequestId;
        ReviewerId = reviewerId;
    }
}
