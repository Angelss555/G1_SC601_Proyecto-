using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EcoRutas.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(32)]
        public string CodigoUsuario { get; set; }

        [Required]
        [StringLength(120)]
        public string NombreCompleto { get; set; }

        public int EstadoId { get; set; }

        public DateTime FechaRegistro { get; set; }

        public DateTime? UltimaConexion { get; set; }

        public virtual Estado Estado { get; set; }

        public virtual ICollection<Salida> SalidasAsignadas { get; set; }

        public virtual ICollection<Reserva> Reservas { get; set; }

        public ApplicationUser()
        {
            SalidasAsignadas = new HashSet<Salida>();
            Reservas = new HashSet<Reserva>();
            FechaRegistro = DateTime.Now;
            CodigoUsuario = Guid.NewGuid().ToString("N").ToUpperInvariant();
            EstadoId = Estado.ActivoId;
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            return await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}
