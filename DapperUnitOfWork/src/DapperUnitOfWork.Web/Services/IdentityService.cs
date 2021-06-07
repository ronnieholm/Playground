using DapperUnitOfWork.Application.Seedwork.Interfaces;

namespace DapperUnitOfWork.Web.Services
{
    public class IdentityService : IIdentityService
    {
        public bool IsCurrentIdentityAnonymous()
        {
            return false;
        }

        public DapperUnitOfWorkIdentity GetCurrentIdentity()
        {
            return new("42");
        }
    }
}