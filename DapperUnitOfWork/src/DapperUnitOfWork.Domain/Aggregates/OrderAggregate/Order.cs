using System;
using System.Collections.Generic;
using DapperUnitOfWork.Domain.Seedwork;

namespace DapperUnitOfWork.Domain.Aggregates.OrderAggregate
{
    public class Order : AggregateRoot
    {
        public OrderDate OrderDate { get; }
        public Guid CustomerId { get; }
        
        private readonly ICollection<OrderLine> _orderLines = null!;
        public IEnumerable<OrderLine> OrderLines => _orderLines;

        public Order(Guid id, Guid customerId, OrderDate orderDate)
        {
            Id = id;
            CustomerId = customerId;
            OrderDate = orderDate;
            AddDomainEvent(new OrderCreated(Id));
        }

        public void AddOrderLine(OrderLine orderLine)
        {
            _orderLines.Add(orderLine);
        }
    }
}