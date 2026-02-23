using MediatR;
using PriorAuthSystem.Application.Common.Interfaces;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.ApprovePriorAuth;

public sealed class ApprovePriorAuthCommandHandler(
    IUnitOfWork unitOfWork,
    IPriorAuthNotificationService notificationService) : IRequestHandler<ApprovePriorAuthCommand>
{
    public async Task Handle(ApprovePriorAuthCommand request, CancellationToken cancellationToken)
    {
        var priorAuth = await unitOfWork.PriorAuthorizationRequests.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Prior authorization request with ID '{request.RequestId}' not found.");

        priorAuth.Approve(request.ReviewerId, request.Notes);

        await unitOfWork.PriorAuthorizationRequests.UpdateAsync(priorAuth, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await notificationService.SendStatusUpdate(request.RequestId, priorAuth.Status.ToString());
    }
}
