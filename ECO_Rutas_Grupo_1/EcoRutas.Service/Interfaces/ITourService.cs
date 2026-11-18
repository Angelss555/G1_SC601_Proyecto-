
using System.Collections.Generic;
using System.Threading.Tasks;
using EcoRutas.Data.Entities;

namespace EcoRutas.Service.Interfaces
{
    public interface ITourService
    {
        // Funciones administrativas.
        Task<List<Tour>> ListarAsync();
        Task<Tour> ObtenerPorIdAsync(int tourId);
        Task<int> CrearAsync(Tour tour, IEnumerable<ImagenTour> imagenes);
        Task<bool> EditarAsync(Tour tour);
        Task<bool> CambiarEstadoAsync(int tourId, bool activar);
        Task<bool> EliminarAsync(int tourId);

        // Funciones del catálogo público.
        Task<List<Tour>> ListarCatalogoAsync(
            int? categoriaId,
            int? dificultadId);

        Task<Tour> ObtenerDetalleCatalogoAsync(int tourId);
    }
}
