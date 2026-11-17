using System.Collections.Generic;
using System.Threading.Tasks;
using EcoRutas.Service.Models;

// Persona 4 — Valeria Calvo Chaves
namespace EcoRutas.Service.Interfaces
{
    // Administración de usuarios (solo rol Administrador).
    public interface IUsuarioService
    {
        Task<List<UsuarioListadoModel>> ListarAsync(UsuarioFiltroModel filtro);
        Task<ResultadoOperacion<UsuarioFormModel>> ObtenerParaEditarAsync(string usuarioId);
        Task<ResultadoOperacion> CrearAsync(UsuarioFormModel model);
        Task<ResultadoOperacion> EditarAsync(UsuarioFormModel model, string administradorActualId);
        Task<ResultadoOperacion> CambiarEstadoAsync(string usuarioId, bool activar, string administradorActualId);
    }
}