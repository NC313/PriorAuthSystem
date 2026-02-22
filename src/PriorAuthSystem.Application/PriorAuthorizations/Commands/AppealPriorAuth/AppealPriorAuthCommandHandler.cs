using MediatR;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.AppealPriorAuth;

public sealed class AppealPriorAuthCommandHandler : IRequestHandler<AppealPriorAuthCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public AppealPriorAuthCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AppealPriorAuthCommand request, CancellationToken cancellationToken)
    {
        var priorAuth = await _unitOfWork.PriorAuthorizationRequests.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Prior authorization request with ID '{request.RequestId}' not found.");

        priorAuth.Appeal(request.AppealedBy, request.ClinicalJustification);

        await _unitOfWork.PriorAuthorizationRequests.UpdateAsync(priorAuth, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
