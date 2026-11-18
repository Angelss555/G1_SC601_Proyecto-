
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EcoRutas.Data.Entities;
using EcoRutas.Repository;
using EcoRutas.Service.Interfaces;
using EcoRutas.Service.Services;

namespace ECO_Rutas_Grupo_1.Controllers
{
    [Authorize(Roles = ApplicationRoleNames.Administrador)]
    public class ToursController : Controller
    {
        private ITourService CrearServicio()
        {
            return new TourService(() => new EfUnitOfWork());
        }

        // Listar tours.
        public async Task<ActionResult> Index()
        {
            var tours = await CrearServicio().ListarAsync();
            return View(tours);
        }

        // Mostrar detalles de un tour.
        public async Task<ActionResult> Details(int id)
        {
            var tour = await CrearServicio().ObtenerPorIdAsync(id);

            if (tour == null)
                return HttpNotFound();

            return View(tour);
        }

        // Mostrar imágenes almacenadas en la base de datos.
        public async Task<ActionResult> Imagen(int id)
        {
            using (var uow = new EfUnitOfWork())
            {
                var imagen = await uow.Repository<ImagenTour>()
                    .FindAsync(id);

                if (imagen == null || imagen.Contenido == null)
                    return HttpNotFound();

                return File(imagen.Contenido, imagen.TipoMime);
            }
        }

        // Mostrar formulario de registro.
        public async Task<ActionResult> Create()
        {
            await CargarListasAsync();
            return View();
        }

        // Registrar tour e imágenes.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Exclude = "Imagenes,Salidas")] Tour tour)
        {
            // Evitar que MVC intente convertir los archivos
            // en objetos de navegación de la entidad Tour.
            ModelState.Remove("Imagenes");

            // Obtener directamente los archivos del formulario.
            var archivos = Request.Files.GetMultiple("imagenes")
                .Where(i => i != null &&
                            i.ContentLength > 0 &&
                            !string.IsNullOrWhiteSpace(i.FileName))
                .ToList();

            if (archivos.Count < 3)
            {
                ModelState.AddModelError(
                    "",
                    "Debe seleccionar al menos 3 imágenes.");
            }

            var tiposPermitidos = new[]
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/webp"
            };

            foreach (var archivo in archivos)
            {
                if (!tiposPermitidos.Contains(
                    archivo.ContentType.ToLowerInvariant()))
                {
                    ModelState.AddModelError(
                        "",
                        "Solo se permiten imágenes JPG, PNG, GIF o WEBP.");
                    break;
                }

                if (archivo.ContentLength > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "",
                        "Cada imagen debe pesar como máximo 5 MB.");
                    break;
                }
            }

            if (ModelState.IsValid)
            {
                var imagenesTour = new List<ImagenTour>();

                for (int i = 0; i < archivos.Count; i++)
                {
                    var archivo = archivos[i];

                    using (var memoria = new MemoryStream())
                    {
                        archivo.InputStream.CopyTo(memoria);

                        imagenesTour.Add(new ImagenTour
                        {
                            NombreArchivo = Path.GetFileName(
                                archivo.FileName),
                            TipoMime = archivo.ContentType,
                            Contenido = memoria.ToArray(),
                            Orden = i + 1
                        });
                    }
                }

                var id = await CrearServicio().CrearAsync(
                    tour,
                    imagenesTour);

                if (id <= 0)
                {
                    ModelState.AddModelError(
                        "",
                        "No fue posible registrar el tour. Verifique sus imágenes.");

                    await CargarListasAsync(
                        tour.CategoriaId,
                        tour.DificultadId);

                    return View(tour);
                }

                TempData["Mensaje"] =
                    "El tour se registró correctamente.";

                return RedirectToAction("Details", new { id = id });
            }

            await CargarListasAsync(
                tour.CategoriaId,
                tour.DificultadId);

            return View(tour);
        }

        // Mostrar formulario de edición.
        public async Task<ActionResult> Edit(int id)
        {
            var tour = await CrearServicio().ObtenerPorIdAsync(id);

            if (tour == null)
                return HttpNotFound();

            await CargarListasAsync(
                tour.CategoriaId,
                tour.DificultadId);

            return View(tour);
        }

        // Guardar cambios del tour.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Tour tour)
        {
            if (ModelState.IsValid)
            {
                var resultado = await CrearServicio().EditarAsync(tour);

                if (!resultado)
                    return HttpNotFound();

                TempData["Mensaje"] =
                    "El tour se actualizó correctamente.";

                return RedirectToAction(
                    "Details",
                    new { id = tour.TourId });
            }

            await CargarListasAsync(
                tour.CategoriaId,
                tour.DificultadId);

            return View(tour);
        }

        // Activar o inactivar un tour.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarEstado(
            int id,
            bool activar)
        {
            var resultado = await CrearServicio()
                .CambiarEstadoAsync(id, activar);

            if (resultado)
            {
                TempData["Mensaje"] = activar
                    ? "El tour se activó correctamente."
                    : "El tour se inactivó correctamente.";
            }
            else
            {
                TempData["Error"] =
                    "No se pudo cambiar el estado. Verifique que el tour exista, tenga al menos 3 imágenes para activarlo y que no tenga reservas para inactivarlo.";
            }

            return RedirectToAction("Index");
        }

        // Cargar categorías y dificultades.
        private async Task CargarListasAsync(
            int? categoriaId = null,
            int? dificultadId = null)
        {
            using (var uow = new EfUnitOfWork())
            {
                var categorias = await uow.Repository<Categoria>()
                    .Query()
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();

                var dificultades = await uow.Repository<Dificultad>()
                    .Query()
                    .OrderBy(d => d.Nombre)
                    .ToListAsync();

                ViewBag.CategoriaId = new SelectList(
                    categorias,
                    "CategoriaId",
                    "Nombre",
                    categoriaId);

                ViewBag.DificultadId = new SelectList(
                    dificultades,
                    "DificultadId",
                    "Nombre",
                    dificultadId);
            }
        }
    }
}
