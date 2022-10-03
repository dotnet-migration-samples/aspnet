using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebFormsIndAuth.Startup))]
namespace WebFormsIndAuth
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
