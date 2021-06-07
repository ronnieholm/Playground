using System;
using System.Data;

namespace DapperUnitOfWork.Application.Seedwork.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Guid Id { get; }
        IDbConnection Connection { get; }
        IDbTransaction? Transaction { get; }
        void Begin();
        void Commit();
        void Rollback();
    }
}