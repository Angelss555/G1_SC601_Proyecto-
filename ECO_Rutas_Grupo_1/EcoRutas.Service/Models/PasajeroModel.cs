using System.ComponentModel.DataAnnotations;

namespace EcoRutas.Service.Models
{
    public class PasajeroModel
    {
        [Required, StringLength(120)]
        public string NombreCompleto { get; set; }

        [Required, StringLength(30)]
        public string Identificacion { get; set; }
    }
}
