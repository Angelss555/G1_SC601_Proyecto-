using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRutas.Data.Entities
{
    [Table("ECO_ESTADO_TB")]
    public class Estado
    {
        public const int ActivoId = 1;
        public const int InactivoId = 2;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EstadoId { get; set; }

        [Required]
        [StringLength(30)]
        public string Nombre { get; set; }

        [StringLength(150)]
        public string Descripcion { get; set; }

        public virtual ICollection<ApplicationUser> Usuarios { get; set; }
        public virtual ICollection<Tour> Tours { get; set; }
        public virtual ICollection<Salida> Salidas { get; set; }

        public Estado()
        {
            Usuarios = new HashSet<ApplicationUser>();
            Tours = new HashSet<Tour>();
            Salidas = new HashSet<Salida>();
        }
    }
}
