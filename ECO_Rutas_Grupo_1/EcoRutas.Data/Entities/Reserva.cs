using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRutas.Data.Entities
{
    [Table("ECO_RESERVA_TB")]
    public class Reserva
    {
        public int ReservaId { get; set; }

        [Required]
        [StringLength(128)]
        public string UsuarioId { get; set; }

        public DateTime FechaReserva { get; set; }

        public virtual ApplicationUser Usuario { get; set; }
        public virtual ICollection<DetalleReserva> Detalles { get; set; }

        public Reserva()
        {
            Detalles = new HashSet<DetalleReserva>();
            FechaReserva = DateTime.Now;
        }
    }
}
