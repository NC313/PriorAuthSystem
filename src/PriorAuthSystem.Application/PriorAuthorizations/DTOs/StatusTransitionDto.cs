namespace PriorAuthSystem.Application.PriorAuthorizations.DTOs;

public sealed record StatusTransitionDto(
    string FromStatus,
    string ToStatus,
    string TransitionedBy,
    string Notes,
    DateTime TransitionedAt);
