using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using EcoRutas.Data.Entities;
using EcoRutas.Repository;
using EcoRutas.Service.Models;
// Ángel Felipe Rodríguez Vargas

namespace ECO_Rutas_Grupo_1.Controllers
{
    // Controlador que maneja las operaciones del carrito de compras.
    public class CarritoController : Controller
    {
        private const string SessionKey = "EcoRutas.Carrito";

        private List<CarritoItem> Items
        {
            get { return Session[SessionKey] as List<CarritoItem> ?? new List<CarritoItem>(); }
            set { Session[SessionKey] = value; }
        }

        [HttpGet]
        public JsonResult Index()
        {
            return Json(Items, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Agregar(int salidaId, int cantidadPasajeros)
        {
            if (cantidadPasajeros < 1 || cantidadPasajeros > 6)
                return new HttpStatusCodeResult(400, "La cantidad debe estar entre 1 y 6.");

            using (var unitOfWork = new EfUnitOfWork())
            {
                var salida = unitOfWork.Repository<Salida>().Query()
                    .Include(s => s.Tour)
                    .SingleOrDefault(s => s.SalidaId == salidaId);
                if (salida == null || salida.EstadoId != Estado.ActivoId || salida.Tour == null || salida.Tour.EstadoId != Estado.ActivoId || salida.FechaHora <= DateTime.Now)
                    return new HttpStatusCodeResult(400, "La salida no está disponible para agregar.");
            }

            var items = Items;
            var item = items.SingleOrDefault(i => i.SalidaId == salidaId);
            if (item == null) items.Add(new CarritoItem { SalidaId = salidaId, CantidadPasajeros = cantidadPasajeros });
            else item.CantidadPasajeros = cantidadPasajeros;
            Items = items;
            return Json(items);
        }

        [HttpPut]
        public ActionResult Modificar(int id, int cantidadPasajeros)
        {
            if (cantidadPasajeros < 1 || cantidadPasajeros > 6)
                return new HttpStatusCodeResult(400, "La cantidad debe estar entre 1 y 6.");
            var items = Items;
            var item = items.SingleOrDefault(i => i.SalidaId == id);
            if (item == null) return HttpNotFound();
            item.CantidadPasajeros = cantidadPasajeros;
            Items = items;
            return Json(items);
        }

        [HttpDelete]
        public ActionResult Eliminar(int id)
        {
            var items = Items;
            items.RemoveAll(i => i.SalidaId == id);
            Items = items;
            return Json(items);
        }
    }
}
