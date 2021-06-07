using System.Threading.Tasks;
using DapperUnitOfWork.Domain.Seedwork;

namespace DapperUnitOfWork.Application.Seedwork.Interfaces
{
    public interface IDomainEventService
    {
        Task Publish(DomainEvent domainEvent);
    }
}