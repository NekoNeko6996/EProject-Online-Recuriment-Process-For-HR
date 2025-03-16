using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Sem3EProjectOnlineCPFH.Libraries;
using Sem3EProjectOnlineCPFH.Models.Auth;
using Sem3EProjectOnlineCPFH.Models;
using System.Web;
using System;
using System.Linq;
using Sem3EProjectOnlineCPFH.Services;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    [Authorize]
    public class AccountController : BaseController
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
                    System.Diagnostics.Debug.WriteLine("RoleManager is NULL! Initializing again...");
                    _roleManager = HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
                }
                return _roleManager;
            }
            private set { _roleManager = value; }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            var identity = await user.GenerateUserIdentityAsync(UserManager);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,

                // cookie time [not click remember me in login]
                ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(1)
            };

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(authProperties, identity);
        }


        public ApplicationUserManager UserManager => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        public ApplicationSignInManager SignInManager => _signInManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationSignInManager>();
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;


        // GET: Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var isFirstUser = !db.Users.Any();

            if (!isFirstUser)
            {
                ViewBag.Page = "Index";
                ViewBag.Controller = "Home";

                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var isFirstUser = !db.Users.Any();
            
            if(!isFirstUser)
            {
                SetDefaultPage(null);
                return RedirectToAction(ViewBag.Page, ViewBag.Controller);
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine("Validation Error: " + error.ErrorMessage);
                }
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = false
            };

            var result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    System.Diagnostics.Debug.WriteLine("Create User Error: " + error);
                }
                AddErrors(result);
                return View(model);
            }

            try
            {
                // Tạo profile user
                var userProfile = new UserProfile
                {
                    Id = user.Id,
                    AvatarUrl = "~/Content/Resources/DefaultImg.png",
                };

                db.UserProfiles.Add(userProfile);
                db.SaveChanges();

                await UserManager.AddToRoleAsync(user.Id, "admin");
                System.Diagnostics.Debug.WriteLine("First user created, assigned as 'admin'");
                SetDefaultPage("admin");

                TempData["SuccessMessage"] = "Account created successfully!";
                return RedirectToAction(ViewBag.Page, ViewBag.Controller);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DB Error: " + ex.Message);
                return View(model);
            }
        }

        // GET: Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            if(User.Identity.IsAuthenticated)
            {
                string roleName = UserManager.GetRoles(User.Identity.GetUserId()).FirstOrDefault();
                SetDefaultPage(roleName);
                RedirectToAction(ViewBag.Page, ViewBag.Controller);
            }

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

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user != null && await UserManager.CheckPasswordAsync(user, model.Password))
            {
                await SignInAsync(user, model.RememberMe); // Gọi trực tiếp SignInAsync

                var roles = await UserManager.GetRolesAsync(user.Id);
                var roleName = roles.FirstOrDefault();

                System.Diagnostics.Debug.WriteLine("User logged in: " + user.Email + " Role Name: " + roleName);

                switch (roleName)
                {
                    case "admin":
                        SetDefaultPage("admin");
                        return RedirectToAction("Index", "Admin");

                    case "hrgroup":
                        SetDefaultPage("hrgroup");
                        return RedirectToAction("Index", "Home");

                    case "interviewer":
                        SetDefaultPage("interviewer");
                        return RedirectToAction("Index", "Home");

                    default:
                        SetDefaultPage(null);
                        return RedirectToAction("Unauthorized", "Account");
                }
            }

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
        public ActionResult ProfileSetting()
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
                return View("ProfileSetting", new ProfileSettingsViewModel { ProfileUpdate = model.ProfileUpdate});
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
                user.PhoneNumber = model.ProfileUpdate.PhoneNumber;
                user.UserProfile.Bio = model.ProfileUpdate.Bio;
                user.UserProfile.SocialAccount1 = model.ProfileUpdate.SocialAccount1;
                user.UserProfile.SocialAccount2 = model.ProfileUpdate.SocialAccount2;
                user.UserProfile.SocialAccount3 = model.ProfileUpdate.SocialAccount3;
                db.SaveChanges();
                System.Diagnostics.Debug.WriteLine("Profile updated successfully for: " + user.Email);
            }
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("ProfileSetting");
        }

        // POST: Account Setting [change password]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ProfileSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid Input";
                return RedirectToAction("ProfileSetting");
            }

            try
            {
                var userId = User.Identity.GetUserId();
                var result = await UserManager.ChangePasswordAsync(userId, model.ChangePassword.OldPassword, model.ChangePassword.NewPassword);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Password changed successfully!";
                    return RedirectToAction("ProfileSetting");
                }

                TempData["ErrorMessage"] = string.Join("<br/>", result.Errors);
                return RedirectToAction("ProfileSetting");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction("ProfileSetting");
            }
        }

        // POST: Account Setting [upload avatar]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadAvatar(HttpPostedFileBase avatar)
        {
            if (avatar == null || avatar.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "No file selected or file is empty.";
                return RedirectToAction("ProfileSetting");
            }

            if (avatar.ContentLength > 5 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "File size must be less than 5MB.";
                return RedirectToAction("ProfileSetting");
            }

            if (!avatar.ContentType.StartsWith("image/"))
            {
                TempData["ErrorMessage"] = "Invalid file type. Only images are allowed.";
                return RedirectToAction("ProfileSetting");
            }

            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("ProfileSetting");
                }

                string oldAvatarPath = Server.MapPath(user.UserProfile.AvatarUrl);

                // Tạo tên file mới
                string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(avatar.FileName);
                string newAvatarPath = System.IO.Path.Combine(Server.MapPath("~/Content/Resources/Avatars"), fileName);

                try
                {
                    // **Xóa avatar cũ nếu không phải ảnh mặc định**
                    if (!string.IsNullOrEmpty(user.UserProfile.AvatarUrl) &&
                        System.IO.File.Exists(oldAvatarPath) &&
                        !user.UserProfile.AvatarUrl.Contains("DefaultImg.png"))
                    {
                        System.IO.File.Delete(oldAvatarPath);
                        System.Diagnostics.Debug.WriteLine($"Deleted old avatar: {oldAvatarPath}");
                    }

                    // Lưu ảnh mới
                    avatar.SaveAs(newAvatarPath);
                    user.UserProfile.AvatarUrl = "/Content/Resources/Avatars/" + fileName; // Lưu đường dẫn tương đối
                    db.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"Avatar updated successfully for: {user.Email}");
                    TempData["SuccessMessage"] = "Avatar updated successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while uploading the file.";
                    System.Diagnostics.Debug.WriteLine("Error uploading avatar: " + ex.Message);
                }
            }

            return RedirectToAction("ProfileSetting");
        }
    }
}