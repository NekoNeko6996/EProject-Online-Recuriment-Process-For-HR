using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Sem3EProjectOnlineCPFH.Libraries;
using Sem3EProjectOnlineCPFH.Models.Auth;
using Sem3EProjectOnlineCPFH.Models;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Linq;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationSignInManager _signInManager;
        private ApplicationRoleManager _roleManager;

        public AccountController() { }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                if (_roleManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ RoleManager is NULL! Initializing again...");
                    _roleManager = HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
                }
                return _roleManager;
            }
            private set { _roleManager = value; }
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
            // Kiểm tra Model có hợp lệ không
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine("Validation Error: " + error.ErrorMessage);
                }
                return View(model);
            }

            // Tạo User trước
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    System.Diagnostics.Debug.WriteLine("UserManager Error: " + error);
                }
                AddErrors(result);
                return View(model);
            }

            // Tạo hồ sơ UserProfile sau khi User được tạo thành công
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userProfile = new UserProfile
                    {
                        Id = user.Id, // Sử dụng User.Id làm khóa chính
                        AvatarUrl = "~/Content/Resources/DefaultImg.png",
                        Bio = "Chưa có thông tin nào."
                    };

                    db.UserProfiles.Add(userProfile);
                    db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine("UserProfile created successfully for: " + user.Email);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DB Error: " + ex.Message);
                return View(model);
            }

            // Kiểm tra Role "candidate" trước khi gán
            try
            {
                if (!await RoleManager.RoleExistsAsync("candidate"))
                {
                    System.Diagnostics.Debug.WriteLine("Role 'candidate' does not exist. Creating...");
                    await RoleManager.CreateAsync(new IdentityRole("candidate"));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Role 'candidate' already exists.");
                }

                await UserManager.AddToRoleAsync(user.Id, "candidate");
                System.Diagnostics.Debug.WriteLine("User added to 'candidate' role.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RoleManager Error: " + ex.Message);
                return View(model);
            }

            // Đăng nhập người dùng sau khi đăng ký
            try
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                System.Diagnostics.Debug.WriteLine("User signed in successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SignInManager Error: " + ex.Message);
            }

            return RedirectToAction("Index", "Home");
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

            TempData["ErrorMessage"] = "Invalid Email or Password";
            return View(model);
        }

        //POST: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            try
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Logout Error: " + ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);
        }

        // GET: Account Setting
        [Authorize]
        public ActionResult Setting()
        {
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                if (user == null)
                {
                    return HttpNotFound();
                }
                var model = new ProfileViewModel
                {
                    AvatarUrl = user.UserProfile.AvatarUrl,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Bio = user.UserProfile.Bio,
                    PhoneNumber = user.PhoneNumber,
                    SocialAccount1 = user.UserProfile.SocialAccount1,
                    SocialAccount2 = user.UserProfile.SocialAccount2,
                    SocialAccount3 = user.UserProfile.SocialAccount3,
                    Address = user.Address,
                    City = user.City,
                    Country = user.Country
                };

                ProfileSettingsViewModel profileSettings = new ProfileSettingsViewModel
                {
                    ProfileUpdate = model,
                    ChangePassword = new ChangePasswordViewModel()
                };

                return View(profileSettings);
            }
        }

        // POST: Account Setting [update profile]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine("Validation Error: " + error.ErrorMessage);
                }
                TempData["ErrorMessage"] = "Invalid input, Please Try Again...";
                return View("Setting", new ProfileSettingsViewModel { ProfileUpdate = model.ProfileUpdate});
            }

            // Update profile
            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                if (user == null)
                {
                    return HttpNotFound();
                }
                user.FirstName = model.ProfileUpdate.FirstName;
                user.LastName = model.ProfileUpdate.LastName;
                user.Email = model.ProfileUpdate.Email;
                user.PhoneNumber = model.ProfileUpdate.PhoneNumber;
                user.Address = model.ProfileUpdate.Address;
                user.City = model.ProfileUpdate.City;
                user.Country = model.ProfileUpdate.Country;
                user.UserProfile.AvatarUrl = model.ProfileUpdate.AvatarUrl;
                user.UserProfile.Bio = model.ProfileUpdate.Bio;
                user.UserProfile.SocialAccount1 = model.ProfileUpdate.SocialAccount1;
                user.UserProfile.SocialAccount2 = model.ProfileUpdate.SocialAccount2;
                user.UserProfile.SocialAccount3 = model.ProfileUpdate.SocialAccount3;
                db.SaveChanges();
                System.Diagnostics.Debug.WriteLine("Profile updated successfully for: " + user.Email);
            }
            TempData["SuccessMessage"] = "Profile updated successfully...";
            return RedirectToAction("Setting");
        }

        // POST: Account Setting [change password]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ProfileSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fix the errors below.";
                return RedirectToAction("Setting");
            }

            try
            {
                var userId = User.Identity.GetUserId();
                var result = await UserManager.ChangePasswordAsync(userId, model.ChangePassword.OldPassword, model.ChangePassword.NewPassword);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Password changed successfully!";
                    return RedirectToAction("Setting");
                }

                TempData["ErrorMessage"] = string.Join("<br/>", result.Errors);
                return RedirectToAction("Setting");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction("Setting");
            }
        }

    }
}