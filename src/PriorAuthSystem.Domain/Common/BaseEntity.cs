using System;
using System.Collections.Generic;
using System.Text;

namespace PriorAuthSystem.Domain.Common;

    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; protected set; }

        private readonly List<BaseDomainEvent> _domainEvents = new();
        public IReadOnlyList<BaseDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(BaseDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public void SetUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

