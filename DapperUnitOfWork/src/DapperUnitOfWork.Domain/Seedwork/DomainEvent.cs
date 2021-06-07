using System;

namespace DapperUnitOfWork.Domain.Seedwork
{
    public abstract record DomainEvent
    {
        public bool IsPublished { get; private set; }
        public DateTimeOffset DateOccurred { get; } = DateTime.UtcNow;

        public void MarkAsPublished()
        {
            IsPublished = true;
        }
    }
}