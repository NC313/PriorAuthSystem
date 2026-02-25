namespace PriorAuthSystem.Application.PriorAuthorizations.DTOs;

public sealed record PriorAuthSummaryDto(
    Guid Id,
    string PatientName,
    string ProviderName,
    string PayerName,
    string CptCode,
    string IcdCode,
    string Status,
    DateTime SubmittedAt,
    DateTime RequiredResponseBy);
