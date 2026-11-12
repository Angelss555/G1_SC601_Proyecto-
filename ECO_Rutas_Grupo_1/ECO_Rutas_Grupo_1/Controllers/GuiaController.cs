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
    // Vista del guía (solo consulta en el Avance 2).
    [Authorize(Roles = ApplicationRoleNames.Guia)]
    public class GuiaController : Controller
    {
        private readonly IGuiaService _guiaService;

        public GuiaController()
            : this(new GuiaService(() => new EfUnitOfWork()))
        {
        }

        public GuiaController(IGuiaService guiaService)
        {
            _guiaService = guiaService;
        }

        // GET: /Guia
        public async Task<ActionResult> Index()
        {
            var salidas = await _guiaService.ObtenerSalidasAsignadasAsync(User.Identity.GetUserId());
            return View(salidas);
        }

        // GET: /Guia/Pasajeros/{id}   (id = SalidaId)
        public async Task<ActionResult> Pasajeros(int id)
        {
            var resultado = await _guiaService.ObtenerPasajerosAsync(id, User.Identity.GetUserId());

            if (!resultado.Exito)
            {
                // Devuelve el código real (403 si la salida es de otro guía, 404 si no existe).
                Response.StatusCode = resultado.CodigoEstado;
                Response.TrySkipIisCustomErrors = true;
                ViewBag.CodigoEstado = resultado.CodigoEstado;
                return View("ErrorOperacion", (object)resultado.Mensaje);
            }

            return View(resultado.Datos);
        }
    }
}