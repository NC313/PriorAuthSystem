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

    public async Task<(IReadOnlyList<PriorAuthorizationRequest> Items, int TotalCount)> GetPagedPendingAsync(
        int page, int pageSize, string? search, string? status, string? priority,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PriorAuthorizationRequests
            .Include(pa => pa.Patient)
            .Include(pa => pa.Provider)
            .Include(pa => pa.Payer)
            .Where(pa => PendingStatuses.Contains(pa.Status))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PriorAuthStatus>(status, out var parsedStatus))
            query = query.Where(pa => pa.Status == parsedStatus);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            query = query.Where(pa =>
                pa.Patient.FirstName.ToLower().Contains(q) ||
                pa.Patient.LastName.ToLower().Contains(q) ||
                pa.IcdCode.Code.ToLower().Contains(q) ||
                pa.CptCode.Code.ToLower().Contains(q) ||
                pa.Payer.PayerName.ToLower().Contains(q));
        }

        var now = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(priority))
        {
            query = priority switch
            {
                "High"   => query.Where(pa => pa.RequiredResponseBy < now),
                "Medium" => query.Where(pa => pa.RequiredResponseBy >= now && pa.RequiredResponseBy <= now.AddHours(24)),
                "Normal" => query.Where(pa => pa.RequiredResponseBy > now.AddHours(24)),
                _ => query
            };
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(pa => pa.RequiredResponseBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
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
