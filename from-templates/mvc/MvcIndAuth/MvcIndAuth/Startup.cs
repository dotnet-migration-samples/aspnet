using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MvcIndAuth.Startup))]
namespace MvcIndAuth
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
