using MediatR;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.DenyPriorAuth;

public sealed class DenyPriorAuthCommandHandler : IRequestHandler<DenyPriorAuthCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DenyPriorAuthCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DenyPriorAuthCommand request, CancellationToken cancellationToken)
    {
        var priorAuth = await _unitOfWork.PriorAuthorizationRequests.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Prior authorization request with ID '{request.RequestId}' not found.");

        priorAuth.Deny(request.ReviewerId, request.Reason, request.Notes);

        await _unitOfWork.PriorAuthorizationRequests.UpdateAsync(priorAuth, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
