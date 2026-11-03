using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using EcoRutas.Data.Context;
using EcoRutas.Service.Interfaces;

namespace EcoRutas.Repository
{
    /// <summary>
    /// Comparte un ApplicationDbContext entre repositorios y guarda sus cambios.
    /// </summary>
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
        private bool _disposed;

        public EfUnitOfWork()
            : this(new ApplicationDbContext())
        {
        }

        public EfUnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);

            if (!_repositories.TryGetValue(entityType, out var repository))
            {
                repository = new EfRepository<TEntity>(_context);
                _repositories.Add(entityType, repository);
            }

            return (IRepository<TEntity>)repository;
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
                _disposed = true;
            }
        }
    }
}
