
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using EcoRutas.Repository;
using EcoRutas.Service.Models;
using EcoRutas.Service.Services;
using Microsoft.AspNet.Identity;

namespace ECO_Rutas_Grupo_1.Controllers
{
    [Authorize(Roles = "Asociado")]
    public class ReservasController : Controller
    {
        private const string SessionKey = "EcoRutas.Carrito";

        // Mostrar el formulario de confirmación de reserva.
        [HttpGet]
        public ActionResult Index()
        {
            var items = Session[SessionKey] as List<CarritoItem>
                ?? new List<CarritoItem>();

            if (items.Count == 0)
            {
                TempData["Mensaje"] = "Tu carrito está vacío.";
                return RedirectToAction("Index", "Carrito");
            }

            return View(items);
        }

        // Confirmar la reserva.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Confirmar(
            ConfirmarReservaModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(
                    400,
                    "Datos de reserva inválidos."
                );
            }

            model.UsuarioId = User.Identity.GetUserId();

            var result = await new ReservaService(
                () => new EfUnitOfWork()
            ).ConfirmarAsync(model);

            if (!result.IsSuccess)
            {
                return new HttpStatusCodeResult(
                    result.StatusCode,
                    result.Message
                );
            }

            Session.Remove(SessionKey);

            return Json(new
            {
                reservaId = result.ReservaId
            });
        }
    }
}
