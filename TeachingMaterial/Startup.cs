using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TeachingMaterial.Startup))]
namespace TeachingMaterial
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
