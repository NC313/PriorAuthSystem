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
            new PriorAuthPatientDto(
                pa.Patient.Id,
                pa.Patient.FirstName,
                pa.Patient.LastName,
                pa.Patient.FullName,
                pa.Patient.DateOfBirth,
                pa.Patient.MemberId,
                pa.Patient.ContactInfo.Email,
                pa.Patient.ContactInfo.Phone),
            new PriorAuthProviderDto(
                pa.Provider.Id,
                pa.Provider.FirstName,
                pa.Provider.LastName,
                pa.Provider.FullName,
                pa.Provider.NPI,
                pa.Provider.Specialty,
                pa.Provider.ContactInfo.Email),
            new PriorAuthPayerDto(
                pa.Payer.Id,
                pa.Payer.PayerName,
                pa.Payer.PayerId,
                pa.Payer.StandardResponseDays,
                pa.Payer.ContactInfo.Phone,
                pa.Payer.ContactInfo.Email),
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
