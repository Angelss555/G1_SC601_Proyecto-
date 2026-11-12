using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EcoRutas.Data.Entities;
using EcoRutas.Service.Interfaces;
using EcoRutas.Service.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

// Persona 4 — Valeria Calvo Chaves
namespace EcoRutas.Service.Services
{
    // CRUD administrativo de usuarios: creación, edición, activación, inactivación y roles.
    // Las consultas usan el Repository/UnitOfWork; las escrituras usan UserManager de
    // ASP.NET Identity para que la contraseña quede encriptada y los roles se asignen bien.
    // No se eliminan usuarios físicamente: se inactivan (tienen reservas y salidas asociadas).
    public class UsuarioService : IUsuarioService
    {
        public static readonly string[] RolesDisponibles =
        {
            ApplicationRoleNames.Administrador,
            ApplicationRoleNames.Asociado,
            ApplicationRoleNames.Guia
        };

        // Fecha de bloqueo usada para impedir el inicio de sesión de usuarios inactivos.
        private static readonly DateTimeOffset BloqueoIndefinido =
            new DateTimeOffset(2999, 12, 31, 0, 0, 0, TimeSpan.Zero);

        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuarioService(Func<IUnitOfWork> unitOfWorkFactory, UserManager<ApplicationUser> userManager)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<List<UsuarioListadoModel>> ListarAsync(UsuarioFiltroModel filtro)
        {
            filtro = filtro ?? new UsuarioFiltroModel();

            using (var unitOfWork = _unitOfWorkFactory())
            {
                var roles = await unitOfWork.Repository<IdentityRole>().Query().ToListAsync();
                var nombresRol = roles.ToDictionary(r => r.Id, r => r.Name);

                var consulta = unitOfWork.Repository<ApplicationUser>().Query()
                    .Include(u => u.Estado)
                    .Include(u => u.Roles);

                if (!string.IsNullOrWhiteSpace(filtro.Texto))
                {
                    var texto = filtro.Texto.Trim();
                    consulta = consulta.Where(u => u.NombreCompleto.Contains(texto)
                                                   || u.Email.Contains(texto)
                                                   || u.CodigoUsuario.Contains(texto));
                }

                if (filtro.EstadoId.HasValue)
                {
                    var estadoId = filtro.EstadoId.Value;
                    consulta = consulta.Where(u => u.EstadoId == estadoId);
                }

                if (!string.IsNullOrWhiteSpace(filtro.Rol))
                {
                    var rol = roles.FirstOrDefault(r => r.Name == filtro.Rol);
                    var rolId = rol != null ? rol.Id : string.Empty;
                    consulta = consulta.Where(u => u.Roles.Any(ur => ur.RoleId == rolId));
                }

                var usuarios = await consulta.OrderBy(u => u.NombreCompleto).ToListAsync();

                return usuarios.Select(u => new UsuarioListadoModel
                {
                    Id = u.Id,
                    CodigoUsuario = u.CodigoUsuario,
                    NombreCompleto = u.NombreCompleto,
                    Email = u.Email,
                    Rol = string.Join(", ", u.Roles
                        .Where(ur => nombresRol.ContainsKey(ur.RoleId))
                        .Select(ur => nombresRol[ur.RoleId])),
                    EstadoId = u.EstadoId,
                    EstadoNombre = u.Estado != null ? u.Estado.Nombre : string.Empty,
                    Activo = u.EstadoId == Estado.ActivoId,
                    FechaRegistro = u.FechaRegistro,
                    UltimaConexion = u.UltimaConexion
                }).ToList();
            }
        }

        public async Task<ResultadoOperacion<UsuarioFormModel>> ObtenerParaEditarAsync(string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                return ResultadoOperacion<UsuarioFormModel>.Error(ResultadoOperacion.CodigoDatosInvalidos, "Debe indicar el usuario.");

            var usuario = await _userManager.FindByIdAsync(usuarioId);
            if (usuario == null)
                return ResultadoOperacion<UsuarioFormModel>.Error(ResultadoOperacion.CodigoNoEncontrado, "El usuario no existe.");

            var roles = await _userManager.GetRolesAsync(usuario.Id);

            return ResultadoOperacion<UsuarioFormModel>.Ok(new UsuarioFormModel
            {
                Id = usuario.Id,
                CodigoUsuario = usuario.CodigoUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email,
                Rol = roles.FirstOrDefault(),
                Activo = usuario.EstadoId == Estado.ActivoId
            });
        }

        public async Task<ResultadoOperacion> CrearAsync(UsuarioFormModel model)
        {
            var validacion = ValidarFormulario(model);
            if (validacion != null)
                return validacion;

            if (string.IsNullOrWhiteSpace(model.Password))
                return ResultadoOperacion.DatosInvalidos("La contraseña es obligatoria al crear un usuario.");

            var email = model.Email.Trim();
            if (await _userManager.FindByEmailAsync(email) != null)
                return ResultadoOperacion.Conflicto("Ya existe un usuario registrado con el correo " + email + ".");

            var usuario = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                NombreCompleto = model.NombreCompleto.Trim(),
                EstadoId = model.Activo ? Estado.ActivoId : Estado.InactivoId,
                FechaRegistro = DateTime.Now
            };

            var creado = await _userManager.CreateAsync(usuario, model.Password);
            if (!creado.Succeeded)
                return ResultadoOperacion.DatosInvalidos(UnirErrores(creado));

            var rolAsignado = await _userManager.AddToRoleAsync(usuario.Id, model.Rol);
            if (!rolAsignado.Succeeded)
            {
                // Si no se pudo asignar el rol, no se deja un usuario incompleto.
                await _userManager.DeleteAsync(usuario);
                return ResultadoOperacion.DatosInvalidos(UnirErrores(rolAsignado));
            }

            await AplicarBloqueoSegunEstadoAsync(usuario.Id, model.Activo);

            return ResultadoOperacion.Ok("Se creó el usuario " + usuario.NombreCompleto + " con el rol " + model.Rol + ".");
        }

        public async Task<ResultadoOperacion> EditarAsync(UsuarioFormModel model, string administradorActualId)
        {
            var validacion = ValidarFormulario(model);
            if (validacion != null)
                return validacion;

            var usuario = await _userManager.FindByIdAsync(model.Id);
            if (usuario == null)
                return ResultadoOperacion.NoEncontrado("El usuario no existe.");

            var rolesActuales = await _userManager.GetRolesAsync(usuario.Id);

            var reglas = await ValidarCambioDeRolOEstadoAsync(usuario, rolesActuales, model.Rol, model.Activo, administradorActualId);
            if (reglas != null)
                return reglas;

            var email = model.Email.Trim();
            var otro = await _userManager.FindByEmailAsync(email);
            if (otro != null && otro.Id != usuario.Id)
                return ResultadoOperacion.Conflicto("Ya existe otro usuario registrado con el correo " + email + ".");

            // Si se indicó una nueva contraseña, se valida antes de tocar el usuario.
            var cambiarPassword = !string.IsNullOrWhiteSpace(model.Password);
            if (cambiarPassword)
            {
                var passwordValida = await _userManager.PasswordValidator.ValidateAsync(model.Password);
                if (!passwordValida.Succeeded)
                    return ResultadoOperacion.DatosInvalidos(UnirErrores(passwordValida));
            }

            var estabaActivo = usuario.EstadoId == Estado.ActivoId;

            usuario.NombreCompleto = model.NombreCompleto.Trim();
            usuario.Email = email;
            usuario.UserName = email;
            usuario.EstadoId = model.Activo ? Estado.ActivoId : Estado.InactivoId;

            var actualizado = await _userManager.UpdateAsync(usuario);
            if (!actualizado.Succeeded)
                return ResultadoOperacion.DatosInvalidos(UnirErrores(actualizado));

            // Cambio de rol: cada usuario tiene un único rol.
            if (!(rolesActuales.Count == 1 && rolesActuales[0] == model.Rol))
            {
                if (rolesActuales.Count > 0)
                {
                    var quitados = await _userManager.RemoveFromRolesAsync(usuario.Id, rolesActuales.ToArray());
                    if (!quitados.Succeeded)
                        return ResultadoOperacion.DatosInvalidos(UnirErrores(quitados));
                }

                var agregado = await _userManager.AddToRoleAsync(usuario.Id, model.Rol);
                if (!agregado.Succeeded)
                    return ResultadoOperacion.DatosInvalidos(UnirErrores(agregado));

                // Obliga a renovar la cookie para que el nuevo rol tome efecto.
                await _userManager.UpdateSecurityStampAsync(usuario.Id);
            }

            if (cambiarPassword)
            {
                await _userManager.RemovePasswordAsync(usuario.Id);
                var nuevaPassword = await _userManager.AddPasswordAsync(usuario.Id, model.Password);
                if (!nuevaPassword.Succeeded)
                    return ResultadoOperacion.DatosInvalidos(UnirErrores(nuevaPassword));
            }

            if (estabaActivo != model.Activo)
                await AplicarBloqueoSegunEstadoAsync(usuario.Id, model.Activo);

            return ResultadoOperacion.Ok("Se actualizaron los datos de " + usuario.NombreCompleto + ".");
        }

        public async Task<ResultadoOperacion> CambiarEstadoAsync(string usuarioId, bool activar, string administradorActualId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                return ResultadoOperacion.DatosInvalidos("Debe indicar el usuario.");

            var usuario = await _userManager.FindByIdAsync(usuarioId);
            if (usuario == null)
                return ResultadoOperacion.NoEncontrado("El usuario no existe.");

            var estaActivo = usuario.EstadoId == Estado.ActivoId;
            if (estaActivo == activar)
                return ResultadoOperacion.Conflicto(activar ? "El usuario ya está activo." : "El usuario ya está inactivo.");

            var rolesActuales = await _userManager.GetRolesAsync(usuario.Id);
            var rolActual = rolesActuales.FirstOrDefault();

            var reglas = await ValidarCambioDeRolOEstadoAsync(usuario, rolesActuales, rolActual, activar, administradorActualId);
            if (reglas != null)
                return reglas;

            usuario.EstadoId = activar ? Estado.ActivoId : Estado.InactivoId;
            var actualizado = await _userManager.UpdateAsync(usuario);
            if (!actualizado.Succeeded)
                return ResultadoOperacion.DatosInvalidos(UnirErrores(actualizado));

            await AplicarBloqueoSegunEstadoAsync(usuario.Id, activar);

            return ResultadoOperacion.Ok((activar ? "Se activó" : "Se inactivó") + " el usuario " + usuario.NombreCompleto + ".");
        }

        // ----------------------------------------------------------------- Reglas de negocio

        private static ResultadoOperacion ValidarFormulario(UsuarioFormModel model)
        {
            if (model == null)
                return ResultadoOperacion.DatosInvalidos("Los datos del usuario son obligatorios.");

            if (string.IsNullOrWhiteSpace(model.NombreCompleto) || string.IsNullOrWhiteSpace(model.Email))
                return ResultadoOperacion.DatosInvalidos("El nombre completo y el correo electrónico son obligatorios.");

            if (!RolesDisponibles.Contains(model.Rol))
                return ResultadoOperacion.DatosInvalidos("El rol seleccionado no es válido.");

            if (!string.IsNullOrEmpty(model.Password) && model.Password != model.ConfirmPassword)
                return ResultadoOperacion.DatosInvalidos("La contraseña y su confirmación no coinciden.");

            return null;
        }

        // Reglas que protegen la integridad del sistema al cambiar rol o estado.
        private async Task<ResultadoOperacion> ValidarCambioDeRolOEstadoAsync(
            ApplicationUser usuario,
            IList<string> rolesActuales,
            string rolNuevo,
            bool quedaActivo,
            string administradorActualId)
        {
            // 1. El administrador no puede inactivarse ni quitarse su propio rol (se quedaría sin acceso).
            if (usuario.Id == administradorActualId &&
                (!quedaActivo || rolNuevo != ApplicationRoleNames.Administrador))
            {
                return ResultadoOperacion.Conflicto("No puede inactivar su propia cuenta ni quitarse el rol de Administrador.");
            }

            // 2. Un guía con salidas futuras activas no puede inactivarse ni cambiar de rol:
            //    primero el administrador debe reasignar esas salidas a otro guía.
            var esGuia = rolesActuales.Contains(ApplicationRoleNames.Guia);
            var dejaDeSerGuiaActivo = !quedaActivo || rolNuevo != ApplicationRoleNames.Guia;

            if (esGuia && dejaDeSerGuiaActivo)
            {
                using (var unitOfWork = _unitOfWorkFactory())
                {
                    var ahora = DateTime.Now;
                    var usuarioId = usuario.Id;
                    var salidasFuturas = await unitOfWork.Repository<Salida>().Query()
                        .CountAsync(s => s.GuiaId == usuarioId
                                         && s.EstadoId == Estado.ActivoId
                                         && s.FechaHora > ahora);

                    if (salidasFuturas > 0)
                    {
                        return ResultadoOperacion.Conflicto(
                            "El guía tiene " + salidasFuturas + " salida(s) futura(s) asignada(s). " +
                            "Reasígnelas a otro guía antes de inactivarlo o cambiarle el rol.");
                    }
                }
            }

            return null;
        }

        // Un usuario inactivo queda bloqueado en Identity, así PasswordSignInAsync rechaza su
        // inicio de sesión; al activarlo se quita el bloqueo.
        private async Task AplicarBloqueoSegunEstadoAsync(string usuarioId, bool activo)
        {
            await _userManager.SetLockoutEnabledAsync(usuarioId, true);

            if (activo)
            {
                await _userManager.SetLockoutEndDateAsync(usuarioId, DateTimeOffset.MinValue);
                await _userManager.ResetAccessFailedCountAsync(usuarioId);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(usuarioId, BloqueoIndefinido);
                // Invalida la cookie de una sesión abierta (se revisa cada 30 minutos en Startup.Auth).
                await _userManager.UpdateSecurityStampAsync(usuarioId);
            }
        }

        private static string UnirErrores(IdentityResult resultado)
        {
            return string.Join(" ", resultado.Errors);
        }
    }
}