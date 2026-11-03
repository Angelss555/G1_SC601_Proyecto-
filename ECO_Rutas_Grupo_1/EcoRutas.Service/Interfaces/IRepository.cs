using System.Linq;
using System.Threading.Tasks;

namespace EcoRutas.Service.Interfaces
{
    /// <summary>
    /// Contrato común para consultar y modificar entidades persistidas.
    /// </summary>
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> Query();
        Task<TEntity> FindAsync(params object[] keyValues);
        void Add(TEntity entity);
        void Update(TEntity entity);
        void Remove(TEntity entity);
    }
}
