using System.Collections.Generic;
using DapperUnitOfWork.Domain.Seedwork;

namespace DapperUnitOfWork.Domain.Aggregates.CustomerAggregate
{
    public class CustomerName : ValueObject
    {
        public const int MaxLength = 50;
        public string Value { get; } = null!;

        private CustomerName()
        {
        }

        public CustomerName(string value) : this()
        {
            if (!IsValid(value))
                throw new DomainException($"Invalid {nameof(CustomerName)}. Was '{value}' of length {value.Length} but expected length <= {MaxLength}");
            Value = value;
        }

        public static bool TryParse(string value, out CustomerName? customerName)
        {
            customerName = null;
            if (!IsValid(value))
                return false;

            customerName = (CustomerName)value;
            return true;
        }

        public static explicit operator CustomerName(string v) => new(v);
        public static implicit operator string(CustomerName v) => v.Value;

        private static bool IsValid(string value) => !string.IsNullOrWhiteSpace(value) && value.Length <= MaxLength;

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }
    }
}