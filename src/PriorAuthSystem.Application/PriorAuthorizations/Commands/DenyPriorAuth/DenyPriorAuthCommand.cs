using MediatR;
using PriorAuthSystem.Domain.Enums;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.DenyPriorAuth;

public sealed record DenyPriorAuthCommand(
    Guid RequestId,
    string ReviewerId,
    DenialReason Reason,
    string Notes) : IRequest;
