using PriorAuthSystem.Domain.Entities;

namespace PriorAuthSystem.Domain.Interfaces;

public interface IPayerRepository
{
    Task<IReadOnlyList<Payer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Payer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payer?> GetByPayerIdAsync(string payerId, CancellationToken cancellationToken = default);
    Task AddAsync(Payer payer, CancellationToken cancellationToken = default);
}
