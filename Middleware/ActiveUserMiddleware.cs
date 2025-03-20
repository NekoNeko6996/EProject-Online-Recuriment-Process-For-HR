using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using System.Linq;
using System.Threading.Tasks;
using Sem3EProjectOnlineCPFH.Models;

namespace Sem3EProjectOnlineCPFH.Middleware
{
    public class ActiveUserMiddleware : OwinMiddleware
    {
        public ActiveUserMiddleware(OwinMiddleware next) : base(next) { }

        public override async Task Invoke(IOwinContext context)
        {
            var user = context.Authentication?.User;

            if (user != null && user.Identity.IsAuthenticated)
            {
                var userId = user.Identity.GetUserId();
                if (!string.IsNullOrEmpty(userId))
                {
                    using (var db = new ApplicationDbContext())
                    {
                        var dbUser = db.Users.FirstOrDefault(u => u.Id == userId);
                        if (dbUser != null && !dbUser.IsActive)
                        {
                            context.Authentication.SignOut();
                            context.Response.Redirect("/Account/Login?disabled=true");
                            return;
                        }
                    }
                }
            }

            await Next.Invoke(context);
        }
    }
}
