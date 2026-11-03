namespace EcoRutas.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Inicial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ECO_CATEGORIA_TB",
                c => new
                    {
                        CategoriaId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 60),
                    })
                .PrimaryKey(t => t.CategoriaId);
            
            CreateTable(
                "dbo.ECO_TOUR_TB",
                c => new
                    {
                        TourId = c.Int(nullable: false, identity: true),
                        EstadoId = c.Int(nullable: false),
                        CategoriaId = c.Int(nullable: false),
                        DificultadId = c.Int(nullable: false),
                        CodigoTour = c.String(nullable: false, maxLength: 20),
                        Nombre = c.String(nullable: false, maxLength: 120),
                        Descripcion = c.String(nullable: false, maxLength: 1000),
                        DuracionMinutos = c.Int(nullable: false),
                        PuntoEncuentro = c.String(nullable: false, maxLength: 200),
                    })
                .PrimaryKey(t => t.TourId)
                .ForeignKey("dbo.ECO_CATEGORIA_TB", t => t.CategoriaId)
                .ForeignKey("dbo.ECO_DIFICULTAD_TB", t => t.DificultadId)
                .ForeignKey("dbo.ECO_ESTADO_TB", t => t.EstadoId)
                .Index(t => t.EstadoId)
                .Index(t => t.CategoriaId)
                .Index(t => t.DificultadId)
                .Index(t => t.CodigoTour, unique: true);
            
            CreateTable(
                "dbo.ECO_DIFICULTAD_TB",
                c => new
                    {
                        DificultadId = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 60),
                    })
                .PrimaryKey(t => t.DificultadId);
            
            CreateTable(
                "dbo.ECO_ESTADO_TB",
                c => new
                    {
                        EstadoId = c.Int(nullable: false),
                        Nombre = c.String(nullable: false, maxLength: 30),
                        Descripcion = c.String(maxLength: 150),
                    })
                .PrimaryKey(t => t.EstadoId);
            
            CreateTable(
                "dbo.ECO_SALIDA_TB",
                c => new
                    {
                        SalidaId = c.Int(nullable: false, identity: true),
                        TourId = c.Int(nullable: false),
                        EstadoId = c.Int(nullable: false),
                        GuiaId = c.String(nullable: false, maxLength: 128),
                        FechaHora = c.DateTime(nullable: false),
                        PrecioPorPersona = c.Decimal(nullable: false, precision: 10, scale: 2),
                        CupoMaximo = c.Int(nullable: false),
                        CuposDisponibles = c.Int(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        ListaCerrada = c.Boolean(nullable: false),
                        FechaCierre = c.DateTime(),
                    })
                .PrimaryKey(t => t.SalidaId)
                .ForeignKey("dbo.ECO_ESTADO_TB", t => t.EstadoId)
                .ForeignKey("dbo.AspNetUsers", t => t.GuiaId)
                .ForeignKey("dbo.ECO_TOUR_TB", t => t.TourId)
                .Index(t => t.TourId)
                .Index(t => t.EstadoId)
                .Index(t => t.GuiaId);
            
            CreateTable(
                "dbo.ECO_DETALLE_TB",
                c => new
                    {
                        DetalleReservaId = c.Int(nullable: false, identity: true),
                        ReservaId = c.Int(nullable: false),
                        SalidaId = c.Int(nullable: false),
                        EstadoDetalleReservaId = c.Int(nullable: false),
                        CantidadPasajeros = c.Int(nullable: false),
                        PrecioPorPersona = c.Decimal(nullable: false, precision: 10, scale: 2),
                        FechaCancelacion = c.DateTime(),
                    })
                .PrimaryKey(t => t.DetalleReservaId)
                .ForeignKey("dbo.ECO_ESTADO_DETALLE_TB", t => t.EstadoDetalleReservaId)
                .ForeignKey("dbo.ECO_RESERVA_TB", t => t.ReservaId)
                .ForeignKey("dbo.ECO_SALIDA_TB", t => t.SalidaId)
                .Index(t => new { t.ReservaId, t.SalidaId }, unique: true)
                .Index(t => t.EstadoDetalleReservaId);
            
            CreateTable(
                "dbo.ECO_ESTADO_DETALLE_TB",
                c => new
                    {
                        EstadoDetalleReservaId = c.Int(nullable: false),
                        Nombre = c.String(nullable: false, maxLength: 30),
                    })
                .PrimaryKey(t => t.EstadoDetalleReservaId);
            
            CreateTable(
                "dbo.ECO_PASE_TB",
                c => new
                    {
                        PaseId = c.Int(nullable: false, identity: true),
                        DetalleReservaId = c.Int(nullable: false),
                        CodigoPase = c.String(nullable: false, maxLength: 36),
                        NombrePasajero = c.String(nullable: false, maxLength: 120),
                        Identificacion = c.String(nullable: false, maxLength: 30),
                        Asistio = c.Boolean(nullable: false),
                        Anulado = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PaseId)
                .ForeignKey("dbo.ECO_DETALLE_TB", t => t.DetalleReservaId)
                .Index(t => t.DetalleReservaId)
                .Index(t => t.CodigoPase, unique: true);
            
            CreateTable(
                "dbo.ECO_RESERVA_TB",
                c => new
                    {
                        ReservaId = c.Int(nullable: false, identity: true),
                        UsuarioId = c.String(nullable: false, maxLength: 128),
                        FechaReserva = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ReservaId)
                .ForeignKey("dbo.AspNetUsers", t => t.UsuarioId)
                .Index(t => t.UsuarioId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CodigoUsuario = c.String(nullable: false, maxLength: 32),
                        NombreCompleto = c.String(nullable: false, maxLength: 120),
                        EstadoId = c.Int(nullable: false),
                        FechaRegistro = c.DateTime(nullable: false),
                        UltimaConexion = c.DateTime(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ECO_ESTADO_TB", t => t.EstadoId)
                .Index(t => t.CodigoUsuario, unique: true)
                .Index(t => t.EstadoId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.ECO_IMAGEN_TB",
                c => new
                    {
                        ImagenTourId = c.Int(nullable: false, identity: true),
                        TourId = c.Int(nullable: false),
                        NombreArchivo = c.String(maxLength: 255),
                        TipoMime = c.String(nullable: false, maxLength: 100),
                        Contenido = c.Binary(nullable: false),
                        Orden = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ImagenTourId)
                .ForeignKey("dbo.ECO_TOUR_TB", t => t.TourId)
                .Index(t => new { t.TourId, t.Orden }, unique: true);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.ECO_IMAGEN_TB", "TourId", "dbo.ECO_TOUR_TB");
            DropForeignKey("dbo.ECO_TOUR_TB", "EstadoId", "dbo.ECO_ESTADO_TB");
            DropForeignKey("dbo.ECO_SALIDA_TB", "TourId", "dbo.ECO_TOUR_TB");
            DropForeignKey("dbo.ECO_SALIDA_TB", "GuiaId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ECO_SALIDA_TB", "EstadoId", "dbo.ECO_ESTADO_TB");
            DropForeignKey("dbo.ECO_DETALLE_TB", "SalidaId", "dbo.ECO_SALIDA_TB");
            DropForeignKey("dbo.ECO_DETALLE_TB", "ReservaId", "dbo.ECO_RESERVA_TB");
            DropForeignKey("dbo.ECO_RESERVA_TB", "UsuarioId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "EstadoId", "dbo.ECO_ESTADO_TB");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ECO_PASE_TB", "DetalleReservaId", "dbo.ECO_DETALLE_TB");
            DropForeignKey("dbo.ECO_DETALLE_TB", "EstadoDetalleReservaId", "dbo.ECO_ESTADO_DETALLE_TB");
            DropForeignKey("dbo.ECO_TOUR_TB", "DificultadId", "dbo.ECO_DIFICULTAD_TB");
            DropForeignKey("dbo.ECO_TOUR_TB", "CategoriaId", "dbo.ECO_CATEGORIA_TB");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.ECO_IMAGEN_TB", new[] { "TourId", "Orden" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "EstadoId" });
            DropIndex("dbo.AspNetUsers", new[] { "CodigoUsuario" });
            DropIndex("dbo.ECO_RESERVA_TB", new[] { "UsuarioId" });
            DropIndex("dbo.ECO_PASE_TB", new[] { "CodigoPase" });
            DropIndex("dbo.ECO_PASE_TB", new[] { "DetalleReservaId" });
            DropIndex("dbo.ECO_DETALLE_TB", new[] { "EstadoDetalleReservaId" });
            DropIndex("dbo.ECO_DETALLE_TB", new[] { "ReservaId", "SalidaId" });
            DropIndex("dbo.ECO_SALIDA_TB", new[] { "GuiaId" });
            DropIndex("dbo.ECO_SALIDA_TB", new[] { "EstadoId" });
            DropIndex("dbo.ECO_SALIDA_TB", new[] { "TourId" });
            DropIndex("dbo.ECO_TOUR_TB", new[] { "CodigoTour" });
            DropIndex("dbo.ECO_TOUR_TB", new[] { "DificultadId" });
            DropIndex("dbo.ECO_TOUR_TB", new[] { "CategoriaId" });
            DropIndex("dbo.ECO_TOUR_TB", new[] { "EstadoId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.ECO_IMAGEN_TB");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.ECO_RESERVA_TB");
            DropTable("dbo.ECO_PASE_TB");
            DropTable("dbo.ECO_ESTADO_DETALLE_TB");
            DropTable("dbo.ECO_DETALLE_TB");
            DropTable("dbo.ECO_SALIDA_TB");
            DropTable("dbo.ECO_ESTADO_TB");
            DropTable("dbo.ECO_DIFICULTAD_TB");
            DropTable("dbo.ECO_TOUR_TB");
            DropTable("dbo.ECO_CATEGORIA_TB");
        }
    }
}
