using System.Threading.Tasks;
using System.Web.Mvc;
using EcoRutas.Repository;
using EcoRutas.Service.Models;
using EcoRutas.Service.Services;

namespace ECO_Rutas_Grupo_1.Controllers
{
    public class ReservasController : Controller
    {
        [HttpPost]
        public async Task<ActionResult> Confirmar(ConfirmarReservaModel model)
        {
            if (model == null)
                return new HttpStatusCodeResult(400, "Datos de reserva inválidos.");

            model.UsuarioId = User.Identity.IsAuthenticated ? User.Identity.Name : model.UsuarioId;
            var result = await new ReservaService(() => new EfUnitOfWork()).ConfirmarAsync(model);
            if (!result.IsSuccess)
                return new HttpStatusCodeResult(result.StatusCode, result.Message);

            Session.Remove("EcoRutas.Carrito");
            return Json(new { reservaId = result.ReservaId });
        }
    }
}
