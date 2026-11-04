using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EcoRutas.Service.Services;
// Ángel Felipe Rodríguez Vargas

namespace EcoRutas.Tests
{
    // Clase de pruebas unitarias para el servicio de carrito.
    [TestClass]
    public class CarritoServiceTests
    {
        // Prueba que verifica que agregar una salida con un pasajero la incluye correctamente en el carrito.
        [TestMethod]
        public void AgregarSalidaConUnPasajeroLaIncluyeEnElCarrito()
        {
            var carrito = new CarritoService();

            carrito.AgregarSalida(10, 1);

            var item = carrito.ObtenerCarrito().Single();
            Assert.AreEqual(10, item.SalidaId);
            Assert.AreEqual(1, item.CantidadPasajeros);
        }

        // Prueba que verifica que agregar una salida con siete pasajeros lanza una excepción.
        [TestMethod]
        public void AgregarSalidaConSietePasajerosFalla()
        {
            var carrito = new CarritoService();
            var fallo = false;

            try
            {
                carrito.AgregarSalida(10, 7);
            }
            catch (ArgumentOutOfRangeException)
            {
                fallo = true;
            }

            Assert.IsTrue(fallo);
        }

        // Prueba que verifica que modificar la cantidad de pasajeros actualiza correctamente el item en el carrito.
        [TestMethod]
        public void ModificarCantidadActualizaElItem()
        {
            var carrito = new CarritoService();
            carrito.AgregarSalida(10, 2);

            carrito.ModificarCantidad(10, 6);

            Assert.AreEqual(6, carrito.ObtenerCarrito().Single().CantidadPasajeros);
        }

        // Prueba que verifica que eliminar una salida la quita correctamente del carrito.
        [TestMethod]
        public void EliminarSalidaLaQuitaDelCarrito()
        {
            var carrito = new CarritoService();
            carrito.AgregarSalida(10, 2);

            Assert.IsTrue(carrito.EliminarSalida(10));
            Assert.IsFalse(carrito.ObtenerCarrito().Any());
        }
    }
}
