using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRutas.Data.Entities
{
    [Table("ECO_SALIDA_TB")]
    public class Salida
    {
        public int SalidaId { get; set; }
        public int TourId { get; set; }
        public int EstadoId { get; set; }

        [Required]
        [StringLength(128)]
        public string GuiaId { get; set; }

        public DateTime FechaHora { get; set; }

        [Range(0.01d, 9999999999.99d)]
        public decimal PrecioPorPersona { get; set; }

        [Range(1, 10000)]
        public int CupoMaximo { get; set; }

        [Range(0, 10000)]
        public int CuposDisponibles { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public bool ListaCerrada { get; set; }
        public DateTime? FechaCierre { get; set; }

        public virtual Tour Tour { get; set; }
        public virtual Estado Estado { get; set; }
        public virtual ApplicationUser Guia { get; set; }
        public virtual ICollection<DetalleReserva> DetallesReserva { get; set; }

        public Salida()
        {
            DetallesReserva = new HashSet<DetalleReserva>();
            ListaCerrada = false;
        }
    }
}
