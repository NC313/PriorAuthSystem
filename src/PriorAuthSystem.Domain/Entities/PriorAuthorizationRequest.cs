using PriorAuthSystem.Domain.Common;
using PriorAuthSystem.Domain.Enums;
using PriorAuthSystem.Domain.Events;
using PriorAuthSystem.Domain.Exceptions;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Domain.Entities;

public class PriorAuthorizationRequest : BaseEntity
{
    public Patient Patient { get; private set; }
    public Provider Provider { get; private set; }
    public Payer Payer { get; private set; }
    public IcdCode IcdCode { get; private set; }
    public CptCode CptCode { get; private set; }
    public ClinicalJustification ClinicalJustification { get; private set; }
    public PriorAuthStatus Status { get; private set; }
    public DateTime RequiredResponseBy { get; private set; }

    private readonly List<StatusTransition> _statusTransitions = new();
    public IReadOnlyList<StatusTransition> StatusTransitions => _statusTransitions.AsReadOnly();

    private PriorAuthorizationRequest() { }

    public PriorAuthorizationRequest(
        Patient patient,
        Provider provider,
        Payer payer,
        IcdCode icdCode,
        CptCode cptCode,
        ClinicalJustification clinicalJustification,
        DateTime requiredResponseBy)
    {
        Patient = patient ?? throw new ArgumentNullException(nameof(patient));
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Payer = payer ?? throw new ArgumentNullException(nameof(payer));
        IcdCode = icdCode ?? throw new ArgumentNullException(nameof(icdCode));
        CptCode = cptCode ?? throw new ArgumentNullException(nameof(cptCode));
        ClinicalJustification = clinicalJustification ?? throw new ArgumentNullException(nameof(clinicalJustification));


        RequiredResponseBy = requiredResponseBy;
        Status = PriorAuthStatus.Draft;
    }

    public void Submit()
    {
        EnsureValidTransition(PriorAuthStatus.Submitted,
            PriorAuthStatus.Draft, PriorAuthStatus.AdditionalInfoRequested);

        TransitionTo(PriorAuthStatus.Submitted, "System", "Request submitted for review.");
        AddDomainEvent(new PriorAuthSubmittedEvent(Id));
    }

    public void Approve(string reviewerId, string notes)
    {
        EnsureValidTransition(PriorAuthStatus.Approved,
            PriorAuthStatus.Submitted, PriorAuthStatus.UnderReview);

        TransitionTo(PriorAuthStatus.Approved, reviewerId, notes);
        AddDomainEvent(new PriorAuthApprovedEvent(Id, reviewerId));
    }

    public void Deny(string reviewerId, DenialReason reason, string notes)
    {
        EnsureValidTransition(PriorAuthStatus.Denied,
            PriorAuthStatus.Submitted, PriorAuthStatus.UnderReview);

        TransitionTo(PriorAuthStatus.Denied, reviewerId, $"[{reason}] {notes}");
        AddDomainEvent(new PriorAuthDeniedEvent(Id, reviewerId, reason));
    }

    public void RequestAdditionalInfo(string requestedBy, string notes)
    {
        EnsureValidTransition(PriorAuthStatus.AdditionalInfoRequested,
            PriorAuthStatus.Submitted, PriorAuthStatus.UnderReview);

        TransitionTo(PriorAuthStatus.AdditionalInfoRequested, requestedBy, notes);
        AddDomainEvent(new AdditionalInfoRequestedEvent(Id, requestedBy));
    }

    public void Appeal(string appealedBy, string clinicalJustification)
    {
        EnsureValidTransition(PriorAuthStatus.Appealed,
            PriorAuthStatus.Denied);

        TransitionTo(PriorAuthStatus.Appealed, appealedBy, clinicalJustification);
        AddDomainEvent(new PriorAuthAppealedEvent(Id, appealedBy));
    }

    public void AppealApprove(string reviewerId, string notes)
    {
        EnsureValidTransition(PriorAuthStatus.AppealApproved,
            PriorAuthStatus.Appealed);

        TransitionTo(PriorAuthStatus.AppealApproved, reviewerId, notes);
        AddDomainEvent(new PriorAuthAppealApprovedEvent(Id, reviewerId));
    }

    public void AppealDeny(string reviewerId, string notes)
    {
        EnsureValidTransition(PriorAuthStatus.AppealDenied,
            PriorAuthStatus.Appealed);

        TransitionTo(PriorAuthStatus.AppealDenied, reviewerId, notes);
        AddDomainEvent(new PriorAuthAppealDeniedEvent(Id, reviewerId));
    }

    public void ExpireIfOverdue()
    {
        if (DateTime.UtcNow <= RequiredResponseBy)
            return;

        EnsureValidTransition(PriorAuthStatus.Expired,
            PriorAuthStatus.Submitted, PriorAuthStatus.UnderReview, PriorAuthStatus.AdditionalInfoRequested);

        TransitionTo(PriorAuthStatus.Expired, "System", "Request expired due to exceeding required response date.");
        AddDomainEvent(new PriorAuthExpiredEvent(Id));
    }

    private void EnsureValidTransition(PriorAuthStatus toStatus, params PriorAuthStatus[] validFromStatuses)
    {
        if (!validFromStatuses.Contains(Status))
            throw new InvalidStatusTransitionException(Status, toStatus);
    }

    private void TransitionTo(PriorAuthStatus newStatus, string transitionedBy, string notes)
    {
        var transition = new StatusTransition(Id, Status, newStatus, transitionedBy, notes);
        _statusTransitions.Add(transition);
        Status = newStatus;
        SetUpdated();
    }
}
