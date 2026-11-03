using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EcoRutas.Service.Interfaces;

namespace EcoRutas.Repository
{
    /// <summary>
    /// Implementación EF6 del repositorio genérico.
    /// </summary>
    public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _set;

        public EfRepository(DbContext context)
        {
            _context = context;
            _set = context.Set<TEntity>();
        }

        public IQueryable<TEntity> Query()
        {
            return _set;
        }

        public Task<TEntity> FindAsync(params object[] keyValues)
        {
            return _set.FindAsync(keyValues);
        }

        public void Add(TEntity entity)
        {
            _set.Add(entity);
        }

        public void Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(TEntity entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _set.Attach(entity);
            }

            _set.Remove(entity);
        }
    }
}
