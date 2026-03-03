namespace PriorAuthSystem.Application.PriorAuthorizations.DTOs;

public sealed record PriorAuthDto(
    Guid Id,
    PriorAuthPatientDto Patient,
    PriorAuthProviderDto Provider,
    PriorAuthPayerDto Payer,
    string IcdCode,
    string IcdDescription,
    string CptCode,
    string CptDescription,
    string Status,
    string ClinicalJustification,
    DateTime SubmittedAt,
    DateTime RequiredResponseBy,
    IReadOnlyList<StatusTransitionDto> StatusTransitions);

public sealed record PriorAuthPatientDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    DateTime DateOfBirth,
    string MemberId,
    string Email,
    string Phone);

public sealed record PriorAuthProviderDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Npi,
    string Specialty,
    string Email);

public sealed record PriorAuthPayerDto(
    Guid Id,
    string Name,
    string PayerId,
    int StandardResponseDays,
    string Phone,
    string Email);
