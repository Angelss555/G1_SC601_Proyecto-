namespace EcoRutas.Data.Migrations
{
    using EcoRutas.Data.Context;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<EcoRutas.Data.Context.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(EcoRutas.Data.Context.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.
            ApplicationDbSeeder.SeedReferenceData(context);

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
