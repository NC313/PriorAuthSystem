using Serilog;
using Serilog.Context;

namespace PriorAuthSystem.Infrastructure.Services;

public class AuditService
{
    private readonly ILogger _logger;

    public AuditService()
    {
        _logger = Log.ForContext<AuditService>();
    }

    public void LogAccess(Guid requestId, string accessedBy)
    {
        _logger.Information(
            "PriorAuth audit: {AuditAction} | RequestId={RequestId} | By={AccessedBy} | At={Timestamp}",
            "Accessed",
            requestId,
            accessedBy,
            DateTime.UtcNow);
    }

    public void LogStatusChange(Guid requestId, string changedBy, string fromStatus, string toStatus)
    {
        _logger.Information(
            "PriorAuth audit: {AuditAction} | RequestId={RequestId} | By={ChangedBy} | From={FromStatus} | To={ToStatus} | At={Timestamp}",
            "StatusChanged",
            requestId,
            changedBy,
            fromStatus,
            toStatus,
            DateTime.UtcNow);
    }
}
