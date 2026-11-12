using System;
using System.Collections.Generic;
using System.Linq;

// Persona 4 — Valeria Calvo Chaves
namespace EcoRutas.Service.Models
{
    // Una reserva del asociado con todos sus detalles (una salida por detalle).
    public class HistorialReservaModel
    {
        public int ReservaId { get; set; }
        public DateTime FechaReserva { get; set; }
        public List<DetalleHistorialModel> Detalles { get; set; }

        // Total de la reserva sin contar los detalles cancelados.
        public decimal TotalVigente
        {
            get { return Detalles.Where(d => !d.EstaCancelado).Sum(d => d.Subtotal); }
        }

        public HistorialReservaModel()
        {
            Detalles = new List<DetalleHistorialModel>();
        }
    }

    public class DetalleHistorialModel
    {
        public int DetalleReservaId { get; set; }
        public int SalidaId { get; set; }
        public string CodigoTour { get; set; }
        public string NombreTour { get; set; }
        public string PuntoEncuentro { get; set; }
        public DateTime FechaHoraSalida { get; set; }
        public int CantidadPasajeros { get; set; }
        public decimal PrecioPorPersona { get; set; }
        public int EstadoId { get; set; }
        public string EstadoNombre { get; set; }
        public DateTime? FechaCancelacion { get; set; }

        // Calculados en la capa Service (la vista solo los muestra).
        public bool PuedeCancelar { get; set; }
        public string MotivoNoCancelable { get; set; }

        public List<PaseModel> Pases { get; set; }

        public decimal Subtotal
        {
            get { return CantidadPasajeros * PrecioPorPersona; }
        }

        public bool EstaCancelado
        {
            get { return EstadoId == EcoRutas.Data.Entities.EstadoDetalleReserva.CanceladaId; }
        }

        public DetalleHistorialModel()
        {
            Pases = new List<PaseModel>();
        }
    }

    public class PaseModel
    {
        public int PaseId { get; set; }
        public string CodigoPase { get; set; }
        public string NombrePasajero { get; set; }
        public string Identificacion { get; set; }
        public bool Asistio { get; set; }
        public bool Anulado { get; set; }
    }
}