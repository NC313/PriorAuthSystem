namespace PriorAuthSystem.Mobile.Models;

public sealed record PriorAuthDto(
    Guid Id,
    string PatientName,
    string PatientDateOfBirth,
    string InsuranceProvider,
    string InsuranceMemberId,
    string DiagnosisCode,
    string DiagnosisDescription,
    string ProcedureCode,
    string ProcedureDescription,
    string Status,
    DateTime RequestDate,
    DateTime? DecisionDate,
    string? Notes);
