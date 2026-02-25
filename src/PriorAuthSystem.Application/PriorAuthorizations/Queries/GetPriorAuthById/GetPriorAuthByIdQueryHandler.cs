using MediatR;
using PriorAuthSystem.Application.PriorAuthorizations.DTOs;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Exceptions;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.Application.PriorAuthorizations.Queries.GetPriorAuthById;

public sealed class GetPriorAuthByIdQueryHandler : IRequestHandler<GetPriorAuthByIdQuery, PriorAuthDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPriorAuthByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PriorAuthDto> Handle(GetPriorAuthByIdQuery request, CancellationToken cancellationToken)
    {
        var priorAuth = await _unitOfWork.PriorAuthorizationRequests.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new PriorAuthNotFoundException(request.Id);

        return MapToDto(priorAuth);
    }

    private static PriorAuthDto MapToDto(PriorAuthorizationRequest pa) =>
        new(
            pa.Id,
            pa.Patient.FullName,
            pa.Provider.FullName,
            pa.Payer.PayerName,
            pa.IcdCode.Code,
            pa.IcdCode.Description,
            pa.CptCode.Code,
            pa.CptCode.Description,
            pa.Status.ToString(),
            pa.ClinicalJustification.Notes,
            pa.CreatedAt,
            pa.RequiredResponseBy,
            pa.StatusTransitions.Select(MapTransitionToDto).ToList());

    private static StatusTransitionDto MapTransitionToDto(StatusTransition st) =>
        new(
            st.FromStatus.ToString(),
            st.ToStatus.ToString(),
            st.TransitionedBy,
            st.Notes,
            st.TransitionedAt);
}
