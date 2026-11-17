using System.Threading.Tasks;
using System.Web.Mvc;
using EcoRutas.Data.Entities;
using EcoRutas.Repository;
using EcoRutas.Service.Interfaces;
using EcoRutas.Service.Services;
using Microsoft.AspNet.Identity;

// Persona 4 — Valeria Calvo Chaves
namespace ECO_Rutas_Grupo_1.Controllers
{
    // Historial de reservas y cancelación (rol Asociado).
    // El id del usuario siempre se toma de la sesión autenticada, nunca del formulario,
    // y la capa Service comprueba que la reserva le pertenece.
    [Authorize(Roles = ApplicationRoleNames.Asociado)]
    public class HistorialController : Controller
    {
        private readonly IHistorialReservaService _historialService;

        public HistorialController()
            : this(new HistorialReservaService(() => new EfUnitOfWork()))
        {
        }

        public HistorialController(IHistorialReservaService historialService)
        {
            _historialService = historialService;
        }

        // GET: /Historial
        public async Task<ActionResult> Index()
        {
            var historial = await _historialService.ObtenerHistorialAsync(User.Identity.GetUserId());
            return View(historial);
        }

        // POST: /Historial/Cancelar/{id}   (id = DetalleReservaId)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Cancelar(int id)
        {
            var resultado = await _historialService.CancelarDetalleAsync(id, User.Identity.GetUserId());

            if (resultado.Exito)
            {
                TempData["MensajeExito"] = resultado.Mensaje;
            }
            else
            {
                // Mensaje distinto según el código devuelto por la capa Service.
                switch (resultado.CodigoEstado)
                {
                    case 403:
                        TempData["MensajeError"] = "Acceso denegado: " + resultado.Mensaje;
                        break;
                    case 404:
                        TempData["MensajeError"] = "No encontrado: " + resultado.Mensaje;
                        break;
                    case 409:
                        TempData["MensajeError"] = "No se pudo cancelar: " + resultado.Mensaje;
                        break;
                    default:
                        TempData["MensajeError"] = resultado.Mensaje;
                        break;
                }
            }

            return RedirectToAction("Index");
        }
    }
}