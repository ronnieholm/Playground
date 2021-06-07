using System.Collections.Generic;

namespace DapperUnitOfWork.Domain.Seedwork
{
    public class AggregateRoot : Entity
    {
        private readonly List<DomainEvent> _domainEvents = new();
        public virtual IReadOnlyList<DomainEvent> DomainEvents => _domainEvents;

        protected void AddDomainEvent(DomainEvent newEvent)
        {
            _domainEvents.Add(newEvent);
        }

        public virtual void ClearEvents()
        {
            _domainEvents.Clear();
        }        
    }
}