using MediatR;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.RequestAdditionalInfo;

public sealed record RequestAdditionalInfoCommand(
    Guid RequestId,
    string RequestedBy,
    string Notes) : IRequest;
