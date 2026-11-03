using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRutas.Data.Entities
{
    [Table("ECO_TOUR_TB")]
    public class Tour
    {
        public int TourId { get; set; }

        public int EstadoId { get; set; }
        public int CategoriaId { get; set; }
        public int DificultadId { get; set; }

        [Required]
        [StringLength(20)]
        public string CodigoTour { get; set; }

        [Required]
        [StringLength(120)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(1000)]
        public string Descripcion { get; set; }

        [Range(1, 1440)]
        public int DuracionMinutos { get; set; }

        [Required]
        [StringLength(200)]
        public string PuntoEncuentro { get; set; }

        public virtual Estado Estado { get; set; }
        public virtual Categoria Categoria { get; set; }
        public virtual Dificultad Dificultad { get; set; }
        public virtual ICollection<Salida> Salidas { get; set; }
        public virtual ICollection<ImagenTour> Imagenes { get; set; }

        public Tour()
        {
            Salidas = new HashSet<Salida>();
            Imagenes = new HashSet<ImagenTour>();
        }
    }
}
