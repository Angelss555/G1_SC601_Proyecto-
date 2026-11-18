
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EcoRutas.Data.Entities;
using EcoRutas.Service.Interfaces;

namespace EcoRutas.Service.Services
{
    public class TourService : ITourService
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;

        public TourService(Func<IUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory
                ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        // Listar todos los tours para la administración.
        public async Task<List<Tour>> ListarAsync()
        {
            using (var uow = _unitOfWorkFactory())
            {
                return await uow.Repository<Tour>()
                    .Query()
                    .Include(t => t.Categoria)
                    .Include(t => t.Dificultad)
                    .Include(t => t.Estado)
                    .Include(t => t.Imagenes)
                    .OrderBy(t => t.Nombre)
                    .ToListAsync();
            }
        }

        // Obtener un tour con sus imágenes y salidas para administración.
        public async Task<Tour> ObtenerPorIdAsync(int tourId)
        {
            using (var uow = _unitOfWorkFactory())
            {
                return await uow.Repository<Tour>()
                    .Query()
                    .Include(t => t.Categoria)
                    .Include(t => t.Dificultad)
                    .Include(t => t.Estado)
                    .Include(t => t.Imagenes)
                    .Include(t => t.Salidas)
                    .FirstOrDefaultAsync(t => t.TourId == tourId);
            }
        }

        // Registrar un tour y sus imágenes.
        public async Task<int> CrearAsync(
            Tour tour,
            IEnumerable<ImagenTour> imagenes)
        {
            if (tour == null)
                return 0;

            using (var uow = _unitOfWorkFactory())
            {
                var listaImagenes = imagenes == null
                    ? new List<ImagenTour>()
                    : imagenes.ToList();

                if (listaImagenes.Count < 3)
                    return 0;

                tour.EstadoId = Estado.ActivoId;

                uow.Repository<Tour>().Add(tour);

                int orden = 1;

                foreach (var imagen in listaImagenes)
                {
                    imagen.Tour = tour;
                    imagen.Orden = orden++;

                    uow.Repository<ImagenTour>().Add(imagen);
                }

                await uow.SaveChangesAsync();

                return tour.TourId;
            }
        }

        // Editar los datos de un tour.
        public async Task<bool> EditarAsync(Tour tour)
        {
            if (tour == null)
                return false;

            using (var uow = _unitOfWorkFactory())
            {
                var existente = await uow.Repository<Tour>()
                    .FindAsync(tour.TourId);

                if (existente == null)
                    return false;

                existente.Nombre = tour.Nombre;
                existente.Descripcion = tour.Descripcion;
                existente.CodigoTour = tour.CodigoTour;
                existente.CategoriaId = tour.CategoriaId;
                existente.DificultadId = tour.DificultadId;
                existente.DuracionMinutos = tour.DuracionMinutos;
                existente.PuntoEncuentro = tour.PuntoEncuentro;

                await uow.SaveChangesAsync();

                return true;
            }
        }

        // Activar o inactivar un tour.
        public async Task<bool> CambiarEstadoAsync(int tourId, bool activar)
        {
            using (var uow = _unitOfWorkFactory())
            {
                var tour = await uow.Repository<Tour>()
                    .Query()
                    .Include(t => t.Salidas.Select(s => s.DetallesReserva))
                    .FirstOrDefaultAsync(t => t.TourId == tourId);

                if (tour == null)
                    return false;

                // Para activar, debe tener al menos 3 imágenes.
                if (activar)
                {
                    int cantidadImagenes =
                        await uow.Repository<ImagenTour>()
                            .Query()
                            .CountAsync(i => i.TourId == tourId);

                    if (cantidadImagenes < 3)
                        return false;
                }

                // No permitir inactivar si existen reservas confirmadas.
                if (!activar)
                {
                    bool tieneReservasConfirmadas = tour.Salidas
                        .Any(s => s.DetallesReserva.Any(d =>
                            d.EstadoDetalleReservaId ==
                            EstadoDetalleReserva.ConfirmadaId));

                    if (tieneReservasConfirmadas)
                        return false;
                }

                tour.EstadoId = activar
                    ? Estado.ActivoId
                    : Estado.InactivoId;

                await uow.SaveChangesAsync();

                return true;
            }
        }

        // Eliminar un tour cuando no tiene salidas asociadas.
        public async Task<bool> EliminarAsync(int tourId)
        {
            using (var uow = _unitOfWorkFactory())
            {
                var tour = await uow.Repository<Tour>()
                    .Query()
                    .Include(t => t.Salidas)
                    .Include(t => t.Imagenes)
                    .FirstOrDefaultAsync(t => t.TourId == tourId);

                if (tour == null)
                    return false;

                // No eliminar tours que tengan salidas.
                if (tour.Salidas.Any())
                    return false;

                var repositorioImagenes =
                    uow.Repository<ImagenTour>();

                foreach (var imagen in tour.Imagenes.ToList())
                {
                    repositorioImagenes.Remove(imagen);
                }

                uow.Repository<Tour>().Remove(tour);

                await uow.SaveChangesAsync();

                return true;
            }
        }

        // Listar los tours activos para el catálogo público.
        // Permite filtrar por categoría y dificultad.
        public async Task<List<Tour>> ListarCatalogoAsync(
            int? categoriaId,
            int? dificultadId)
        {
            using (var uow = _unitOfWorkFactory())
            {
                var consulta = uow.Repository<Tour>()
                    .Query()
                    .Include(t => t.Categoria)
                    .Include(t => t.Dificultad)
                    .Include(t => t.Imagenes)
                    .Where(t => t.EstadoId == Estado.ActivoId);

                if (categoriaId.HasValue)
                {
                    consulta = consulta.Where(t =>
                        t.CategoriaId == categoriaId.Value);
                }

                if (dificultadId.HasValue)
                {
                    consulta = consulta.Where(t =>
                        t.DificultadId == dificultadId.Value);
                }

                return await consulta
                    .OrderBy(t => t.Nombre)
                    .ToListAsync();
            }
        }

        // Obtener los detalles de un tour activo para el catálogo público.
        // Solo incluye salidas futuras, activas y que puedan reservarse.
        public async Task<Tour> ObtenerDetalleCatalogoAsync(int tourId)
        {
            using (var uow = _unitOfWorkFactory())
            {
                var tour = await uow.Repository<Tour>()
                    .Query()
                    .Include(t => t.Categoria)
                    .Include(t => t.Dificultad)
                    .Include(t => t.Imagenes)
                    .Include(t => t.Salidas)
                    .FirstOrDefaultAsync(t =>
                        t.TourId == tourId &&
                        t.EstadoId == Estado.ActivoId);

                if (tour == null)
                    return null;

                DateTime ahora = DateTime.Now;

                // Filtrar las salidas que realmente pueden reservarse.
                tour.Salidas = tour.Salidas
                    .Where(s =>
                        s.EstadoId == Estado.ActivoId &&
                        s.FechaHora > ahora &&
                        s.CuposDisponibles > 0 &&
                        !s.ListaCerrada)
                    .OrderBy(s => s.FechaHora)
                    .ToList();

                return tour;
            }
        }
    }
}
