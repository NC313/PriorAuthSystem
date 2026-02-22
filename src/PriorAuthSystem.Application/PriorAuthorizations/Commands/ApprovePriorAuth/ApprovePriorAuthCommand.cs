using MediatR;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.ApprovePriorAuth;

public sealed record ApprovePriorAuthCommand(
    Guid RequestId,
    string ReviewerId,
    string Notes) : IRequest;
