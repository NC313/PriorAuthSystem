using MediatR;
using PriorAuthSystem.Application.Common.Interfaces;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.ResubmitPriorAuth;

public sealed class ResubmitPriorAuthCommandHandler(
    IUnitOfWork unitOfWork,
    IPriorAuthNotificationService notificationService) : IRequestHandler<ResubmitPriorAuthCommand>
{
    public async Task Handle(ResubmitPriorAuthCommand request, CancellationToken cancellationToken)
    {
        var priorAuth = await unitOfWork.PriorAuthorizationRequests.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Prior authorization request with ID '{request.RequestId}' not found.");

        priorAuth.Submit();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await notificationService.SendStatusUpdate(request.RequestId, priorAuth.Status.ToString());
    }
}
