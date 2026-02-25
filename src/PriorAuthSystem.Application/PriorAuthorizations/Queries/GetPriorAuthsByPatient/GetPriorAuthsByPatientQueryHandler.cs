using MediatR;
using PriorAuthSystem.Application.PriorAuthorizations.DTOs;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.Application.PriorAuthorizations.Queries.GetPriorAuthsByPatient;

public sealed class GetPriorAuthsByPatientQueryHandler : IRequestHandler<GetPriorAuthsByPatientQuery, IList<PriorAuthSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPriorAuthsByPatientQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IList<PriorAuthSummaryDto>> Handle(GetPriorAuthsByPatientQuery request, CancellationToken cancellationToken)
    {
        var results = await _unitOfWork.PriorAuthorizationRequests.GetByPatientIdAsync(request.PatientId, cancellationToken);

        return results.Select(MapToSummaryDto).ToList();
    }

    private static PriorAuthSummaryDto MapToSummaryDto(PriorAuthorizationRequest pa) =>
        new(
            pa.Id,
            pa.Patient.FullName,
            pa.Provider.FullName,
            pa.Payer.PayerName,
            pa.CptCode.Code,
            pa.IcdCode.Code,
            pa.Status.ToString(),
            pa.CreatedAt,
            pa.RequiredResponseBy);
}
