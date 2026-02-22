using PriorAuthSystem.Domain.Common;
using PriorAuthSystem.Domain.Enums;

namespace PriorAuthSystem.Domain.Entities;

public class StatusTransition : BaseEntity
{
    public Guid PriorAuthorizationRequestId { get; private set; }
    public PriorAuthStatus FromStatus { get; private set; }
    public PriorAuthStatus ToStatus { get; private set; }
    public string TransitionedBy { get; private set; }
    public string Notes { get; private set; }
    public DateTime TransitionedAt { get; private set; }

    private StatusTransition() { }

    public StatusTransition(
        Guid priorAuthorizationRequestId,
        PriorAuthStatus fromStatus,
        PriorAuthStatus toStatus,
        string transitionedBy,
        string notes = "")
    {
        if (priorAuthorizationRequestId == Guid.Empty)
            throw new ArgumentException("Prior authorization request ID cannot be empty.", nameof(priorAuthorizationRequestId));

        if (string.IsNullOrWhiteSpace(transitionedBy))
            throw new ArgumentException("TransitionedBy cannot be empty.", nameof(transitionedBy));

        PriorAuthorizationRequestId = priorAuthorizationRequestId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        TransitionedBy = transitionedBy.Trim();
        Notes = notes?.Trim() ?? string.Empty;
        TransitionedAt = DateTime.UtcNow;
    }
}