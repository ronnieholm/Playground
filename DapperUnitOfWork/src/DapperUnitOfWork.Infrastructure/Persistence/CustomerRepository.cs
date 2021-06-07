using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DapperUnitOfWork.Application.Customers;
using DapperUnitOfWork.Application.Seedwork.Interfaces;
using DapperUnitOfWork.Domain.Aggregates.CustomerAggregate;

namespace DapperUnitOfWork.Infrastructure.Persistence
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Customer> GetByIdAsync(Guid id)
        {
            var result = 
                await _unitOfWork.Connection.QuerySingleAsync(
                    "select * from Customers where Id = @Id",
                    new { Id = id },
                    _unitOfWork.Transaction);
            return new Customer(result.Id, new CustomerName(result.Name));
        }

        public async Task<Customer?> GetByNameAsync(string name)
        {
            var result = 
                await _unitOfWork.Connection.QuerySingleOrDefaultAsync(
                    "select * from Customers where Name = @Name",
                    new { Name = name },
                    _unitOfWork.Transaction);
            return result == null ? null : new Customer(result.Id, new CustomerName(result.Name));
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            var result =
                await _unitOfWork.Connection.QueryAsync(
                    "select * from Customers",
                    null,
                    _unitOfWork.Transaction);
            
            // Alternative 0, provide custom public constructor with first argument being a reconstitution marker type.
            
            // Alternative 1, mapping Id, Name, CreatedAt, CreatedBy, UpdateAt, UpdatedBy columns
            // var customer = (Customer)Activator.CreateInstance(typeof(Customer), true)!;
            // customer.GetType().GetProperty(nameof(customer.Id))!.SetValue(customer, Guid.NewGuid(), null);
            // customer.GetType().GetField($"<{nameof(customer.Name)}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(customer, new CustomerName("John Doe"));

            // Alternative 2, somehow works, though in practice we'd likely need more control over the mapping
            var customers = 
                await _unitOfWork.Connection.QueryAsync<Customer>(
                    "select * from Customers",
                    null,
                    _unitOfWork.Transaction);
            
            // Alternative 3, calls constructor causing domain events to be wrongly published
            return result.Select(r => new Customer(r.Id, new CustomerName(r.Name))).ToList();
        }
        
        public void Create(Customer customer)
        {
            _unitOfWork.Connection.Execute(
                "insert into Customers (Id, Name) values (@Id, @Name)",
                new { customer.Id, Name = customer.Name.Value },
                _unitOfWork.Transaction);
        }
    }
}