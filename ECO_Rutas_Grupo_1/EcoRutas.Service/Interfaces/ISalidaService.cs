
using System.Collections.Generic;
using System.Threading.Tasks;
using EcoRutas.Data.Entities;

namespace EcoRutas.Service.Interfaces
{
    public interface ISalidaService
    {
        // Consultar las salidas de un tour.
        Task<List<Salida>> ListarPorTourAsync(int tourId);

        // Consultar una salida específica.
        Task<Salida> ObtenerPorIdAsync(int salidaId);

        // Registrar una nueva salida.
        Task<int> CrearAsync(Salida salida);

        // Modificar una salida existente.
        Task<bool> EditarAsync(Salida salida);

        // Activar o inactivar una salida.
        Task<bool> CambiarEstadoAsync(int salidaId, bool activar);

        // Eliminar una salida cuando las reglas lo permitan.
        Task<bool> EliminarAsync(int salidaId);
    }
}
