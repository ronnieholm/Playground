using DapperUnitOfWork.Application.Seedwork.Interfaces;

namespace DapperUnitOfWork.Application.Seedwork
{
    public abstract class CommandHandler
    {
        private readonly IUnitOfWork _uow;

        protected CommandHandler(IUnitOfWork uow)
        {
            _uow = uow;
            _uow.Begin();
        }

        protected void Commit()
        {
            _uow.Commit();
        }
    }
}