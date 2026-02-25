using Microsoft.EntityFrameworkCore;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Infrastructure.Persistence;

namespace PriorAuthSystem.Infrastructure.Repositories;

public class PayerRepository : IPayerRepository
{
    private readonly AppDbContext _context;

    public PayerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Payer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payers.ToListAsync(cancellationToken);
    }

    public async Task<Payer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payers.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Payer?> GetByPayerIdAsync(string payerId, CancellationToken cancellationToken = default)
    {
        return await _context.Payers.FirstOrDefaultAsync(p => p.PayerId == payerId, cancellationToken);
    }

    public async Task AddAsync(Payer payer, CancellationToken cancellationToken = default)
    {
        await _context.Payers.AddAsync(payer, cancellationToken);
    }
}
