using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRutas.Data.Entities
{
    [Table("ECO_CATEGORIA_TB")]
    public class Categoria
    {
        public int CategoriaId { get; set; }

        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        public virtual ICollection<Tour> Tours { get; set; }

        public Categoria()
        {
            Tours = new HashSet<Tour>();
        }
    }
}
