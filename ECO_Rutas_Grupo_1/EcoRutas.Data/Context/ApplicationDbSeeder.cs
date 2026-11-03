using System;
using System.Data.Entity.Migrations;
using System.Linq;
using EcoRutas.Data.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EcoRutas.Data.Context
{
    public static class ApplicationDbSeeder
    {
        public static void SeedReferenceData(ApplicationDbContext context)
        {
            SeedStates(context);
            SeedRoles(context);

            var guia = EnsureUser(
                context,
                "guia@ecorutas.local",
                "Guía de prueba",
                "Guia#2026",
                ApplicationRoleNames.Guia);

            EnsureUser(
                context,
                "admin@ecorutas.local",
                "Administrador de prueba",
                "Admin#2026",
                ApplicationRoleNames.Administrador);

            EnsureUser(
                context,
                "asociado@ecorutas.local",
                "Asociado de prueba",
                "Asoc#2026",
                ApplicationRoleNames.Asociado);

            var categoria = EnsureCategoria(context, "Naturaleza");
            var dificultad = EnsureDificultad(context, "Moderada");
            var tour = EnsureTour(context, categoria, dificultad);

            EnsureDemoImages(context, tour);
            EnsureDemoSalida(context, tour, guia, DateTime.Today.AddDays(-7).AddHours(9));
            EnsureDemoSalida(context, tour, guia, DateTime.Today.AddHours(9));
            EnsureDemoSalida(context, tour, guia, DateTime.Today.AddDays(7).AddHours(9));

            context.SaveChanges();
        }

        private static void SeedStates(ApplicationDbContext context)
        {
            context.Estados.AddOrUpdate(
                estado => estado.EstadoId,
                new Estado
                {
                    EstadoId = Estado.ActivoId,
                    Nombre = "Activo",
                    Descripcion = "Registro activo"
                },
                new Estado
                {
                    EstadoId = Estado.InactivoId,
                    Nombre = "Inactivo",
                    Descripcion = "Registro inactivo"
                });

            context.EstadosDetalleReserva.AddOrUpdate(
                estado => estado.EstadoDetalleReservaId,
                new EstadoDetalleReserva
                {
                    EstadoDetalleReservaId = EstadoDetalleReserva.ConfirmadaId,
                    Nombre = "Confirmada"
                },
                new EstadoDetalleReserva
                {
                    EstadoDetalleReservaId = EstadoDetalleReserva.CanceladaId,
                    Nombre = "Cancelada"
                },
                new EstadoDetalleReserva
                {
                    EstadoDetalleReservaId = EstadoDetalleReserva.CompletadaId,
                    Nombre = "Completada"
                },
                new EstadoDetalleReserva
                {
                    EstadoDetalleReservaId = EstadoDetalleReserva.NoPresentadaId,
                    Nombre = "No presentada"
                });

            context.SaveChanges();
        }

        private static void SeedRoles(ApplicationDbContext context)
        {
            EnsureRole(context, ApplicationRoleNames.Administrador);
            EnsureRole(context, ApplicationRoleNames.Asociado);
            EnsureRole(context, ApplicationRoleNames.Guia);
            context.SaveChanges();
        }

        private static void EnsureRole(ApplicationDbContext context, string roleName)
        {
            if (!context.Roles.Any(role => role.Name == roleName))
            {
                context.Roles.Add(new IdentityRole(roleName));
            }
        }

        private static ApplicationUser EnsureUser(
            ApplicationDbContext context,
            string email,
            string fullName,
            string password,
            string roleName)
        {
            var store = new UserStore<ApplicationUser>(context)
            {
                DisposeContext = false
            };

            var userManager = new UserManager<ApplicationUser>(store);
            var user = userManager.FindByName(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    NombreCompleto = fullName,
                    EstadoId = Estado.ActivoId,
                    FechaRegistro = DateTime.Now
                };

                var createResult = userManager.Create(user, password);

                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "No se pudo crear el usuario " + email + ": " +
                        string.Join("; ", createResult.Errors));
                }
            }

            if (!userManager.IsInRole(user.Id, roleName))
            {
                var roleResult = userManager.AddToRole(user.Id, roleName);

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "No se pudo asignar el rol " + roleName + " al usuario " +
                        email + ": " + string.Join("; ", roleResult.Errors));
                }
            }

            return user;
        }

        private static Categoria EnsureCategoria(
            ApplicationDbContext context,
            string nombre)
        {
            var categoria = context.Categorias
                .SingleOrDefault(item => item.Nombre == nombre);

            if (categoria == null)
            {
                categoria = new Categoria { Nombre = nombre };
                context.Categorias.Add(categoria);
                context.SaveChanges();
            }

            return categoria;
        }

        private static Dificultad EnsureDificultad(
            ApplicationDbContext context,
            string nombre)
        {
            var dificultad = context.Dificultades
                .SingleOrDefault(item => item.Nombre == nombre);

            if (dificultad == null)
            {
                dificultad = new Dificultad { Nombre = nombre };
                context.Dificultades.Add(dificultad);
                context.SaveChanges();
            }

            return dificultad;
        }

        private static Tour EnsureTour(
            ApplicationDbContext context,
            Categoria categoria,
            Dificultad dificultad)
        {
            const string codigoTour = "TOUR-DEMO-001";

            var tour = context.Tours
                .SingleOrDefault(item => item.CodigoTour == codigoTour);

            if (tour == null)
            {
                tour = new Tour
                {
                    CodigoTour = codigoTour,
                    Nombre = "Caminata de prueba en la naturaleza",
                    Descripcion = "Tour de ejemplo para verificar las salidas de prueba.",
                    DuracionMinutos = 180,
                    PuntoEncuentro = "Entrada principal del parque",
                    EstadoId = Estado.ActivoId,
                    CategoriaId = categoria.CategoriaId,
                    DificultadId = dificultad.DificultadId
                };

                context.Tours.Add(tour);
                context.SaveChanges();
            }

            return tour;
        }

        private static void EnsureDemoImages(
            ApplicationDbContext context,
            Tour tour)
        {
            // Imagen PNG mínima de prueba. Se puede reemplazar por imágenes reales
            // cuando se implemente la carga de imágenes del módulo de tours.
            var imageBytes = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jXioAAAAASUVORK5CYII=");

            for (var order = 1; order <= 3; order++)
            {
                var currentOrder = order;

                var exists = context.ImagenesTour.Any(image =>
                    image.TourId == tour.TourId &&
                    image.Orden == currentOrder);

                if (!exists)
                {
                    context.ImagenesTour.Add(new ImagenTour
                    {
                        TourId = tour.TourId,
                        NombreArchivo = "imagen-demo-" + currentOrder + ".png",
                        TipoMime = "image/png",
                        Contenido = imageBytes,
                        Orden = currentOrder
                    });
                }
            }

            context.SaveChanges();
        }

        private static void EnsureDemoSalida(
            ApplicationDbContext context,
            Tour tour,
            ApplicationUser guia,
            DateTime fechaHora)
        {
            var exists = context.Salidas.Any(salida =>
                salida.TourId == tour.TourId &&
                salida.FechaHora == fechaHora);

            if (!exists)
            {
                context.Salidas.Add(new Salida
                {
                    TourId = tour.TourId,
                    GuiaId = guia.Id,
                    EstadoId = Estado.ActivoId,
                    FechaHora = fechaHora,
                    PrecioPorPersona = 15000m,
                    CupoMaximo = 12,
                    CuposDisponibles = 12,
                    ListaCerrada = false
                });

                context.SaveChanges();
            }
        }
    }
}
