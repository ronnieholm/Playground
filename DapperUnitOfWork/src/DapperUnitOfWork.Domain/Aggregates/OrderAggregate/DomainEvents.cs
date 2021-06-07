using System;
using DapperUnitOfWork.Domain.Seedwork;

namespace DapperUnitOfWork.Domain.Aggregates.OrderAggregate
{
    public record OrderCreated(Guid OrderId) : DomainEvent;
}