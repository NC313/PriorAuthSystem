using System.Collections.Concurrent;
using Serilog;

namespace PriorAuthSystem.Infrastructure.Services;

public class AuditService
{
    private readonly ILogger _logger;
    private readonly ConcurrentQueue<AuditEntry> _entries = new();

    public AuditService()
    {
        _logger = Log.ForContext<AuditService>();
    }

    public void LogAccess(Guid requestId, string accessedBy)
    {
        var entry = new AuditEntry(DateTime.UtcNow, "Accessed", accessedBy, requestId.ToString(), $"Request {requestId} accessed");
        _entries.Enqueue(entry);
        TrimEntries();

        _logger.Information(
            "PriorAuth audit: {AuditAction} | RequestId={RequestId} | By={AccessedBy} | At={Timestamp}",
            "Accessed", requestId, accessedBy, DateTime.UtcNow);
    }

    public void LogStatusChange(Guid requestId, string changedBy, string fromStatus, string toStatus)
    {
        var entry = new AuditEntry(DateTime.UtcNow, "StatusChanged", changedBy, requestId.ToString(), $"Status changed from {fromStatus} to {toStatus}");
        _entries.Enqueue(entry);
        TrimEntries();

        _logger.Information(
            "PriorAuth audit: {AuditAction} | RequestId={RequestId} | By={ChangedBy} | From={FromStatus} | To={ToStatus} | At={Timestamp}",
            "StatusChanged", requestId, changedBy, fromStatus, toStatus, DateTime.UtcNow);
    }

    public IReadOnlyList<AuditEntry> GetRecentEntries(int count = 50)
    {
        return _entries.Reverse().Take(count).ToList().AsReadOnly();
    }

    private void TrimEntries()
    {
        while (_entries.Count > 500)
            _entries.TryDequeue(out _);
    }
}

public record AuditEntry(DateTime Timestamp, string Action, string PerformedBy, string RequestId, string Details);
