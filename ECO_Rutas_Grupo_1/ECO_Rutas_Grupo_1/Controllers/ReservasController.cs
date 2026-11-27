using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using EcoRutas.Data.Entities;
using EcoRutas.Repository;
using EcoRutas.Service.Models;
using EcoRutas.Service.Services;

namespace ECO_Rutas_Grupo_1.Controllers
{
    [Authorize(Roles = ApplicationRoleNames.Asociado)]
    public class ReservasController : Controller
    {
        [HttpGet]
        public ActionResult Confirmar()
        {
            var items = Session["EcoRutas.Carrito"] as List<CarritoItem>;
            if (items == null || items.Count == 0)
                return RedirectToAction("Index", "Carrito");

            var model = new ConfirmarReservaModel
            {
                UsuarioId = User.Identity.GetUserId(),
                Salidas = items.Select(item => new ConfirmarSalidaModel
                {
                    SalidaId = item.SalidaId,
                    Pasajeros = Enumerable.Range(0, item.CantidadPasajeros)
                        .Select(_ => new PasajeroModel())
                        .ToList()
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Confirmar(ConfirmarReservaModel model)
        {
            if (model == null)
                return new HttpStatusCodeResult(400, "Los datos de reserva no son validos.");

            model.UsuarioId = User.Identity.GetUserId();
            ModelState.Remove("UsuarioId");
            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest())
                {
                    Response.StatusCode = 400;
                    return Json(new { ok = false, mensaje = "Los datos de los pasajeros no son válidos." });
                }
                return View(model);
            }

            var result = await new ReservaService(() => new EfUnitOfWork()).ConfirmarAsync(model);
            if (!result.IsSuccess)
            {
                if (Request.IsAjaxRequest())
                {
                    Response.StatusCode = result.StatusCode;
                    return Json(new { ok = false, mensaje = result.Message });
                }
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            Session.Remove("EcoRutas.Carrito");
            if (Request.IsAjaxRequest())
            {
                Response.StatusCode = 201;
                return Json(new { ok = true, reservaId = result.ReservaId });
            }
            TempData["MensajeExito"] = "Reserva confirmada correctamente. Numero: " + result.ReservaId;
            return RedirectToAction("Index", "Home");
        }
    }
}