using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EcoRutas.Data.Entities;
using EcoRutas.Service.Interfaces;

namespace EcoRutas.Service.Services
{
    // Valida las condiciones de una salida antes de agregarla al carrito.
    public class SalidaCarritoValidator
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;

        public SalidaCarritoValidator(Func<IUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public Task<string> ValidarAsync(int salidaId)
        {
            return ValidarAsync(salidaId, null);
        }

        public async Task<string> ValidarAsync(int salidaId, int cantidadPasajeros)
        {
            return await ValidarAsync(salidaId, (int?)cantidadPasajeros);
        }

        private async Task<string> ValidarAsync(int salidaId, int? cantidadPasajeros)
        {
            if (salidaId <= 0)
                return "Indique una salida válida.";

            if (cantidadPasajeros.HasValue &&
                (cantidadPasajeros.Value < 1 || cantidadPasajeros.Value > 6))
                return "La cantidad debe estar entre 1 y 6 pasajeros.";

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
                if (cantidadPasajeros.HasValue && salida.CuposDisponibles < cantidadPasajeros.Value)
                    return "La salida no tiene suficientes cupos disponibles.";

                return null;
            }
        }
    }
}
