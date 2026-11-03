using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using EcoRutas.Data.Entities;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EcoRutas.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Estado> Estados { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Dificultad> Dificultades { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Salida> Salidas { get; set; }
        public DbSet<ImagenTour> ImagenesTour { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<EstadoDetalleReserva> EstadosDetalleReserva { get; set; }
        public DbSet<DetalleReserva> DetallesReserva { get; set; }
        public DbSet<Pase> Pases { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.CodigoUsuario)
                .IsUnique();
            modelBuilder.Entity<ApplicationUser>()
                .HasRequired(u => u.Estado)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EstadoId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Tour>()
                .HasIndex(t => t.CodigoTour)
                .IsUnique();
            modelBuilder.Entity<Tour>()
                .HasRequired(t => t.Estado)
                .WithMany(e => e.Tours)
                .HasForeignKey(t => t.EstadoId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Tour>()
                .HasRequired(t => t.Categoria)
                .WithMany(c => c.Tours)
                .HasForeignKey(t => t.CategoriaId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Tour>()
                .HasRequired(t => t.Dificultad)
                .WithMany(d => d.Tours)
                .HasForeignKey(t => t.DificultadId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Salida>()
                .Property(s => s.PrecioPorPersona)
                .HasPrecision(10, 2);
            modelBuilder.Entity<Salida>()
                .HasRequired(s => s.Tour)
                .WithMany(t => t.Salidas)
                .HasForeignKey(s => s.TourId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Salida>()
                .HasRequired(s => s.Estado)
                .WithMany(e => e.Salidas)
                .HasForeignKey(s => s.EstadoId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Salida>()
                .HasRequired(s => s.Guia)
                .WithMany(u => u.SalidasAsignadas)
                .HasForeignKey(s => s.GuiaId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ImagenTour>()
                .HasRequired(i => i.Tour)
                .WithMany(t => t.Imagenes)
                .HasForeignKey(i => i.TourId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<ImagenTour>()
                .HasIndex(i => new { i.TourId, i.Orden })
                .IsUnique();

            modelBuilder.Entity<Reserva>()
                .HasRequired(r => r.Usuario)
                .WithMany(u => u.Reservas)
                .HasForeignKey(r => r.UsuarioId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DetalleReserva>()
                .Property(d => d.PrecioPorPersona)
                .HasPrecision(10, 2);
            modelBuilder.Entity<DetalleReserva>()
                .HasRequired(d => d.Reserva)
                .WithMany(r => r.Detalles)
                .HasForeignKey(d => d.ReservaId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<DetalleReserva>()
                .HasRequired(d => d.Salida)
                .WithMany(s => s.DetallesReserva)
                .HasForeignKey(d => d.SalidaId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<DetalleReserva>()
                .HasRequired(d => d.EstadoDetalleReserva)
                .WithMany(e => e.Detalles)
                .HasForeignKey(d => d.EstadoDetalleReservaId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<DetalleReserva>()
                .HasIndex(d => new { d.ReservaId, d.SalidaId })
                .IsUnique();

            modelBuilder.Entity<Pase>()
                .HasIndex(p => p.CodigoPase)
                .IsUnique();
            modelBuilder.Entity<Pase>()
                .HasRequired(p => p.DetalleReserva)
                .WithMany(d => d.Pases)
                .HasForeignKey(p => p.DetalleReservaId)
                .WillCascadeOnDelete(false);
        }
    }
}
