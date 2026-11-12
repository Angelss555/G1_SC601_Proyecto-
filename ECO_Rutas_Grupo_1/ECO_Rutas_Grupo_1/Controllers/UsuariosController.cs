using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EcoRutas.Data.Entities;
using EcoRutas.Repository;
using EcoRutas.Service.Interfaces;
using EcoRutas.Service.Models;
using EcoRutas.Service.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

// Persona 4 — Valeria Calvo Chaves
namespace ECO_Rutas_Grupo_1.Controllers
{
    // CRUD administrativo de usuarios. Solo el rol Administrador tiene acceso.
    // El controlador no toca el contexto de datos: todo pasa por IUsuarioService.
    [Authorize(Roles = ApplicationRoleNames.Administrador)]
    public class UsuariosController : Controller
    {
        private IUsuarioService CrearServicio()
        {
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            return new UsuarioService(() => new EfUnitOfWork(), userManager);
        }

        // GET: /Usuarios
        public async Task<ActionResult> Index(UsuarioFiltroModel filtro)
        {
            ViewBag.Filtro = filtro ?? new UsuarioFiltroModel();
            ViewBag.Roles = new SelectList(UsuarioService.RolesDisponibles);
            var usuarios = await CrearServicio().ListarAsync(filtro);
            return View(usuarios);
        }

        // GET: /Usuarios/Crear
        public ActionResult Crear()
        {
            CargarRoles();
            return View(new UsuarioFormModel { Activo = true, Rol = ApplicationRoleNames.Asociado });
        }

        // POST: /Usuarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crear(UsuarioFormModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError("Password", "La contraseña es obligatoria al crear un usuario.");

            if (!ModelState.IsValid)
            {
                CargarRoles();
                return View(model);
            }

            var resultado = await CrearServicio().CrearAsync(model);
            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                CargarRoles();
                return View(model);
            }

            TempData["MensajeExito"] = resultado.Mensaje;
            return RedirectToAction("Index");
        }

        // GET: /Usuarios/Editar/{id}
        public async Task<ActionResult> Editar(string id)
        {
            var resultado = await CrearServicio().ObtenerParaEditarAsync(id);
            if (!resultado.Exito)
            {
                TempData["MensajeError"] = resultado.Mensaje;
                return RedirectToAction("Index");
            }

            CargarRoles();
            return View(resultado.Datos);
        }

        // POST: /Usuarios/Editar/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar(UsuarioFormModel model)
        {
            if (!ModelState.IsValid)
            {
                CargarRoles();
                return View(model);
            }

            var resultado = await CrearServicio().EditarAsync(model, User.Identity.GetUserId());
            if (!resultado.Exito)
            {
                if (resultado.CodigoEstado == ResultadoOperacion.CodigoNoEncontrado)
                {
                    TempData["MensajeError"] = resultado.Mensaje;
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                CargarRoles();
                return View(model);
            }

            TempData["MensajeExito"] = resultado.Mensaje;
            return RedirectToAction("Index");
        }

        // POST: /Usuarios/CambiarEstado/{id}?activar=true|false
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarEstado(string id, bool activar)
        {
            var resultado = await CrearServicio().CambiarEstadoAsync(id, activar, User.Identity.GetUserId());
            TempData[resultado.Exito ? "MensajeExito" : "MensajeError"] = resultado.Mensaje;
            return RedirectToAction("Index");
        }

        private void CargarRoles()
        {
            ViewBag.Roles = new SelectList(UsuarioService.RolesDisponibles);
        }
    }
}