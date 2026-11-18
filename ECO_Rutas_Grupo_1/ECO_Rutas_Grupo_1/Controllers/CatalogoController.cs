
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using EcoRutas.Data.Entities;
using EcoRutas.Repository;
using EcoRutas.Service.Interfaces;
using EcoRutas.Service.Services;

namespace ECO_Rutas_Grupo_1.Controllers
{
    [AllowAnonymous]
    public class CatalogoController : Controller
    {
        private ITourService CrearServicio()
        {
            return new TourService(() => new EfUnitOfWork());
        }

        // Mostrar el catálogo público de tours.
        public async Task<ActionResult> Index(
            int? categoriaId,
            int? dificultadId)
        {
            var tours = await CrearServicio()
                .ListarCatalogoAsync(categoriaId, dificultadId);

            using (var uow = new EfUnitOfWork())
            {
                ViewBag.Categorias = new SelectList(
                    await uow.Repository<Categoria>()
                        .Query()
                        .ToListAsync(),
                    "CategoriaId",
                    "Nombre",
                    categoriaId);

                ViewBag.Dificultades = new SelectList(
                    await uow.Repository<Dificultad>()
                        .Query()
                        .ToListAsync(),
                    "DificultadId",
                    "Nombre",
                    dificultadId);
            }

            return View(tours);
        }

        // Mostrar los detalles públicos de un tour.
        public async Task<ActionResult> Details(int id)
        {
            var tour = await CrearServicio()
                .ObtenerDetalleCatalogoAsync(id);

            if (tour == null)
                return HttpNotFound();

            return View(tour);
        }

        // Cargar imágenes del catálogo público.
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
    }
}
