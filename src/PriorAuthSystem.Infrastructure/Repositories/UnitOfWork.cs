using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Infrastructure.Persistence;

namespace PriorAuthSystem.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IPriorAuthorizationRepository? _priorAuthorizationRequests;
    private IPatientRepository? _patients;
    private IProviderRepository? _providers;
    private IPayerRepository? _payers;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IPriorAuthorizationRepository PriorAuthorizationRequests =>
        _priorAuthorizationRequests ??= new PriorAuthorizationRepository(_context);

    public IPatientRepository Patients =>
        _patients ??= new PatientRepository(_context);

    public IProviderRepository Providers =>
        _providers ??= new ProviderRepository(_context);

    public IPayerRepository Payers =>
        _payers ??= new PayerRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
