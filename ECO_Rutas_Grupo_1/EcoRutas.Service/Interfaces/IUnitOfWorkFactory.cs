using System;

// Ángel Felipe Rodríguez Vargas
namespace EcoRutas.Service.Interfaces
{
    // Interface para la fábrica
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create();
    }
}
