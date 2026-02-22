namespace PriorAuthSystem.Domain.Interfaces;

public interface IUnitOfWork
{
    IPriorAuthorizationRepository PriorAuthorizationRequests { get; }
    IPatientRepository Patients { get; }
    IProviderRepository Providers { get; }
    IPayerRepository Payers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
