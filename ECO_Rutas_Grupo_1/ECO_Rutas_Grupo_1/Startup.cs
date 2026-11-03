using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ECO_Rutas_Grupo_1.Startup))]
namespace ECO_Rutas_Grupo_1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
