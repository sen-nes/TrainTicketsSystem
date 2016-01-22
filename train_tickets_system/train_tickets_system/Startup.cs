using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(train_tickets_system.Startup))]
namespace train_tickets_system
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
