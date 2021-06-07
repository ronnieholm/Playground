using System;
using System.Collections.Generic;
using DapperUnitOfWork.Domain.Seedwork;

namespace DapperUnitOfWork.Domain.Aggregates.OrderAggregate
{
    public class OrderDate : ValueObject
    {
        public DateTime Value { get; }

        private OrderDate()
        {
        }

        public OrderDate(DateTime value) : this()
        {
            if (!IsValid(value))
                throw new DomainException($"Invalid {nameof(OrderDate)}");
            Value = value;
        }

        public static bool TryParse(DateTime value, out OrderDate orderDate)
        {
            orderDate = (OrderDate) DateTime.MinValue;
            if (!IsValid(value))
                return false;
            orderDate = (OrderDate) value;
            return true;
        }
        
        public static explicit operator OrderDate(DateTime v) => new(v);
        public static implicit operator DateTime(OrderDate v) => v.Value;

        public static bool IsValid(DateTime value) => value > new DateTime(2021, 1, 1);

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }
    }
}