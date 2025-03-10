using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Sem3EProjectOnlineCPFH.Libraries;
using Sem3EProjectOnlineCPFH.Models;

[assembly: OwinStartup(typeof(Sem3EProjectOnlineCPFH.Startup))]

namespace Sem3EProjectOnlineCPFH
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                LogoutPath = new PathString("/Account/Logout"),
                ExpireTimeSpan = System.TimeSpan.FromDays(14),
                SlidingExpiration = true
            });
        }
    }
}
