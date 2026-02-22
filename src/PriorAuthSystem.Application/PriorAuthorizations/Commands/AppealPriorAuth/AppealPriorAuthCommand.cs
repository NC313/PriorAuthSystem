using MediatR;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.AppealPriorAuth;

public sealed record AppealPriorAuthCommand(
    Guid RequestId,
    string AppealedBy,
    string ClinicalJustification) : IRequest;
