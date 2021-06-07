using System.Collections.Generic;
using DapperUnitOfWork.Domain.Seedwork;

namespace DapperUnitOfWork.Domain.ValueObjects
{
    public class Comment : ValueObject
    {
        public const int MaxLength = 500;
        public string Value { get; } = null!;

        private Comment()
        {
        }

        public Comment(string value) : this()
        {
            if (!IsValid(value))
                throw new DomainException($"Invalid {nameof(Comment)}. Was '{value}' of length {value.Length} but expected length <= {MaxLength}");
            Value = value;
        }

        public static bool TryParse(string value, out Comment? comment)
        {
            comment = null;
            if (!IsValid(value))
                return false;

            comment = (Comment)value;
            return true;
        }

        public static explicit operator Comment(string v) => new(v);
        public static implicit operator string(Comment v) => v.Value;

        private static bool IsValid(string value) => !string.IsNullOrWhiteSpace(value) && value.Length <= MaxLength;

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }
    }
}