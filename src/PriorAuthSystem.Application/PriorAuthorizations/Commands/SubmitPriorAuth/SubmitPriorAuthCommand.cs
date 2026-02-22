using MediatR;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.SubmitPriorAuth;

public sealed record SubmitPriorAuthCommand(
    Guid PatientId,
    Guid ProviderId,
    Guid PayerId,
    string IcdCode,
    string IcdDescription,
    string CptCode,
    string CptDescription,
    bool CptRequiresPriorAuth,
    string ClinicalNotes,
    string ClinicalDocumentedBy,
    string ClinicalSupportingDocumentPath,
    DateTime RequiredResponseBy) : IRequest<Guid>;
