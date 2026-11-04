using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EcoRutas.Data.Entities;
using EcoRutas.Service.Interfaces;
// Ángel Felipe Rodríguez Vargas

namespace EcoRutas.Service.Services
{
    // Valida las condiciones de una salida antes de agregarla al carrito
    public class SalidaCarritoValidator
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;

        public SalidaCarritoValidator(Func<IUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        // Valida si una salida cumple con las condiciones para ser agregada al carrito
        public async Task<string> ValidarAsync(int salidaId)
        {
            using (var unitOfWork = _unitOfWorkFactory())
            {
                var salida = await unitOfWork.Repository<Salida>().Query()
                    .Include(s => s.Tour)
                    .SingleOrDefaultAsync(s => s.SalidaId == salidaId);

                if (salida == null)
                    return "La salida no existe.";
                if (salida.EstadoId != Estado.ActivoId)
                    return "La salida no está activa.";
                if (salida.Tour == null || salida.Tour.EstadoId != Estado.ActivoId)
                    return "El tour no está activo.";
                if (salida.FechaHora <= DateTime.Now)
                    return "La salida debe ser futura.";
                if (salida.ListaCerrada)
                    return "La lista de la salida está cerrada.";

                return null;
            }
        }
    }
}
