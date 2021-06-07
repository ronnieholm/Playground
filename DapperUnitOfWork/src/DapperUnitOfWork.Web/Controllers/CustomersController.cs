using System;
using System.Collections;
using System.Threading.Tasks;
using DapperUnitOfWork.Application.Customers.Commands;
using DapperUnitOfWork.Application.Customers.Queries;
using DapperUnitOfWork.Web.Seedwork;
using Microsoft.AspNetCore.Mvc;

namespace DapperUnitOfWork.Web.Controllers
{
    public class CustomersController : BaseController
    {
        [HttpGet]
        public async Task<IEnumerable> GetCustomers() =>
            await Mediator.Send(new GetCustomersQuery());

        // [HttpGet]
        // public async Task<IEnumerable> GetCustomerById(Guid id) =>
        //     await Mediator.Send(new GetCustomerByIdQuery(id));
        
        public record CreateCustomerDto(
            string Name);
        
        [HttpPost]
        public async Task<Guid> Post([FromBody] CreateCustomerDto createCustomerDto) =>
            await Mediator.Send(
                new CreateCustomerCommand(
                    createCustomerDto.Name));
    }
}