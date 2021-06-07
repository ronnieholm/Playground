using System;
using DapperUnitOfWork.Application.Seedwork.Interfaces;

namespace DapperUnitOfWork.Infrastructure.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime Now => DateTime.Now;
    }
}