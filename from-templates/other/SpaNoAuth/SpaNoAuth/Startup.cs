using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: OwinStartup(typeof(SpaNoAuth.Startup))]

namespace SpaNoAuth {
    public partial class Startup {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
