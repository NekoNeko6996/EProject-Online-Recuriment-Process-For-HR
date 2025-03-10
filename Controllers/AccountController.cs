using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Sem3EProjectOnlineCPFH.Libraries;
using Sem3EProjectOnlineCPFH.Models.Auth;
using System.Web;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;

        public AccountController() { }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public ApplicationUserManager UserManager => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationSignInManager SignInManager => _signInManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationSignInManager>();
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;


        // GET: Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                PhoneNumber = model.PhoneNumber
            };

            var result = await UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await UserManager.AddToRoleAsync(user.Id, "candidate");

                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                return RedirectToAction("Index", "Home");
            }

            AddErrors(result);
            return View(model);
        }


        // GET: Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            if (result == SignInStatus.Success)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Đăng nhập không hợp lệ.");
            return View(model);
        }

        // GET: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);
        }
    }
}