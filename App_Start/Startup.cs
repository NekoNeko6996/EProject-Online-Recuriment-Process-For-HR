using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Sem3EProjectOnlineCPFH.Libraries;
using Sem3EProjectOnlineCPFH.Middleware;
using Sem3EProjectOnlineCPFH.Models;
using Sem3EProjectOnlineCPFH.Models.Auth;

[assembly: OwinStartup(typeof(Sem3EProjectOnlineCPFH.Startup))]

namespace Sem3EProjectOnlineCPFH
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Cấu hình OWIN Contexts
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

            ConfigureAuth(app);

            // Middleware kiểm tra active
            app.Use<ActiveUserMiddleware>();
        }
    }

}
