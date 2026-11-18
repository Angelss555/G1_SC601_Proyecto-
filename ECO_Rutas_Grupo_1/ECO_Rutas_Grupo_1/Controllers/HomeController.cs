
using System.Web.Mvc;

namespace ECO_Rutas_Grupo_1.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Conoce más sobre EcoRutas.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contáctanos.";

            return View();
        }
    }
}
