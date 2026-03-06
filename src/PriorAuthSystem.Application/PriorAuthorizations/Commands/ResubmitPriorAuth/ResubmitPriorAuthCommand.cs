using MediatR;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.ResubmitPriorAuth;

public sealed record ResubmitPriorAuthCommand(Guid RequestId, string ResubmittedBy) : IRequest;
