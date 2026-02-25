using MediatR;
using PriorAuthSystem.Application.PriorAuthorizations.DTOs;

namespace PriorAuthSystem.Application.PriorAuthorizations.Queries.GetPriorAuthById;

public sealed record GetPriorAuthByIdQuery(Guid Id) : IRequest<PriorAuthDto>;
