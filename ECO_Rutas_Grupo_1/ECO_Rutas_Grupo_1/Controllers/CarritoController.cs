using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using EcoRutas.Data.Entities;
using EcoRutas.Repository;
using EcoRutas.Service.Models;
using EcoRutas.Service.Services;

namespace ECO_Rutas_Grupo_1.Controllers
{
    [Authorize(Roles = ApplicationRoleNames.Asociado)]
    public class CarritoController : Controller
    {
        private const string SessionKey = "EcoRutas.Carrito";

        private List<CarritoItem> Items
        {
            get { return Session[SessionKey] as List<CarritoItem> ?? new List<CarritoItem>(); }
            set { Session[SessionKey] = value; }
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(Items);
        }

        [HttpGet]
        public JsonResult Datos()
        {
            return Json(Items, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Listar()
        {
            return Json(Items, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Agregar(int salidaId, int cantidadPasajeros)
        {
            if (salidaId <= 0 || cantidadPasajeros < 1 || cantidadPasajeros > 6)
                return new HttpStatusCodeResult(400, "Indique una salida y entre 1 y 6 pasajeros.");

            var error = await new SalidaCarritoValidator(
                () => new EfUnitOfWork()).ValidarAsync(salidaId, cantidadPasajeros);

            if (error != null)
                return new HttpStatusCodeResult(400, error);

            var items = Items;
            var item = items.SingleOrDefault(current => current.SalidaId == salidaId);
            if (item == null)
                items.Add(new CarritoItem { SalidaId = salidaId, CantidadPasajeros = cantidadPasajeros });
            else
                item.CantidadPasajeros = cantidadPasajeros;

            Items = items;
            return Request.IsAjaxRequest() ? (ActionResult)Json(items) : RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Modificar(int id, int cantidadPasajeros)
        {
            if (cantidadPasajeros < 1 || cantidadPasajeros > 6)
                return new HttpStatusCodeResult(400, "La cantidad debe estar entre 1 y 6.");

            var items = Items;
            var item = items.SingleOrDefault(current => current.SalidaId == id);
            if (item == null)
                return HttpNotFound();

            item.CantidadPasajeros = cantidadPasajeros;
            Items = items;
            return Request.IsAjaxRequest() ? (ActionResult)Json(items) : RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            var items = Items;
            items.RemoveAll(current => current.SalidaId == id);
            Items = items;
            return Request.IsAjaxRequest() ? (ActionResult)Json(items) : RedirectToAction("Index");
        }
    }
}
