namespace PriorAuthSystem.Mobile.Models;

public sealed record PriorAuthSummaryDto(
    Guid Id,
    string PatientName,
    string Status,
    DateTime RequestDate,
    string InsuranceProvider);
