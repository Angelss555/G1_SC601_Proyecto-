using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRutas.Data.Entities
{
    [Table("ECO_DIFICULTAD_TB")]
    public class Dificultad
    {
        public int DificultadId { get; set; }

        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        public virtual ICollection<Tour> Tours { get; set; }

        public Dificultad()
        {
            Tours = new HashSet<Tour>();
        }
    }
}
