using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EcoRutas.Service.Models;
using EcoRutas.Service.Services;
// Ángel Felipe Rodríguez Vargas

namespace EcoRutas.Tests
{
    // Clase de pruebas unitarias para validar el servicio de reservas.
    [TestClass]
    public class ReservaServiceValidationTests
    {
        // Prueba que verifica que confirmar una reserva sin pasajeros devuelve un error 400.
        [TestMethod]
        public async Task ConfirmarSinPasajerosDevuelve400()
        {
            var service = new ReservaService(() => null);
            var resultado = await service.ConfirmarAsync(new ConfirmarReservaModel
            {
                UsuarioId = "usuario-1",
                Salidas = new[]
                {
                    new ConfirmarSalidaModel { SalidaId = 1, Pasajeros = new PasajeroModel[0] }
                }
            });

            Assert.AreEqual(400, resultado.StatusCode);
            Assert.IsFalse(resultado.IsSuccess);
        }

        // Prueba que verifica que confirmar una reserva con un pasajero sin identificación devuelve un error 400.
        [TestMethod]
        public async Task ConfirmarConPasajeroSinIdentificacionDevuelve400()
        {
            var service = new ReservaService(() => null);
            var resultado = await service.ConfirmarAsync(new ConfirmarReservaModel
            {
                UsuarioId = "usuario-1",
                Salidas = new[]
                {
                    new ConfirmarSalidaModel
                    {
                        SalidaId = 1,
                        Pasajeros = new[]
                        {
                            new PasajeroModel { NombreCompleto = "Ana Pérez", Identificacion = " " }
                        }
                    }
                }
            });

            Assert.AreEqual(400, resultado.StatusCode);
            Assert.IsFalse(resultado.IsSuccess);
        }
    }
}
