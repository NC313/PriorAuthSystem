namespace PriorAuthSystem.Application.PriorAuthorizations.DTOs;

public sealed record PriorAuthDto(
    Guid Id,
    string PatientName,
    string ProviderName,
    string PayerName,
    string IcdCode,
    string IcdDescription,
    string CptCode,
    string CptDescription,
    string Status,
    string ClinicalJustification,
    DateTime SubmittedAt,
    DateTime RequiredResponseBy,
    IReadOnlyList<StatusTransitionDto> StatusTransitions);
