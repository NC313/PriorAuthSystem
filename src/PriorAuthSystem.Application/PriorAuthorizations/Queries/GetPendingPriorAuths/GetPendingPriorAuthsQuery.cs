using MediatR;
using PriorAuthSystem.Application.PriorAuthorizations.DTOs;

namespace PriorAuthSystem.Application.PriorAuthorizations.Queries.GetPendingPriorAuths;

public sealed record GetPendingPriorAuthsQuery() : IRequest<IList<PriorAuthSummaryDto>>;
