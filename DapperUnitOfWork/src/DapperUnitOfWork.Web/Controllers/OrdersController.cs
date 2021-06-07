using System;
using System.Threading.Tasks;
using DapperUnitOfWork.Application.Orders.Commands;
using DapperUnitOfWork.Web.Seedwork;
using Microsoft.AspNetCore.Mvc;

namespace DapperUnitOfWork.Web.Controllers
{
    public class OrdersController : BaseController
    {
        public record CreateOrderDto(
            Guid CustomerId);
        
        [HttpPost]
        public async Task<Guid> Post([FromBody] CreateOrderDto createOrderDto) =>
            await Mediator.Send(
                new CreateOrderCommand(
                    createOrderDto.CustomerId));
    }
}