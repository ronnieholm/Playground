using System;
using DapperUnitOfWork.Domain.Aggregates.CustomerAggregate;

namespace DapperUnitOfWork.Application.Customers.Queries
{
    public record CustomerDto(
        Guid Id,
        string Name
        // string? Comment,
        // DateTimeOffset CreatedAt,
        // string CreatedBy,
        // DateTimeOffset UpdatedAt,
        // string UpdatedBy
        )
    {
        public static CustomerDto FromDomain(Customer domain) =>
            new(
                domain.Id,
                domain.Name
                // domain.Comment?.Value,
                // domain.CreatedAt,
                // domain.CreatedBy,
                // domain.UpdatedAt,
                // domain.UpdatedBy
                );
    }
}