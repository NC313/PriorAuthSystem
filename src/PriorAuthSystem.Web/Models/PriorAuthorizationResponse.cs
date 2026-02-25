namespace PriorAuthSystem.Web.Models;

public sealed record PriorAuthorizationResponse(
    Guid Id,
    string PatientName,
    string Status,
    DateTime SubmittedDate);
