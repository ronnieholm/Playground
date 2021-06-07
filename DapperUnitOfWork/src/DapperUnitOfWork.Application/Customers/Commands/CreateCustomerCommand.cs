using System;
using System.Threading;
using System.Threading.Tasks;
using DapperUnitOfWork.Application.Seedwork;
using DapperUnitOfWork.Application.Seedwork.Exceptions;
using DapperUnitOfWork.Application.Seedwork.Interfaces;
using DapperUnitOfWork.Domain.Aggregates.CustomerAggregate;
using FluentValidation;
using MediatR;

namespace DapperUnitOfWork.Application.Customers.Commands
{
    public record CreateCustomerCommand(
        string Name) : IRequest<Guid>;
    
    public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
    {
        public CreateCustomerCommandValidator()
        {
            RuleFor(c => c.Name).NotEmpty();
        }
    }

    public class CreateCustomerCommandHandler : CommandHandler, IRequestHandler<CreateCustomerCommand, Guid>
    {
        private readonly ICustomerRepository _customerRepository;
        
        public CreateCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _customerRepository = customerRepository;
        }
        
        public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var name = request.Name.Trim();
            var customer = await _customerRepository.GetByNameAsync(name);
            if (customer != null)
                throw new ConflictException($"'{nameof(Customer)}' with name '{name}' already exists");

            var id = Guid.NewGuid();
            var newCustomer = new Customer(
                id,
                new CustomerName(name));
            
            _customerRepository.Create(newCustomer);
            Commit();
            return id;
        }
    }
}