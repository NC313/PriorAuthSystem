using Microsoft.EntityFrameworkCore;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Infrastructure.Persistence;

namespace PriorAuthSystem.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _context;

    public PatientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Patients.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Patient?> GetByMemberIdAsync(string memberId, CancellationToken cancellationToken = default)
    {
        return await _context.Patients.FirstOrDefaultAsync(p => p.MemberId == memberId, cancellationToken);
    }

    public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        await _context.Patients.AddAsync(patient, cancellationToken);
    }
}
