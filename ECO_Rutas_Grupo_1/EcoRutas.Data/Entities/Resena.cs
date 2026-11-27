using System;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoRutas.Data.Entities
{
    [Table("ECO_RESENA_TB")]
    public class Resena
    {
        public int ResenaId { get; set; }

        [Required]
        [StringLength(128)]
        public string UsuarioId { get; set; }

        public int TourId { get; set; }

        [Range(1, 5)]
        public int Calificacion { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comentario { get; set; }

        public DateTime Fecha { get; set; }

        public bool Publicada { get; set; }

        public virtual ApplicationUser Usuario { get; set; }
        public virtual Tour Tour { get; set; }

        public Resena()
        {
            Fecha = DateTime.Now;
        }
    }
}
