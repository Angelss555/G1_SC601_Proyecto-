using System.Collections.Generic;
using System.Threading.Tasks;
using EcoRutas.Service.Models;

// Persona 4 — Valeria Calvo Chaves
namespace EcoRutas.Service.Interfaces
{
    // Consultas del guía sobre sus salidas asignadas (solo lectura en el Avance 2).
    public interface IGuiaService
    {
        Task<SalidasGuiaModel> ObtenerSalidasAsignadasAsync(string guiaId);
        Task<ResultadoOperacion<ListaPasajerosModel>> ObtenerPasajerosAsync(int salidaId, string guiaId);
    }
}