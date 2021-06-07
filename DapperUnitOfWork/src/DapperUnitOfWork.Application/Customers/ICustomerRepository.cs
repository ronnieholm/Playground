using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DapperUnitOfWork.Domain.Aggregates.CustomerAggregate;

namespace DapperUnitOfWork.Application.Customers
{
    public interface ICustomerRepository
    {
        void Create(Customer customer);
        public Task<List<Customer>> GetCustomersAsync();
        public Task<Customer> GetByIdAsync(Guid id);
        public Task<Customer?> GetByNameAsync(string name);
    }
}