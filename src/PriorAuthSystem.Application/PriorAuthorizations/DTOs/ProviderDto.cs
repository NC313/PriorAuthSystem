namespace PriorAuthSystem.Application.PriorAuthorizations.DTOs;

public sealed record ProviderDto(
    Guid Id,
    string FirstName,
    string LastName,
    string NPI,
    string Specialty,
    string OrganizationName,
    string Phone,
    string Email,
    string FaxNumber);
