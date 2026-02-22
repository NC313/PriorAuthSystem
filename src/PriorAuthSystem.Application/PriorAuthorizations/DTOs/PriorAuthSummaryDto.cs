namespace PriorAuthSystem.Application.PriorAuthorizations.DTOs;

public sealed record PriorAuthSummaryDto(
    Guid Id,
    string PatientName,
    string Status,
    string ProcedureCode,
    DateTime SubmittedAt);
