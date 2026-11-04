using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace EcoRutas.Service.Interfaces
{
    /// <summary>
    /// Coordina repositorios que comparten el mismo contexto y guardado.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;
        Task<int> SaveChangesAsync();
        DbContextTransaction BeginTransaction();
    }
}

