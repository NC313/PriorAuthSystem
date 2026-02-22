namespace PriorAuthSystem.Application.PriorAuthorizations.DTOs;

public sealed record PatientDto(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string MemberId,
    string InsurancePlanId,
    string Phone,
    string Email,
    string FaxNumber);
