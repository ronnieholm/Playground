using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DapperUnitOfWork.Domain.Seedwork
{
    public abstract class Enumeration : IComparable
    {
        public string Name { get; } = null!;
        public int Id { get; }

        protected Enumeration()
        {
        }

        protected Enumeration(int id, string name)
        {
            if (id <= 0)
                throw new DomainException($"{nameof(id)} must be greater than zero");
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException($"{nameof(name)} was '{name}' must have non-zero length, not counting whitespace");
            
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public static IEnumerable<T> GetAll<T>() where T : Enumeration
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            return fields.Where(f => /* filter out const fields */ !f.IsLiteral).Select(f => f.GetValue(null)).Cast<T>();
        }

        public override bool Equals(object? obj)
        {
            if (!(obj is Enumeration otherValue))
                return false;

            var typeMatches = GetType() == obj.GetType() || obj.GetType().IsSubclassOf(GetType()); ;
            var valueMatches = Id.Equals(otherValue.Id);
            return typeMatches && valueMatches;
        }

        public int CompareTo(object? other) => other == null ? -1 : Id.CompareTo(((Enumeration)other).Id);

        public override int GetHashCode()
        {
            var hashCode = 1460282102;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            return hashCode;
        }
    }
}