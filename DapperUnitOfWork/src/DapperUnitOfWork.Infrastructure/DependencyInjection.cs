using DapperUnitOfWork.Application.Customers;
using DapperUnitOfWork.Application.Seedwork.Interfaces;
using DapperUnitOfWork.Infrastructure.Persistence;
using DapperUnitOfWork.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DapperUnitOfWork.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IDomainEventService, DomainEventService>();
            services.AddTransient<IDateTimeService, DateTimeService>();
            return services;
        }        
    }
}