using System.Collections.Generic;
using EcoRutas.Service.Models;

// Ángel Felipe Rodríguez Vargas
namespace EcoRutas.Service.Interfaces
{
    // Interface para el servicio de carrito
    public interface ICarritoService
    {
        IReadOnlyCollection<CarritoItem> ObtenerCarrito();
        void AgregarSalida(int salidaId, int cantidadPasajeros);
        void ModificarCantidad(int salidaId, int cantidadPasajeros);
        bool EliminarSalida(int salidaId);
        void Vaciar();
    }
}
