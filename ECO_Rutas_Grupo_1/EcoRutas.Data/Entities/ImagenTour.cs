using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRutas.Data.Entities
{
    [Table("ECO_IMAGEN_TB")]
    public class ImagenTour
    {
        public int ImagenTourId { get; set; }
        public int TourId { get; set; }

        [StringLength(255)]
        public string NombreArchivo { get; set; }

        [Required]
        [StringLength(100)]
        public string TipoMime { get; set; }

        [Required]
        public byte[] Contenido { get; set; }

        [Range(1, 100)]
        public int Orden { get; set; }

        public virtual Tour Tour { get; set; }
    }
}
