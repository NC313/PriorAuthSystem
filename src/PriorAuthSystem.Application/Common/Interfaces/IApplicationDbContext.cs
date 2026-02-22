using Microsoft.EntityFrameworkCore;
using PriorAuthSystem.Domain.Entities;

namespace PriorAuthSystem.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<PriorAuthorizationRequest> PriorAuthorizationRequests { get; }
    DbSet<Patient> Patients { get; }
    DbSet<Provider> Providers { get; }
    DbSet<Payer> Payers { get; }
    DbSet<StatusTransition> StatusTransitions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
