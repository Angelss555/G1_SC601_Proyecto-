using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using EcoRutas.Repository;
using EcoRutas.Service.Services;

namespace EcoRutas.Web.Controllers
{
    [SessionState(SessionStateBehavior.Required)]
    [RoutePrefix("api/carrito")]
    public class CarritoController : Controller
    {
        private const string CarritoKey = "EcoRutas.Carrito";

        private CarritoService Carrito
        {
            get
            {
                var carrito = Session[CarritoKey] as CarritoService;
                if (carrito == null)
                {
                    carrito = new CarritoService();
                    Session[CarritoKey] = carrito;
                }

                return carrito;
            }
        }

        [HttpGet, Route("")]
        public JsonResult Index()
        {
            return Json(Carrito.ObtenerCarrito(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Route("items")]
        public async Task<JsonResult> Agregar(int salidaId, int cantidadPasajeros)
        {
            try
            {
                var validacion = await new SalidaCarritoValidator(() => new EfUnitOfWork()).ValidarAsync(salidaId);
                if (validacion != null)
                {
                    Response.StatusCode = 400;
                    return Json(new { ok = false, mensaje = validacion });
                }

                Carrito.AgregarSalida(salidaId, cantidadPasajeros);
                return Json(new { ok = true, items = Carrito.ObtenerCarrito() });
            }
            catch (ArgumentException ex)
            {
                Response.StatusCode = 400;
                return Json(new { ok = false, mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Response.StatusCode = 409;
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPut, Route("items/{salidaId:int}")]
        public JsonResult Modificar(int salidaId, int cantidadPasajeros)
        {
            try
            {
                Carrito.ModificarCantidad(salidaId, cantidadPasajeros);
                return Json(new { ok = true, items = Carrito.ObtenerCarrito() });
            }
            catch (ArgumentException ex)
            {
                Response.StatusCode = 400;
                return Json(new { ok = false, mensaje = ex.Message });
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                Response.StatusCode = 404;
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpDelete, Route("items/{salidaId:int}")]
        public JsonResult Eliminar(int salidaId)
        {
            var eliminado = Carrito.EliminarSalida(salidaId);
            if (!eliminado)
            {
                Response.StatusCode = 404;
                return Json(new { ok = false, mensaje = "La salida no está en el carrito." });
            }

            return Json(new { ok = true, items = Carrito.ObtenerCarrito() });
        }
    }
}
