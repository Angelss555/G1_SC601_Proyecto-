using System;
using System.Linq;
using EcoRutas.Data.Entities;

// Persona 4 — Valeria Calvo Chaves
namespace EcoRutas.Data.Context
{
    // Reservas de prueba para el asociado demo, para poder probar el historial,
    // la cancelación (regla de 48 h) y la vista del guía sin pasar por el carrito.
    // Se ejecuta después de ApplicationDbSeeder y solo si el asociado no tiene reservas.
    //
    // Resultado:
    //   Reserva 1: salida dentro de 7 días (2 pasajeros, Confirmada  -> SÍ se puede cancelar)
    //              salida de hoy           (1 pasajero,  Confirmada  -> NO, faltan menos de 48 h)
    //   Reserva 2: salida dentro de 7 días (1 pasajero,  Cancelada   -> no aparece en la lista del guía)
    //   Reserva 3: salida de hace 7 días   (2 pasajeros, Confirmada  -> pasada, NO se puede cancelar)
    public static class DemoReservasSeeder
    {
        private const string CorreoAsociado = "asociado@ecorutas.local";
        private const string CodigoTourDemo = "TOUR-DEMO-001";

        public static void Seed(ApplicationDbContext context)
        {
            var asociado = context.Users.SingleOrDefault(u => u.UserName == CorreoAsociado);
            var tour = context.Tours.SingleOrDefault(t => t.CodigoTour == CodigoTourDemo);

            if (asociado == null || tour == null)
                return;

            if (context.Reservas.Any(r => r.UsuarioId == asociado.Id))
                return;

            var salidaPasada = BuscarSalida(context, tour.TourId, DateTime.Today.AddDays(-7).AddHours(9));
            var salidaHoy = BuscarSalida(context, tour.TourId, DateTime.Today.AddHours(9));
            var salidaFutura = BuscarSalida(context, tour.TourId, DateTime.Today.AddDays(7).AddHours(9));

            if (salidaPasada == null || salidaHoy == null || salidaFutura == null)
                return;

            // Reserva 1: una salida cancelable y una no cancelable.
            var reserva1 = NuevaReserva(context, asociado.Id, DateTime.Now.AddDays(-2));
            AgregarDetalle(context, reserva1, salidaFutura, EstadoDetalleReserva.ConfirmadaId,
                new[] { "Ana Rodríguez Mora", "1-1111-1111" },
                new[] { "Luis Rodríguez Mora", "1-2222-2222" });
            AgregarDetalle(context, reserva1, salidaHoy, EstadoDetalleReserva.ConfirmadaId,
                new[] { "Ana Rodríguez Mora", "1-1111-1111" });

            // Reserva 2: un detalle ya cancelado (pases anulados, cupos no descontados).
            var reserva2 = NuevaReserva(context, asociado.Id, DateTime.Now.AddDays(-1));
            AgregarDetalle(context, reserva2, salidaFutura, EstadoDetalleReserva.CanceladaId,
                new[] { "Carlos Jiménez Solís", "3-3333-3333" });

            // Reserva 3: salida pasada.
            var reserva3 = NuevaReserva(context, asociado.Id, DateTime.Now.AddDays(-10));
            AgregarDetalle(context, reserva3, salidaPasada, EstadoDetalleReserva.ConfirmadaId,
                new[] { "María Vargas Quesada", "4-4444-4444" },
                new[] { "José Vargas Quesada", "5-5555-5555" });

            context.SaveChanges();
        }

        private static Salida BuscarSalida(ApplicationDbContext context, int tourId, DateTime fechaHora)
        {
            return context.Salidas.SingleOrDefault(s => s.TourId == tourId && s.FechaHora == fechaHora);
        }

        private static Reserva NuevaReserva(ApplicationDbContext context, string usuarioId, DateTime fecha)
        {
            var reserva = new Reserva { UsuarioId = usuarioId, FechaReserva = fecha };
            context.Reservas.Add(reserva);
            return reserva;
        }

        // Cada pasajero es { nombre completo, identificación }.
        private static void AgregarDetalle(
            ApplicationDbContext context,
            Reserva reserva,
            Salida salida,
            int estadoId,
            params string[][] pasajeros)
        {
            var cancelado = estadoId == EstadoDetalleReserva.CanceladaId;

            var detalle = new DetalleReserva
            {
                Reserva = reserva,
                Salida = salida,
                EstadoDetalleReservaId = estadoId,
                CantidadPasajeros = pasajeros.Length,
                PrecioPorPersona = salida.PrecioPorPersona,
                FechaCancelacion = cancelado ? DateTime.Now.AddHours(-12) : (DateTime?)null
            };
            context.DetallesReserva.Add(detalle);

            // Solo los detalles vigentes ocupan cupos.
            if (!cancelado)
                salida.CuposDisponibles -= pasajeros.Length;

            foreach (var pasajero in pasajeros)
            {
                context.Pases.Add(new Pase
                {
                    DetalleReserva = detalle,
                    CodigoPase = Guid.NewGuid().ToString("N"),
                    NombrePasajero = pasajero[0],
                    Identificacion = pasajero[1],
                    Asistio = false,
                    Anulado = cancelado
                });
            }
        }
    }
}