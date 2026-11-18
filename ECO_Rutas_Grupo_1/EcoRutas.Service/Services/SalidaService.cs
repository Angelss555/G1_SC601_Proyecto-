
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EcoRutas.Data.Entities;
using EcoRutas.Service.Interfaces;

namespace EcoRutas.Service.Services
{
    public class SalidaService : ISalidaService
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;

        public SalidaService(Func<IUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory
                ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        // Listar las salidas de un tour.
        public async Task<List<Salida>> ListarPorTourAsync(int tourId)
        {
            using (var uow = _unitOfWorkFactory())
            {
                return await uow.Repository<Salida>()
                    .Query()
                    .Include(s => s.Tour)
                    .Include(s => s.Estado)
                    .Include(s => s.Guia)
                    .Include(s => s.DetallesReserva)
                    .Where(s => s.TourId == tourId)
                    .OrderBy(s => s.FechaHora)
                    .ToListAsync();
            }
        }

        // Consultar una salida específica.
        public async Task<Salida> ObtenerPorIdAsync(int salidaId)
        {
            using (var uow = _unitOfWorkFactory())
            {
                return await uow.Repository<Salida>()
                    .Query()
                    .Include(s => s.Tour)
                    .Include(s => s.Estado)
                    .Include(s => s.Guia)
                    .Include(s => s.DetallesReserva)
                    .FirstOrDefaultAsync(s => s.SalidaId == salidaId);
            }
        }

        // Registrar una nueva salida.
        public async Task<int> CrearAsync(Salida salida)
        {
            if (salida == null)
                return 0;

            if (salida.FechaHora <= DateTime.Now ||
                salida.PrecioPorPersona <= 0 ||
                salida.CupoMaximo < 1 ||
                string.IsNullOrWhiteSpace(salida.GuiaId))
                return 0;

            using (var uow = _unitOfWorkFactory())
            {
                var tour = await uow.Repository<Tour>()
                    .FindAsync(salida.TourId);

                if (tour == null || tour.EstadoId != Estado.ActivoId)
                    return 0;

                var guia = await uow.Repository<ApplicationUser>()
                    .Query()
                    .FirstOrDefaultAsync(g =>
                        g.Id == salida.GuiaId &&
                        g.EstadoId == Estado.ActivoId);

                if (guia == null)
                    return 0;

                salida.EstadoId = Estado.ActivoId;
                salida.CuposDisponibles = salida.CupoMaximo;
                salida.ListaCerrada = false;
                salida.FechaCierre = null;

                uow.Repository<Salida>().Add(salida);

                await uow.SaveChangesAsync();

                return salida.SalidaId;
            }
        }

        // Editar una salida.
        public async Task<bool> EditarAsync(Salida salida)
        {
            if (salida == null ||
                salida.FechaHora <= DateTime.Now ||
                salida.PrecioPorPersona <= 0 ||
                salida.CupoMaximo < 1 ||
                string.IsNullOrWhiteSpace(salida.GuiaId))
                return false;

            using (var uow = _unitOfWorkFactory())
            {
                var existente = await uow.Repository<Salida>()
                    .Query()
                    .Include(s => s.DetallesReserva)
                    .FirstOrDefaultAsync(s =>
                        s.SalidaId == salida.SalidaId);

                if (existente == null)
                    return false;

                int pasajerosReservados = existente.DetallesReserva
                    .Where(d =>
                        d.EstadoDetalleReservaId ==
                        EstadoDetalleReserva.ConfirmadaId)
                    .Sum(d => d.CantidadPasajeros);

                if (salida.CupoMaximo < pasajerosReservados)
                    return false;

                var guia = await uow.Repository<ApplicationUser>()
                    .Query()
                    .FirstOrDefaultAsync(g =>
                        g.Id == salida.GuiaId &&
                        g.EstadoId == Estado.ActivoId);

                if (guia == null)
                    return false;

                existente.FechaHora = salida.FechaHora;
                existente.PrecioPorPersona = salida.PrecioPorPersona;
                existente.CupoMaximo = salida.CupoMaximo;
                existente.CuposDisponibles =
                    salida.CupoMaximo - pasajerosReservados;
                existente.GuiaId = salida.GuiaId;

                await uow.SaveChangesAsync();

                return true;
            }
        }

        // Activar o inactivar una salida.
        public async Task<bool> CambiarEstadoAsync(
            int salidaId,
            bool activar)
        {
            using (var uow = _unitOfWorkFactory())
            {
                var salida = await uow.Repository<Salida>()
                    .Query()
                    .Include(s => s.DetallesReserva)
                    .FirstOrDefaultAsync(s =>
                        s.SalidaId == salidaId);

                if (salida == null)
                    return false;

                if (activar)
                {
                    var tour = await uow.Repository<Tour>()
                        .FindAsync(salida.TourId);

                    if (tour == null ||
                        tour.EstadoId != Estado.ActivoId ||
                        salida.FechaHora <= DateTime.Now ||
                        salida.ListaCerrada)
                        return false;
                }
                else
                {
                    bool tieneReservasConfirmadas =
                        salida.DetallesReserva.Any(d =>
                            d.EstadoDetalleReservaId ==
                            EstadoDetalleReserva.ConfirmadaId);

                    if (tieneReservasConfirmadas)
                        return false;
                }

                salida.EstadoId = activar
                    ? Estado.ActivoId
                    : Estado.InactivoId;

                await uow.SaveChangesAsync();

                return true;
            }
        }

        // Eliminar una salida sin reservas asociadas.
        public async Task<bool> EliminarAsync(int salidaId)
        {
            using (var uow = _unitOfWorkFactory())
            {
                var salida = await uow.Repository<Salida>()
                    .Query()
                    .Include(s => s.DetallesReserva)
                    .FirstOrDefaultAsync(s =>
                        s.SalidaId == salidaId);

                if (salida == null)
                    return false;

                if (salida.DetallesReserva.Any())
                    return false;

                uow.Repository<Salida>().Remove(salida);

                await uow.SaveChangesAsync();

                return true;
            }
        }
    }
}
