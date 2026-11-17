using System;
using System.ComponentModel.DataAnnotations;

// Creado por Valeria Calvo Chaves
namespace EcoRutas.Service.Models
{
    // Fila del listado administrativo de usuarios.
    public class UsuarioListadoModel
    {
        public string Id { get; set; }
        public string CodigoUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public int EstadoId { get; set; }
        public string EstadoNombre { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimaConexion { get; set; }
    }

    // Filtros opcionales del listado.
    public class UsuarioFiltroModel
    {
        [Display(Name = "Buscar")]
        public string Texto { get; set; }

        [Display(Name = "Rol")]
        public string Rol { get; set; }

        [Display(Name = "Estado")]
        public int? EstadoId { get; set; }
    }

    // Formulario de creación y edición de usuarios (rol Administrador).
    public class UsuarioFormModel
    {
        // Vacío al crear; con valor al editar.
        public string Id { get; set; }

        [Display(Name = "Código de usuario")]
        public string CodigoUsuario { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(120, ErrorMessage = "El nombre no puede superar los 120 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        [StringLength(256)]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        [Display(Name = "Rol")]
        public string Rol { get; set; }

        [Display(Name = "Usuario activo")]
        public bool Activo { get; set; }

        // Obligatoria al crear; opcional al editar (solo si se quiere restablecer).
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y su confirmación no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmPassword { get; set; }

        public bool EsNuevo
        {
            get { return string.IsNullOrEmpty(Id); }
        }
    }
}