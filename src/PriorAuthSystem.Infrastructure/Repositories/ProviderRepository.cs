using Microsoft.EntityFrameworkCore;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Infrastructure.Persistence;

namespace PriorAuthSystem.Infrastructure.Repositories;

public class ProviderRepository : IProviderRepository
{
    private readonly AppDbContext _context;

    public ProviderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Providers.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Provider?> GetByNPIAsync(string npi, CancellationToken cancellationToken = default)
    {
        return await _context.Providers.FirstOrDefaultAsync(p => p.NPI == npi, cancellationToken);
    }

    public async Task AddAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        await _context.Providers.AddAsync(provider, cancellationToken);
    }
}
