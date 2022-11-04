using eShopWebForms.Middleware;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(eShopWebForms.Startup))]

namespace eShopWebForms
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            if (CatalogConfiguration.UseAzureActiveDirectory)
            {
                ConfigureAuth(app);
            }
            else
            {
                app.Use<AuthenticationMiddleware>();
            }
        }
    }
}
