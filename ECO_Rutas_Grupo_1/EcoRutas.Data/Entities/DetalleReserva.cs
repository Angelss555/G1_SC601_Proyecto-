using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRutas.Data.Entities
{
    [Table("ECO_DETALLE_TB")]
    public class DetalleReserva
    {
        public int DetalleReservaId { get; set; }
        public int ReservaId { get; set; }
        public int SalidaId { get; set; }
        public int EstadoDetalleReservaId { get; set; }

        [Range(1, 6)]
        public int CantidadPasajeros { get; set; }

        [Range(0.01d, 9999999999.99d)]
        public decimal PrecioPorPersona { get; set; }

        public DateTime? FechaCancelacion { get; set; }

        public virtual Reserva Reserva { get; set; }
        public virtual Salida Salida { get; set; }
        public virtual EstadoDetalleReserva EstadoDetalleReserva { get; set; }
        public virtual ICollection<Pase> Pases { get; set; }

        public DetalleReserva()
        {
            Pases = new HashSet<Pase>();
        }
    }
}
