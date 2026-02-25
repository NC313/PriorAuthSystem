using Microsoft.EntityFrameworkCore;
using PriorAuthSystem.Application.Common.Interfaces;
using PriorAuthSystem.Domain.Entities;

namespace PriorAuthSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public DbSet<PriorAuthorizationRequest> PriorAuthorizationRequests => Set<PriorAuthorizationRequest>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Payer> Payers => Set<Payer>();
    public DbSet<StatusTransition> StatusTransitions => Set<StatusTransition>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
