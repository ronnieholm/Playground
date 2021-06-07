using System;
using DapperUnitOfWork.Domain.Seedwork;

namespace DapperUnitOfWork.Domain.Aggregates.CustomerAggregate
{
    public record CustomerCreated(Guid CustomerId) : DomainEvent;
}