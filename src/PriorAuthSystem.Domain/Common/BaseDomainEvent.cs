using System;
using System.Collections.Generic;
using System.Text;

namespace PriorAuthSystem.Domain.Common;

public abstract class BaseDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public abstract string EventType { get; }

}
