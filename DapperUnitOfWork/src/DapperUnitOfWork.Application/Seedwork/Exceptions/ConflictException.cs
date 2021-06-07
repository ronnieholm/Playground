using System;

namespace DapperUnitOfWork.Application.Seedwork.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }

        public ConflictException(Type type, Guid id) : base($"'{type.Name}' with id '{id}' already exists")
        {
        }
    }
}