using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using EcoRutas.Repository;
using EcoRutas.Service.Models;
using EcoRutas.Service.Services;
// Ángel Felipe Rodríguez Vargas

namespace EcoRutas.Web.Controllers
{
    // Controlador que maneja las operaciones relacionadas con las reservas.
    [RoutePrefix("api/reservas")]
    public class ReservaController : Controller
    {
        [HttpGet, Route("~/Reserva/Confirmar")]
        public ActionResult Confirmar()
        {
            return View(new ConfirmarReservaModel());
        }

        [HttpPost, Route("confirmar")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Confirmar(ConfirmarReservaModel model)
        {
            if (model == null)
                return new HttpStatusCodeResult(400, "La solicitud es inválida.");

            model.UsuarioId = User.Identity.IsAuthenticated
                ? User.Identity.Name
                : model.UsuarioId;

            if (!ModelState.IsValid)
                return new HttpStatusCodeResult(400, "Los datos de los pasajeros son inválidos.");

            var service = new ReservaService(() => new EfUnitOfWork());
            var resultado = await service.ConfirmarAsync(model);

            if (!resultado.IsSuccess)
                return new HttpStatusCodeResult(resultado.StatusCode, resultado.Message);

            return Json(new { ok = true, reservaId = resultado.ReservaId });
        }
    }
}
