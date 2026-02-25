using PriorAuthSystem.Domain.Entities;

namespace PriorAuthSystem.Domain.Interfaces;

public interface IPriorAuthorizationRepository
{
    Task<IReadOnlyList<PriorAuthorizationRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PriorAuthorizationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriorAuthorizationRequest>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriorAuthorizationRequest>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task AddAsync(PriorAuthorizationRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(PriorAuthorizationRequest request, CancellationToken cancellationToken = default);
}
