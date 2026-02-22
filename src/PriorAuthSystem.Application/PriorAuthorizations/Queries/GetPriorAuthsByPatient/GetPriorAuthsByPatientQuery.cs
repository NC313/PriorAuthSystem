using MediatR;
using PriorAuthSystem.Application.PriorAuthorizations.DTOs;

namespace PriorAuthSystem.Application.PriorAuthorizations.Queries.GetPriorAuthsByPatient;

public sealed record GetPriorAuthsByPatientQuery(Guid PatientId) : IRequest<IList<PriorAuthSummaryDto>>;
