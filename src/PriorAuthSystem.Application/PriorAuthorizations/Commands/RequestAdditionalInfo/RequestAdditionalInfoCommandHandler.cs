using MediatR;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.RequestAdditionalInfo;

public sealed class RequestAdditionalInfoCommandHandler : IRequestHandler<RequestAdditionalInfoCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestAdditionalInfoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RequestAdditionalInfoCommand request, CancellationToken cancellationToken)
    {
        var priorAuth = await _unitOfWork.PriorAuthorizationRequests.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Prior authorization request with ID '{request.RequestId}' not found.");

        priorAuth.RequestAdditionalInfo(request.RequestedBy, request.Notes);

        await _unitOfWork.PriorAuthorizationRequests.UpdateAsync(priorAuth, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
