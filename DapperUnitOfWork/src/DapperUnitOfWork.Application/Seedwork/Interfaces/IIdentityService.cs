namespace DapperUnitOfWork.Application.Seedwork.Interfaces
{
    public record DapperUnitOfWorkIdentity(string UserId);

    // public interface ICurrentUserService
    // {
    //     string UserId { get; }
    // }
    
    // IIdentityService is implemented by Web. That way Application can make
    // decisions based on identity without concerning itself with how
    // information about the identity is obtained, making unit/integration
    // testing easier.
    
    public interface IIdentityService
    {
        bool IsCurrentIdentityAnonymous();
        DapperUnitOfWorkIdentity GetCurrentIdentity();
    }
}