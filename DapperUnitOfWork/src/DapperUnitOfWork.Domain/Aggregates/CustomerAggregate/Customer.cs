using System;
using DapperUnitOfWork.Domain.Seedwork;

namespace DapperUnitOfWork.Domain.Aggregates.CustomerAggregate
{
    public class Customer : AggregateRoot
    {
        public CustomerName Name { get; }


        private Customer()
        {
        }

        public Customer(Guid id, CustomerName name)
        {
            Id = id;
            Name = name;
            AddDomainEvent(new CustomerCreated(id));
        }
    }
}