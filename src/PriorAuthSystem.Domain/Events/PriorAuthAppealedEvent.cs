using PriorAuthSystem.Domain.Common;

namespace PriorAuthSystem.Domain.Events;

public sealed class PriorAuthAppealedEvent : BaseDomainEvent
{
    public override string EventType => "PriorAuthAppealed";
    public Guid PriorAuthorizationRequestId { get; }
    public string AppealedBy { get; }

    public PriorAuthAppealedEvent(Guid priorAuthorizationRequestId, string appealedBy)
    {
        PriorAuthorizationRequestId = priorAuthorizationRequestId;
        AppealedBy = appealedBy;
    }
}
