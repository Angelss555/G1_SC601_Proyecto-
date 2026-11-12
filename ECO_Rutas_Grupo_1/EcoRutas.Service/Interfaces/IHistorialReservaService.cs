using System.Collections.Generic;
using System.Threading.Tasks;
using EcoRutas.Service.Models;

// Persona 4 — Valeria Calvo Chaves
namespace EcoRutas.Service.Interfaces
{
    // Historial y cancelación de reservas del asociado.
    // Todas las operaciones reciben el usuario autenticado para validar que el recurso le pertenece.
    public interface IHistorialReservaService
    {
        Task<List<HistorialReservaModel>> ObtenerHistorialAsync(string usuarioId);
        Task<ResultadoOperacion> CancelarDetalleAsync(int detalleReservaId, string usuarioId);
    }
}