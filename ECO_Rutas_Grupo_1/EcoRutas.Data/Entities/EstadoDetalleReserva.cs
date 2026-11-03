using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRutas.Data.Entities
{
    [Table("ECO_ESTADO_DETALLE_TB")]
    public class EstadoDetalleReserva
    {
        public const int ConfirmadaId = 1;
        public const int CanceladaId = 2;
        public const int CompletadaId = 3;
        public const int NoPresentadaId = 4;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EstadoDetalleReservaId { get; set; }

        [Required]
        [StringLength(30)]
        public string Nombre { get; set; }

        public virtual ICollection<DetalleReserva> Detalles { get; set; }

        public EstadoDetalleReserva()
        {
            Detalles = new HashSet<DetalleReserva>();
        }
    }
}
