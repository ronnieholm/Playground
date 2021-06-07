using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace DapperUnitOfWork.Application.Customers.Queries
{
    public record GetCustomersQuery : IRequest<IEnumerable<CustomerDto>>;
    
    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, IEnumerable<CustomerDto>>
    {
        private readonly ICustomerRepository _customerRepository;
        
        public GetCustomersQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            return (await _customerRepository.GetCustomersAsync())
                .Select(CustomerDto.FromDomain)
                .ToList();
        }
    }
}