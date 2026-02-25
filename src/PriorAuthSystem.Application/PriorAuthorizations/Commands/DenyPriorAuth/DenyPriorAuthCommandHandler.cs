using MediatR;
using PriorAuthSystem.Application.Common.Interfaces;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.DenyPriorAuth;

public sealed class DenyPriorAuthCommandHandler(
    IUnitOfWork unitOfWork,
    IPriorAuthNotificationService notificationService) : IRequestHandler<DenyPriorAuthCommand>
{
    public async Task Handle(DenyPriorAuthCommand request, CancellationToken cancellationToken)
    {
        var priorAuth = await unitOfWork.PriorAuthorizationRequests.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Prior authorization request with ID '{request.RequestId}' not found.");

        priorAuth.Deny(request.ReviewerId, request.Reason, request.Notes);

        await unitOfWork.PriorAuthorizationRequests.UpdateAsync(priorAuth, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await notificationService.SendStatusUpdate(request.RequestId, priorAuth.Status.ToString());
    }
}
