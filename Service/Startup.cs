using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(nkpnotificationsdemoService.Startup))]

namespace nkpnotificationsdemoService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}