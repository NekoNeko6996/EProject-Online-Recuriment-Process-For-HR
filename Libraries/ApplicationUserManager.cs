using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Sem3EProjectOnlineCPFH.Models.Auth;
using Sem3EProjectOnlineCPFH.Models;
using Microsoft.Owin.Security;

namespace Sem3EProjectOnlineCPFH.Libraries
{
	public class ApplicationUserManager : UserManager<ApplicationUser>
	{
		public ApplicationUserManager(IUserStore<ApplicationUser> store) : base(store) { }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            return manager;
        }
    }

    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) : base(userManager, authenticationManager) { }
        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}