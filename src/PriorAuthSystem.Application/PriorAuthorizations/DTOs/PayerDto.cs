namespace PriorAuthSystem.Application.PriorAuthorizations.DTOs;

public sealed record PayerDto(
    Guid Id,
    string PayerName,
    string PayerId,
    int StandardResponseDays,
    string Phone,
    string Email,
    string FaxNumber);
