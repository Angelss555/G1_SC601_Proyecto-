
using EcoRutas.Data.Entities;
using EcoRutas.Repository;
using EcoRutas.Service.Interfaces;
using EcoRutas.Service.Services;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ECO_Rutas_Grupo_1.Controllers
{
    [Authorize(Roles = ApplicationRoleNames.Administrador)]
    public class SalidasController : Controller
    {
        private ISalidaService CrearServicio()
        {
            return new SalidaService(() => new EfUnitOfWork());
        }

        // Mostrar todas las salidas o las de un tour específico.
        public async Task<ActionResult> Index(int? tourId)
        {
            if (tourId.HasValue)
            {
                var salidas = await CrearServicio()
                    .ListarPorTourAsync(tourId.Value);

                ViewBag.TourId = tourId.Value;

                return View(salidas);
            }

            using (var uow = new EfUnitOfWork())
            {
                var salidas = await uow.Repository<Salida>()
                    .Query()
                    .Include(s => s.Tour)
                    .Include(s => s.Estado)
                    .Include(s => s.Guia)
                    .OrderBy(s => s.FechaHora)
                    .ToListAsync();

                ViewBag.TourId = null;

                return View(salidas);
            }
        }

        // Mostrar los detalles de una salida.
        public async Task<ActionResult> Details(int id)
        {
            var salida = await CrearServicio()
                .ObtenerPorIdAsync(id);

            if (salida == null)
                return HttpNotFound();

            return View(salida);
        }

        // Mostrar el formulario para registrar una salida.
        public async Task<ActionResult> Create(int? tourId)
        {
            if (tourId.HasValue)
            {
                using (var uow = new EfUnitOfWork())
                {
                    var tour = await uow.Repository<Tour>()
                        .FindAsync(tourId.Value);

                    if (tour == null || tour.EstadoId != Estado.ActivoId)
                        return HttpNotFound();

                    ViewBag.TourNombre = tour.Nombre;
                }
            }

            await CargarToursAsync(tourId);
            await CargarGuiasAsync();

            return View(new Salida
            {
                TourId = tourId ?? 0
            });
        }

        // Guardar una nueva salida.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Salida salida)
        {
            var userManager = HttpContext.GetOwinContext()
                .GetUserManager<ApplicationUserManager>();

            bool esGuia = !string.IsNullOrWhiteSpace(salida.GuiaId) &&
                await userManager.IsInRoleAsync(
                    salida.GuiaId,
                    ApplicationRoleNames.Guia);

            if (!esGuia)
            {
                ModelState.AddModelError(
                    "GuiaId",
                    "Debe seleccionar un usuario que tenga el rol Guía.");
            }

            using (var uow = new EfUnitOfWork())
            {
                var tour = await uow.Repository<Tour>()
                    .FindAsync(salida.TourId);

                if (tour == null || tour.EstadoId != Estado.ActivoId)
                {
                    ModelState.AddModelError(
                        "TourId",
                        "Debe seleccionar un tour que esté activo.");
                }
                else
                {
                    ViewBag.TourNombre = tour.Nombre;
                }
            }

            if (ModelState.IsValid)
            {
                int id = await CrearServicio().CrearAsync(salida);

                if (id > 0)
                {
                    TempData["Mensaje"] =
                        "La salida se registró correctamente.";

                    return RedirectToAction(
                        "Index",
                        new { tourId = salida.TourId });
                }

                ModelState.AddModelError(
                    "",
                    "No se pudo registrar la salida. Verifique los datos, el tour y el guía.");
            }

            await CargarToursAsync(salida.TourId);
            await CargarGuiasAsync(salida.GuiaId);

            return View(salida);
        }

        // Mostrar el formulario para editar una salida.
        public async Task<ActionResult> Editar(int id)
        {
            var salida = await CrearServicio()
                .ObtenerPorIdAsync(id);

            if (salida == null)
                return HttpNotFound();

            ViewBag.TourNombre = salida.Tour.Nombre;

            await CargarGuiasAsync(salida.GuiaId);

            return View(salida);
        }

        // Guardar los cambios de una salida.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar(Salida salida)
        {
            var userManager = HttpContext.GetOwinContext()
                .GetUserManager<ApplicationUserManager>();

            bool esGuia = !string.IsNullOrWhiteSpace(salida.GuiaId) &&
                await userManager.IsInRoleAsync(
                    salida.GuiaId,
                    ApplicationRoleNames.Guia);

            if (!esGuia)
            {
                ModelState.AddModelError(
                    "GuiaId",
                    "Debe seleccionar un usuario que tenga el rol Guía.");
            }

            if (ModelState.IsValid)
            {
                bool actualizado = await CrearServicio()
                    .EditarAsync(salida);

                if (actualizado)
                {
                    TempData["Mensaje"] =
                        "La salida se actualizó correctamente.";

                    return RedirectToAction(
                        "Index",
                        new { tourId = salida.TourId });
                }

                ModelState.AddModelError(
                    "",
                    "No se pudo actualizar la salida. Verifique los datos y que el cupo máximo no sea inferior a las reservas confirmadas.");
            }

            var salidaActual = await CrearServicio()
                .ObtenerPorIdAsync(salida.SalidaId);

            if (salidaActual == null)
                return HttpNotFound();

            ViewBag.TourNombre = salidaActual.Tour.Nombre;

            await CargarGuiasAsync(salida.GuiaId);

            return View(salida);
        }

        // Activar o desactivar una salida.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarEstado(
            int salidaId,
            bool activar)
        {
            var salida = await CrearServicio()
                .ObtenerPorIdAsync(salidaId);

            if (salida == null)
                return HttpNotFound();

            bool resultado = await CrearServicio()
                .CambiarEstadoAsync(salidaId, activar);

            if (resultado)
            {
                TempData["Mensaje"] = activar
                    ? "La salida se activó correctamente."
                    : "La salida se desactivó correctamente.";
            }
            else
            {
                TempData["Error"] = activar
                    ? "No se pudo activar la salida. Verifique que el tour esté activo, que la fecha sea futura y que la lista no esté cerrada."
                    : "No se puede desactivar una salida que tenga reservas confirmadas.";
            }

            return RedirectToAction(
                "Details",
                new { id = salidaId });
        }

        // Eliminar una salida si no tiene registros de reserva.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Eliminar(int id)
        {
            var salida = await CrearServicio()
                .ObtenerPorIdAsync(id);

            if (salida == null)
                return HttpNotFound();

            int tourId = salida.TourId;

            bool eliminado = await CrearServicio()
                .EliminarAsync(id);

            if (eliminado)
            {
                TempData["Mensaje"] =
                    "La salida se eliminó correctamente.";
            }
            else
            {
                TempData["Error"] =
                    "No se puede eliminar esta salida porque tiene registros de reserva asociados.";
            }

            return RedirectToAction(
                "Index",
                new { tourId = tourId });
        }

        // Cargar únicamente los tours activos.
        private async Task CargarToursAsync(int? tourSeleccionado = null)
        {
            using (var uow = new EfUnitOfWork())
            {
                var tours = await uow.Repository<Tour>()
                    .Query()
                    .Where(t => t.EstadoId == Estado.ActivoId)
                    .OrderBy(t => t.Nombre)
                    .ToListAsync();

                ViewBag.Tours = new SelectList(
                    tours,
                    "TourId",
                    "Nombre",
                    tourSeleccionado);
            }
        }

        // Cargar únicamente los usuarios activos que tengan el rol Guía.
        private async Task CargarGuiasAsync(string guiaSeleccionado = null)
        {
            var userManager = HttpContext.GetOwinContext()
                .GetUserManager<ApplicationUserManager>();

            var usuarios = await userManager.Users
                .Where(u => u.EstadoId == Estado.ActivoId)
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();

            var guias = new System.Collections.Generic.List<object>();

            foreach (var usuario in usuarios)
            {
                if (await userManager.IsInRoleAsync(
                    usuario.Id,
                    ApplicationRoleNames.Guia))
                {
                    guias.Add(new
                    {
                        Id = usuario.Id,
                        NombreCompleto = usuario.NombreCompleto
                    });
                }
            }

            ViewBag.GuiaId = new SelectList(
                guias,
                "Id",
                "NombreCompleto",
                guiaSeleccionado);
        }
    }
}
