using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoRutas.Service.Models
{
    public class ConfirmarReservaModel
    {
        [Required]
        public string UsuarioId { get; set; }

        [Required, MinLength(1)]
        public IList<ConfirmarSalidaModel> Salidas { get; set; }
    }

    public class ConfirmarSalidaModel
    {
        public int SalidaId { get; set; }

        [Required, MinLength(1)]
        public IList<PasajeroModel> Pasajeros { get; set; }
    }
}
