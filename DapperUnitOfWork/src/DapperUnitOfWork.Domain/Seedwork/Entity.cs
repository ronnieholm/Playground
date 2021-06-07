using System;
using DapperUnitOfWork.Domain.ValueObjects;

namespace DapperUnitOfWork.Domain.Seedwork
{
    public class Entity
    {
        public Guid Id { get; protected init; }
        public Comment? Comment { get; protected set; }

        // Jason's reference implementation provides an AuditableEntity class,
        // but we want every entity to be auditable and therefore inline the
        // properties of AuditableEntity instead.
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = null!;

        public override bool Equals(object? obj)
        {
            var compareTo = obj as Entity;

            if (ReferenceEquals(this, compareTo))
                return true;
            if (compareTo is null)
                return false;

            return Id.Equals(compareTo.Id);
        }

        public static bool operator ==(Entity? a, Entity? b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity? a, Entity? b) => !(a == b);
        public override int GetHashCode() => (GetType().GetHashCode() * 907) + Id.GetHashCode();
        public override string ToString() => $"{GetType().Name} [Id = {Id}]";        
    }
}