using System;
using System.Collections.Generic;
using System.Linq;
using EcoRutas.Service.Interfaces;
using EcoRutas.Service.Models;
// Ángel Felipe Rodríguez Vargas

namespace EcoRutas.Service.Services
{
    // Servicio para manejar el carrito de compras
    public class CarritoService : ICarritoService
    {
        private readonly IDictionary<int, CarritoItem> _items;

        public CarritoService()
        {
            _items = new Dictionary<int, CarritoItem>();
        }

        // Obtiene los elementos del carrito
        public IReadOnlyCollection<CarritoItem> ObtenerCarrito()
        {
            return _items.Values
                .Select(item => new CarritoItem
                {
                    SalidaId = item.SalidaId,
                    CantidadPasajeros = item.CantidadPasajeros
                })
                .ToList()
                .AsReadOnly();
        }

        // Agrega una salida al carrito
        public void AgregarSalida(int salidaId, int cantidadPasajeros)
        {
            ValidarCantidad(cantidadPasajeros);

            if (salidaId <= 0)
                throw new ArgumentOutOfRangeException(nameof(salidaId));

            if (_items.ContainsKey(salidaId))
                throw new InvalidOperationException("La salida ya está en el carrito.");

            _items.Add(salidaId, new CarritoItem
            {
                SalidaId = salidaId,
                CantidadPasajeros = cantidadPasajeros
            });
        }

        // Modifica la cantidad de pasajeros para una salida existente en el carrito
        public void ModificarCantidad(int salidaId, int cantidadPasajeros)
        {
            ValidarCantidad(cantidadPasajeros);

            if (!_items.ContainsKey(salidaId))
                throw new KeyNotFoundException("La salida no está en el carrito.");

            _items[salidaId].CantidadPasajeros = cantidadPasajeros;
        }

        // Elimina una salida del carrito
        public bool EliminarSalida(int salidaId)
        {
            return _items.Remove(salidaId);
        }

        public void Vaciar()
        {
            _items.Clear();
        }

        // Valida que la cantidad de pasajeros esté entre 1 y 6
        private static void ValidarCantidad(int cantidadPasajeros)
        {
            if (cantidadPasajeros < 1 || cantidadPasajeros > 6)
                throw new ArgumentOutOfRangeException(nameof(cantidadPasajeros), "La cantidad debe estar entre 1 y 6 pasajeros.");
        }
    }
}
