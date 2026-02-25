using Microsoft.EntityFrameworkCore;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Enums;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Infrastructure.Persistence;

namespace PriorAuthSystem.Infrastructure.Repositories;

public class PriorAuthorizationRepository : IPriorAuthorizationRepository
{
    private readonly AppDbContext _context;

    private static readonly PriorAuthStatus[] PendingStatuses =
    [
        PriorAuthStatus.Submitted,
        PriorAuthStatus.UnderReview,
        PriorAuthStatus.AdditionalInfoRequested
    ];

    public PriorAuthorizationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PriorAuthorizationRequest>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PriorAuthorizationRequests
            .Include(pa => pa.Patient)
            .Include(pa => pa.Provider)
            .Include(pa => pa.Payer)
            .Include(pa => pa.StatusTransitions)
            .ToListAsync(cancellationToken);
    }

    public async Task<PriorAuthorizationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PriorAuthorizationRequests
            .Include(pa => pa.Patient)
            .Include(pa => pa.Provider)
            .Include(pa => pa.Payer)
            .Include(pa => pa.StatusTransitions)
            .FirstOrDefaultAsync(pa => pa.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PriorAuthorizationRequest>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _context.PriorAuthorizationRequests
            .Include(pa => pa.Patient)
            .Include(pa => pa.Provider)
            .Include(pa => pa.Payer)
            .Where(pa => pa.Patient.Id == patientId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PriorAuthorizationRequest>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PriorAuthorizationRequests
            .Include(pa => pa.Patient)
            .Include(pa => pa.Provider)
            .Include(pa => pa.Payer)
            .Where(pa => PendingStatuses.Contains(pa.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PriorAuthorizationRequest request, CancellationToken cancellationToken = default)
    {
        await _context.PriorAuthorizationRequests.AddAsync(request, cancellationToken);
    }

    public Task UpdateAsync(PriorAuthorizationRequest request, CancellationToken cancellationToken = default)
    {
        _context.PriorAuthorizationRequests.Update(request);
        return Task.CompletedTask;
    }
}
