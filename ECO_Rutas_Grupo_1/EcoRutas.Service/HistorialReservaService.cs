using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using EcoRutas.Data.Entities;
using EcoRutas.Service.Interfaces;
using EcoRutas.Service.Models;

// Persona 4 — Valeria Calvo Chaves
namespace EcoRutas.Service.Services
{
    // Historial de reservas del asociado y cancelación de un detalle de reserva.
    // Todas las reglas (propiedad del recurso, estado, plazo de 48 horas y devolución de cupos)
    // se validan aquí, en la capa Service, y no en el controlador, la vista ni JavaScript.
    public class HistorialReservaService : IHistorialReservaService
    {
        public const int HorasMinimasCancelacion = 48;
        public const int MaximoReintentos = 3;

        private readonly Func<IUnitOfWork> _unitOfWorkFactory;

        public HistorialReservaService(Func<IUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        // Devuelve únicamente las reservas del usuario indicado (autorización por recurso).
        public async Task<List<HistorialReservaModel>> ObtenerHistorialAsync(string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                return new List<HistorialReservaModel>();

            using (var unitOfWork = _unitOfWorkFactory())
            {
                var reservas = await unitOfWork.Repository<Reserva>().Query()
                    .Include(r => r.Detalles.Select(d => d.Salida.Tour))
                    .Include(r => r.Detalles.Select(d => d.EstadoDetalleReserva))
                    .Include(r => r.Detalles.Select(d => d.Pases))
                    .Where(r => r.UsuarioId == usuarioId)
                    .OrderByDescending(r => r.FechaReserva)
                    .ToListAsync();

                var ahora = DateTime.Now;

                return reservas.Select(r => new HistorialReservaModel
                {
                    ReservaId = r.ReservaId,
                    FechaReserva = r.FechaReserva,
                    Detalles = r.Detalles
                        .OrderBy(d => d.Salida.FechaHora)
                        .Select(d => MapearDetalle(d, ahora))
                        .ToList()
                }).ToList();
            }
        }

        // Cancela un detalle completo: devuelve los cupos, anula los pases y registra la fecha
        // de cancelación en una sola transacción. La Salida tiene [Timestamp] (RowVersion), por lo
        // que si otra operación modificó la salida al mismo tiempo, EF lanza
        // DbUpdateConcurrencyException; en ese caso se recargan los datos y se reintenta.
        public async Task<ResultadoOperacion> CancelarDetalleAsync(int detalleReservaId, string usuarioId)
        {
            if (detalleReservaId <= 0 || string.IsNullOrWhiteSpace(usuarioId))
                return ResultadoOperacion.DatosInvalidos("La solicitud de cancelación no es válida.");

            for (var intento = 1; intento <= MaximoReintentos; intento++)
            {
                // Cada intento usa un contexto nuevo para leer los valores actuales de la base de datos.
                using (var unitOfWork = _unitOfWorkFactory())
                using (var transaction = unitOfWork.BeginTransaction())
                {
                    try
                    {
                        var detalle = await unitOfWork.Repository<DetalleReserva>().Query()
                            .Include(d => d.Reserva)
                            .Include(d => d.Salida.Tour)
                            .Include(d => d.Pases)
                            .SingleOrDefaultAsync(d => d.DetalleReservaId == detalleReservaId);

                        if (detalle == null)
                            return ResultadoOperacion.NoEncontrado("El detalle de reserva no existe.");

                        // Autorización por recurso: solo el dueño de la reserva puede cancelarla.
                        if (detalle.Reserva.UsuarioId != usuarioId)
                            return ResultadoOperacion.Prohibido("No tiene permiso para cancelar una reserva de otro asociado.");

                        string motivo;
                        if (!PuedeCancelarse(detalle, DateTime.Now, out motivo))
                            return ResultadoOperacion.Conflicto(motivo);

                        var salida = detalle.Salida;

                        // Nunca puede haber más cupos disponibles que el cupo máximo.
                        if (salida.CuposDisponibles + detalle.CantidadPasajeros > salida.CupoMaximo)
                            return ResultadoOperacion.Conflicto("No se pueden devolver los cupos: superarían el cupo máximo de la salida.");

                        // 1. Devolver cupos (esta modificación activa la verificación de RowVersion).
                        salida.CuposDisponibles += detalle.CantidadPasajeros;

                        // 2. Cambiar el estado y registrar la fecha de cancelación.
                        detalle.EstadoDetalleReservaId = EstadoDetalleReserva.CanceladaId;
                        detalle.FechaCancelacion = DateTime.Now;

                        // 3. Anular todos los pases del detalle.
                        foreach (var pase in detalle.Pases)
                            pase.Anulado = true;

                        await unitOfWork.SaveChangesAsync();
                        transaction.Commit();

                        return ResultadoOperacion.Ok(string.Format(
                            "Se canceló la reserva de {0} para el {1:dd/MM/yyyy HH:mm}. Se liberaron {2} cupo(s).",
                            salida.Tour.Nombre, salida.FechaHora, detalle.CantidadPasajeros));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // Otra operación cambió la salida (por ejemplo, una reserva simultánea).
                        // Se descarta este intento y, si quedan reintentos, se vuelve a leer y validar todo.
                        transaction.Rollback();

                        if (intento == MaximoReintentos)
                            return ResultadoOperacion.Conflicto("La salida fue modificada por otra operación. Intente cancelar nuevamente.");
                    }
                }
            }

            return ResultadoOperacion.Conflicto("No fue posible cancelar la reserva.");
        }

        // Regla única de cancelación: se usa tanto para mostrar el botón en el historial
        // como para validar la operación, así ambas nunca se contradicen.
        public static bool PuedeCancelarse(DetalleReserva detalle, DateTime ahora, out string motivo)
        {
            if (detalle.EstadoDetalleReservaId != EstadoDetalleReserva.ConfirmadaId)
            {
                motivo = "Solo se puede cancelar una reserva en estado Confirmada.";
                return false;
            }

            if (detalle.Salida.FechaHora < ahora.AddHours(HorasMinimasCancelacion))
            {
                motivo = "La cancelación solo se permite con al menos " + HorasMinimasCancelacion +
                         " horas de anticipación a la salida.";
                return false;
            }

            motivo = null;
            return true;
        }

        private static DetalleHistorialModel MapearDetalle(DetalleReserva d, DateTime ahora)
        {
            string motivo;
            var puedeCancelar = PuedeCancelarse(d, ahora, out motivo);

            return new DetalleHistorialModel
            {
                DetalleReservaId = d.DetalleReservaId,
                SalidaId = d.SalidaId,
                CodigoTour = d.Salida.Tour.CodigoTour,
                NombreTour = d.Salida.Tour.Nombre,
                PuntoEncuentro = d.Salida.Tour.PuntoEncuentro,
                FechaHoraSalida = d.Salida.FechaHora,
                CantidadPasajeros = d.CantidadPasajeros,
                PrecioPorPersona = d.PrecioPorPersona,
                EstadoId = d.EstadoDetalleReservaId,
                EstadoNombre = d.EstadoDetalleReserva != null ? d.EstadoDetalleReserva.Nombre : string.Empty,
                FechaCancelacion = d.FechaCancelacion,
                PuedeCancelar = puedeCancelar,
                MotivoNoCancelable = motivo,
                Pases = d.Pases
                    .OrderBy(p => p.PaseId)
                    .Select(p => new PaseModel
                    {
                        PaseId = p.PaseId,
                        CodigoPase = p.CodigoPase,
                        NombrePasajero = p.NombrePasajero,
                        Identificacion = p.Identificacion,
                        Asistio = p.Asistio,
                        Anulado = p.Anulado
                    })
                    .ToList()
            };
        }
    }
}