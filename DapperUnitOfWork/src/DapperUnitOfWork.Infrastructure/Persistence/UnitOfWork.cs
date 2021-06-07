using System;
using System.Data;
using System.Data.SqlClient;
using DapperUnitOfWork.Application.Seedwork.Interfaces;

namespace DapperUnitOfWork.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        public Guid Id { get; }
        public IDbConnection Connection { get; }
        public IDbTransaction? Transaction { get; private set; }
        
        public UnitOfWork()
        {
            Id = Guid.NewGuid();
            Connection = new SqlConnection("Server=localhost;Database=DapperUnitOfWork;User Id=sa;Password=<password>");
            Connection.Open();
        }

        public void Begin()
        {
            // TODO(rh): What if you call Begin multiple times?
            Transaction = Connection.BeginTransaction();
        }

        public void Commit()
        {
            Transaction?.Commit();
            Connection.Close();
        }

        public void Rollback()
        {
            Transaction?.Rollback();
            Connection.Close();
        }

        public void Dispose()
        {
            Transaction?.Dispose();
        }
    }
}