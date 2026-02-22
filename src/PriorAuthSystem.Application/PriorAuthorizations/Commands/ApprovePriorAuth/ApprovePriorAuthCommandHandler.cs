using MediatR;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.ApprovePriorAuth;

public sealed class ApprovePriorAuthCommandHandler : IRequestHandler<ApprovePriorAuthCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApprovePriorAuthCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ApprovePriorAuthCommand request, CancellationToken cancellationToken)
    {
        var priorAuth = await _unitOfWork.PriorAuthorizationRequests.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Prior authorization request with ID '{request.RequestId}' not found.");

        priorAuth.Approve(request.ReviewerId, request.Notes);

        await _unitOfWork.PriorAuthorizationRequests.UpdateAsync(priorAuth, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
