using EcoRutas.Service.Interfaces;

namespace EcoRutas.Repository
{
    public class EfUnitOfWorkFactory : IUnitOfWorkFactory
    {
        public IUnitOfWork Create()
        {
            return new EfUnitOfWork();
        }
    }
}
