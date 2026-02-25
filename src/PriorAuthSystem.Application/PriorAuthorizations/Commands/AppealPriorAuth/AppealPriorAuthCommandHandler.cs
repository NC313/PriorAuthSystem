using MediatR;
using PriorAuthSystem.Application.Common.Interfaces;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.AppealPriorAuth;

public sealed class AppealPriorAuthCommandHandler(
    IUnitOfWork unitOfWork,
    IPriorAuthNotificationService notificationService) : IRequestHandler<AppealPriorAuthCommand>
{
    public async Task Handle(AppealPriorAuthCommand request, CancellationToken cancellationToken)
    {
        var priorAuth = await unitOfWork.PriorAuthorizationRequests.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Prior authorization request with ID '{request.RequestId}' not found.");

        priorAuth.Appeal(request.AppealedBy, request.ClinicalJustification);

        await unitOfWork.PriorAuthorizationRequests.UpdateAsync(priorAuth, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await notificationService.SendStatusUpdate(request.RequestId, priorAuth.Status.ToString());
    }
}
