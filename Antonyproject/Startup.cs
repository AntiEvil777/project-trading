using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Antonyproject.Startup))]
namespace Antonyproject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
