using PriorAuthSystem.Application.Common.Interfaces;
using PriorAuthSystem.Domain.Enums;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.API.Services;

public class ExpirationBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ExpirationBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Expiration background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunExpirationPass(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RunExpirationPass(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<IPriorAuthNotificationService>();

            var all = await unitOfWork.PriorAuthorizationRequests.GetAllAsync(cancellationToken);

            var newlyExpired = all
                .Where(a => a.Status is PriorAuthStatus.Submitted or PriorAuthStatus.UnderReview or PriorAuthStatus.AdditionalInfoRequested)
                .Where(a => DateTime.UtcNow > a.RequiredResponseBy)
                .ToList();

            if (newlyExpired.Count == 0) return;

            foreach (var auth in newlyExpired)
                auth.ExpireIfOverdue();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var auth in newlyExpired)
                await notificationService.SendStatusUpdate(auth.Id, auth.Status.ToString());

            logger.LogInformation("Expiration pass: {Count} authorization(s) expired.", newlyExpired.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error during expiration pass.");
        }
    }
}
