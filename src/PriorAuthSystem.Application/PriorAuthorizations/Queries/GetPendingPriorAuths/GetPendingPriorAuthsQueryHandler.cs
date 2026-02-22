using MediatR;
using PriorAuthSystem.Application.PriorAuthorizations.DTOs;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.Application.PriorAuthorizations.Queries.GetPendingPriorAuths;

public sealed class GetPendingPriorAuthsQueryHandler : IRequestHandler<GetPendingPriorAuthsQuery, IList<PriorAuthSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingPriorAuthsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IList<PriorAuthSummaryDto>> Handle(GetPendingPriorAuthsQuery request, CancellationToken cancellationToken)
    {
        var results = await _unitOfWork.PriorAuthorizationRequests.GetPendingAsync(cancellationToken);

        return results.Select(MapToSummaryDto).ToList();
    }

    private static PriorAuthSummaryDto MapToSummaryDto(PriorAuthorizationRequest pa) =>
        new(
            pa.Id,
            pa.Patient.FullName,
            pa.Status.ToString(),
            pa.CptCode.Code,
            pa.CreatedAt);
}
