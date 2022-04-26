using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MembersManager.Startup))]
namespace MembersManager
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
