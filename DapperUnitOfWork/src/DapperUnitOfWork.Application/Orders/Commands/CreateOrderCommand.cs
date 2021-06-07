using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace DapperUnitOfWork.Application.Orders.Commands
{
    public record CreateOrderCommand(
        Guid CustomerId) : IRequest<Guid>;
    
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(c => c.CustomerId).NotEmpty();
        }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        public CreateOrderCommandHandler()
        {
        }
        
        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(Guid.NewGuid());
        }
    }
}