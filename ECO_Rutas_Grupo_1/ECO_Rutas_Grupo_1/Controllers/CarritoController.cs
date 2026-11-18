using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using EcoRutas.Data.Entities;
using EcoRutas.Repository;
using EcoRutas.Service.Models;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Agregar(int salidaId, int cantidadPasajeros)
        {
            if (salidaId <= 0 || cantidadPasajeros < 1 || cantidadPasajeros > 6)
                return new HttpStatusCodeResult(400, "Indique una salida y entre 1 y 6 pasajeros.");

            using (var unitOfWork = new EfUnitOfWork())
            {
                var salida = unitOfWork.Repository<Salida>().Query()
                    .Include(salidaRecord => salidaRecord.Tour)
                    .SingleOrDefault(salidaRecord => salidaRecord.SalidaId == salidaId);

                if (salida == null || salida.EstadoId != Estado.ActivoId ||
                    salida.Tour == null || salida.Tour.EstadoId != Estado.ActivoId ||
                    salida.FechaHora <= System.DateTime.Now || salida.ListaCerrada)
                    return new HttpStatusCodeResult(400, "La salida no esta disponible para agregar.");

                if (salida.CuposDisponibles < cantidadPasajeros)
                    return new HttpStatusCodeResult(400, "La salida no tiene suficientes cupos disponibles.");
            }

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
