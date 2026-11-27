namespace EcoRutas.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AgregarResenas : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ECO_RESENA_TB",
                c => new
                    {
                        ResenaId = c.Int(nullable: false, identity: true),
                        UsuarioId = c.String(nullable: false, maxLength: 128),
                        TourId = c.Int(nullable: false),
                        Calificacion = c.Int(nullable: false),
                        Comentario = c.String(nullable: false, maxLength: 1000),
                        Fecha = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ResenaId)
                .ForeignKey("dbo.AspNetUsers", t => t.UsuarioId)
                .ForeignKey("dbo.ECO_TOUR_TB", t => t.TourId)
                .Index(t => new { t.UsuarioId, t.TourId }, unique: true)
                .Index(t => t.TourId);
        }

        public override void Down()
        {
            DropIndex("dbo.ECO_RESENA_TB", new[] { "TourId" });
            DropIndex("dbo.ECO_RESENA_TB", new[] { "UsuarioId", "TourId" });
            DropForeignKey("dbo.ECO_RESENA_TB", "TourId", "dbo.ECO_TOUR_TB");
            DropForeignKey("dbo.ECO_RESENA_TB", "UsuarioId", "dbo.AspNetUsers");
            DropTable("dbo.ECO_RESENA_TB");
        }
    }
}
