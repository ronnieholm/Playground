using DapperUnitOfWork.Domain.Seedwork;
using MediatR;

namespace DapperUnitOfWork.Application.Seedwork.Models
{
    public class DomainEventNotification<TDomainEvent> : INotification where TDomainEvent : DomainEvent
    {
        public TDomainEvent DomainEvent { get; }
        
        public DomainEventNotification(TDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }
}