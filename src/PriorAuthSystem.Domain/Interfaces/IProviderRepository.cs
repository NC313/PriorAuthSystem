using PriorAuthSystem.Domain.Entities;

namespace PriorAuthSystem.Domain.Interfaces;

public interface IProviderRepository
{
    Task<IReadOnlyList<Provider>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Provider?> GetByNPIAsync(string npi, CancellationToken cancellationToken = default);
    Task AddAsync(Provider provider, CancellationToken cancellationToken = default);
}
