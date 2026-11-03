using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRutas.Data.Entities
{
    [Table("ECO_PASE_TB")]
    public class Pase
    {
        public int PaseId { get; set; }
        public int DetalleReservaId { get; set; }

        [Required]
        [StringLength(36)]
        public string CodigoPase { get; set; }

        [Required]
        [StringLength(120)]
        public string NombrePasajero { get; set; }

        [Required]
        [StringLength(30)]
        public string Identificacion { get; set; }

        public bool Asistio { get; set; }
        public bool Anulado { get; set; }

        public virtual DetalleReserva DetalleReserva { get; set; }
    }
}
