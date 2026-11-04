using System.Threading.Tasks;
using EcoRutas.Service.Models;
using EcoRutas.Service.Services;
// Ángel Felipe Rodríguez Vargas

namespace EcoRutas.Service.Interfaces
{
    // Interface para el servicio de reservas
    public interface IReservaService
    {
        Task<ReservaResult> ConfirmarAsync(ConfirmarReservaModel model);
    }
}

