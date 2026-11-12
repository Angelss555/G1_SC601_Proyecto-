using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EcoRutas.Data.Entities;
using EcoRutas.Service.Interfaces;
using EcoRutas.Service.Models;

// Persona 4 — Valeria Calvo Chaves
namespace EcoRutas.Service.Services
{
    // Consultas del guía. En el Avance 2 son solo de lectura; el registro de asistencia
    // y el cierre de lista se agregan en la entrega final.
    public class GuiaService : IGuiaService
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;

        public GuiaService(Func<IUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        // Solo devuelve las salidas cuyo GuiaId es el del usuario autenticado.
        public async Task<SalidasGuiaModel> ObtenerSalidasAsignadasAsync(string guiaId)
        {
            var resultado = new SalidasGuiaModel();
            if (string.IsNullOrWhiteSpace(guiaId))
                return resultado;

            using (var unitOfWork = _unitOfWorkFactory())
            {
                var salidas = await unitOfWork.Repository<Salida>().Query()
                    .Include(s => s.Tour)
                    .Include(s => s.DetallesReserva)
                    .Where(s => s.GuiaId == guiaId)
                    .ToListAsync();

                var hoy = DateTime.Today;
                var modelos = salidas.Select(s => MapearSalida(s, hoy)).ToList();

                // Las salidas de hoy se consideran próximas (el guía las trabaja ese día).
                resultado.Proximas = modelos
                    .Where(s => s.FechaHora >= hoy)
                    .OrderBy(s => s.FechaHora)
                    .ToList();

                resultado.Pasadas = modelos
                    .Where(s => s.FechaHora < hoy)
                    .OrderByDescending(s => s.FechaHora)
                    .ToList();

                return resultado;
            }
        }

        // Lista de pasajeros de una salida. Excluye los pases de detalles cancelados
        // y valida que la salida esté asignada al guía que consulta.
        public async Task<ResultadoOperacion<ListaPasajerosModel>> ObtenerPasajerosAsync(int salidaId, string guiaId)
        {
            if (salidaId <= 0 || string.IsNullOrWhiteSpace(guiaId))
                return ResultadoOperacion<ListaPasajerosModel>.Error(
                    ResultadoOperacion.CodigoDatosInvalidos, "La solicitud no es válida.");

            using (var unitOfWork = _unitOfWorkFactory())
            {
                var salida = await unitOfWork.Repository<Salida>().Query()
                    .Include(s => s.Tour)
                    .Include(s => s.DetallesReserva)
                    .SingleOrDefaultAsync(s => s.SalidaId == salidaId);

                if (salida == null)
                    return ResultadoOperacion<ListaPasajerosModel>.Error(
                        ResultadoOperacion.CodigoNoEncontrado, "La salida solicitada no existe.");

                // Autorización por recurso: un guía no puede ver salidas de otro guía.
                if (salida.GuiaId != guiaId)
                    return ResultadoOperacion<ListaPasajerosModel>.Error(
                        ResultadoOperacion.CodigoProhibido, "Esta salida no está asignada a usted.");

                var canceladaId = EstadoDetalleReserva.CanceladaId;

                var pases = await unitOfWork.Repository<Pase>().Query()
                    .Include(p => p.DetalleReserva.Reserva.Usuario)
                    .Include(p => p.DetalleReserva.EstadoDetalleReserva)
                    .Where(p => p.DetalleReserva.SalidaId == salidaId
                                && p.DetalleReserva.EstadoDetalleReservaId != canceladaId
                                && !p.Anulado)
                    .OrderBy(p => p.NombrePasajero)
                    .ToListAsync();

                var modelo = new ListaPasajerosModel
                {
                    Salida = MapearSalida(salida, DateTime.Today),
                    Pasajeros = pases.Select(p => new PasajeroGuiaModel
                    {
                        PaseId = p.PaseId,
                        CodigoPase = p.CodigoPase,
                        NombrePasajero = p.NombrePasajero,
                        Identificacion = p.Identificacion,
                        Asistio = p.Asistio,
                        DetalleReservaId = p.DetalleReservaId,
                        TitularReserva = p.DetalleReserva.Reserva.Usuario != null
                            ? p.DetalleReserva.Reserva.Usuario.NombreCompleto
                            : string.Empty,
                        EstadoDetalle = p.DetalleReserva.EstadoDetalleReserva != null
                            ? p.DetalleReserva.EstadoDetalleReserva.Nombre
                            : string.Empty
                    }).ToList()
                };

                return ResultadoOperacion<ListaPasajerosModel>.Ok(modelo);
            }
        }

        // La ocupación se calcula en el servidor a partir de los detalles no cancelados.
        private static SalidaGuiaModel MapearSalida(Salida s, DateTime hoy)
        {
            var confirmados = s.DetallesReserva
                .Where(d => d.EstadoDetalleReservaId != EstadoDetalleReserva.CanceladaId)
                .Sum(d => d.CantidadPasajeros);

            var porcentaje = s.CupoMaximo > 0
                ? Math.Round(confirmados * 100m / s.CupoMaximo, 1)
                : 0m;

            return new SalidaGuiaModel
            {
                SalidaId = s.SalidaId,
                CodigoTour = s.Tour.CodigoTour,
                NombreTour = s.Tour.Nombre,
                PuntoEncuentro = s.Tour.PuntoEncuentro,
                FechaHora = s.FechaHora,
                CupoMaximo = s.CupoMaximo,
                CuposDisponibles = s.CuposDisponibles,
                PasajerosConfirmados = confirmados,
                PorcentajeOcupacion = porcentaje,
                Activa = s.EstadoId == Estado.ActivoId,
                ListaCerrada = s.ListaCerrada,
                EsHoy = s.FechaHora.Date == hoy
            };
        }
    }
}