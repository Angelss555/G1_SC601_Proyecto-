using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using EcoRutas.Data.Entities;
using EcoRutas.Service.Interfaces;
using EcoRutas.Service.Models;
// Ángel Felipe Rodríguez Vargas

namespace EcoRutas.Service.Services
{
    // Clase que implementa la lógica de negocio para la gestión de reservas.
    public class ReservaService : IReservaService
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;

        public ReservaService(Func<IUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        // Método que confirma una reserva basada en el modelo proporcionado.
        public async Task<ReservaResult> ConfirmarAsync(ConfirmarReservaModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.UsuarioId) || model.Salidas == null || model.Salidas.Count == 0)
                return ReservaResult.BadRequest("El carrito no contiene salidas.");

            foreach (var item in model.Salidas)
            {
                if (item == null || item.Pasajeros == null || item.Pasajeros.Count < 1 || item.Pasajeros.Count > 6)
                    return ReservaResult.BadRequest("Cada salida debe tener entre 1 y 6 pasajeros.");

                if (item.Pasajeros.Any(p => p == null || string.IsNullOrWhiteSpace(p.NombreCompleto) || string.IsNullOrWhiteSpace(p.Identificacion)))
                    return ReservaResult.BadRequest("Todos los pasajeros deben incluir nombre e identificación.");
            }

            if (model.Salidas.Select(s => s.SalidaId).Distinct().Count() != model.Salidas.Count)
                return ReservaResult.BadRequest("No se puede repetir una salida en la misma reserva.");

            for (var intento = 1; intento <= 3; intento++)
            {
                using (var unitOfWork = _unitOfWorkFactory())
                using (var transaction = unitOfWork.BeginTransaction())
                {
                    try
                    {
                        var ids = model.Salidas.Select(s => s.SalidaId).ToList();
                        var salidas = await unitOfWork.Repository<Salida>().Query()
                            .Include(s => s.Tour)
                            .Where(s => ids.Contains(s.SalidaId))
                            .ToListAsync();

                        foreach (var item in model.Salidas)
                        {
                            var salida = salidas.SingleOrDefault(s => s.SalidaId == item.SalidaId);
                            if (salida == null || salida.EstadoId != Estado.ActivoId || salida.Tour == null || salida.Tour.EstadoId != Estado.ActivoId || salida.FechaHora <= DateTime.Now)
                                return ReservaResult.Conflict("La salida no existe, está inactiva o ya ocurrió.");

                            if (salida.CuposDisponibles < item.Pasajeros.Count)
                                return ReservaResult.Conflict("La salida '" + salida.Tour.Nombre + "' no tiene cupos suficientes.");
                        }

                        var reserva = new Reserva { UsuarioId = model.UsuarioId, FechaReserva = DateTime.Now };
                        unitOfWork.Repository<Reserva>().Add(reserva);

                        foreach (var item in model.Salidas)
                        {
                            var salida = salidas.Single(s => s.SalidaId == item.SalidaId);
                            salida.CuposDisponibles -= item.Pasajeros.Count;
                            var detalle = new DetalleReserva
                            {
                                Salida = salida,
                                Reserva = reserva,
                                EstadoDetalleReservaId = EstadoDetalleReserva.ConfirmadaId,
                                CantidadPasajeros = item.Pasajeros.Count,
                                PrecioPorPersona = salida.PrecioPorPersona
                            };
                            unitOfWork.Repository<DetalleReserva>().Add(detalle);

                            foreach (var pasajero in item.Pasajeros)
                                unitOfWork.Repository<Pase>().Add(new Pase
                                {
                                    DetalleReserva = detalle,
                                    CodigoPase = Guid.NewGuid().ToString("N"),
                                    NombrePasajero = pasajero.NombreCompleto.Trim(),
                                    Identificacion = pasajero.Identificacion.Trim()
                                });
                        }

                        await unitOfWork.SaveChangesAsync();
                        transaction.Commit();
                        return ReservaResult.Success(reserva.ReservaId);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        transaction.Rollback();
                        if (intento == 3)
                            return ReservaResult.Conflict("La disponibilidad cambió. Intente nuevamente.");
                    }
                }
            }

            return ReservaResult.Conflict("No fue posible confirmar la reserva.");
        }
    }

    // Clase que representa el resultado de la operación de reserva.
    public class ReservaResult
    {
        public bool IsSuccess { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; }
        public int ReservaId { get; private set; }

        public static ReservaResult Success(int id) { return new ReservaResult { IsSuccess = true, StatusCode = 201, ReservaId = id }; }
        public static ReservaResult BadRequest(string message) { return new ReservaResult { StatusCode = 400, Message = message }; }
        public static ReservaResult Conflict(string message) { return new ReservaResult { StatusCode = 409, Message = message }; }
    }
}

