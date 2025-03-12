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
using Sem3EProjectOnlineCPFH.Services;

namespace Sem3EProjectOnlineCPFH.Controllers
{
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
                IsActive = false
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

            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var userProfile = new UserProfile
                    {
                        Id = user.Id,
                        AvatarUrl = "~/Content/Resources/DefaultImg.png",
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

            //// Đăng nhập người dùng sau khi đăng ký
            //try
            //{
            //    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            //    System.Diagnostics.Debug.WriteLine("User signed in successfully.");
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine("SignInManager Error: " + ex.Message);
            //}

            //return RedirectToAction("Index", "Home");

            try
            {
                // **Tạo mã PIN 6 số**
                var random = new Random();
                var pinCode = random.Next(100000, 999999).ToString();

                using (var db = new ApplicationDbContext())
                {
                    var createdUser = db.Users.Find(user.Id);
                    if (createdUser != null)
                    {
                        createdUser.EmailConfirmationCode = pinCode;
                        createdUser.CodeExpirationTime = DateTime.UtcNow.AddMinutes(10); // Mã hết hạn sau 10 phút
                        db.SaveChanges();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Error: User not found in the database.");
                        TempData["ErrorMessage"] = "An error occurred while generating the verification code. Please try again.";
                        return RedirectToAction("Register");
                    }
                }

                // **Gửi mã PIN qua email**
                var emailService = new EmailService();
                await emailService.SendEmailAsync(user.Email, "Your Verification Code",
                    $"Your verification code is: <b>{pinCode}</b>. The code is valid for 10 minutes.");

                TempData["InfoMessage"] = "A verification code has been sent to your email. Please enter it below.";
                return RedirectToAction("VerifyEmail", new { userId = user.Id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in sending verification code: " + ex.Message);
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return RedirectToAction("Register");
            }

        }

        //GET: VerifyEmail
        [HttpGet]
        [AllowAnonymous]
        public ActionResult VerifyEmail(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login");

            return View(new VerifyEmailViewModel { UserId = userId });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input!";
                return View(model);
            }

            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(model.UserId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return View(model);
                }

                // Kiểm tra mã PIN có khớp và chưa hết hạn không
                if (user.EmailConfirmationCode != model.Code || user.CodeExpirationTime < DateTime.UtcNow)
                {
                    TempData["ErrorMessage"] = "Invalid or expired code.";
                    return View(model);
                }

                // Đánh dấu email đã xác thực
                user.EmailConfirmed = true;
                user.IsActive = true;
                user.EmailConfirmationCode = null; // Xóa mã PIN sau khi xác thực thành công
                user.CodeExpirationTime = null;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Email verified successfully! You can now log in.";
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ResendVerificationCode(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login");

            using (var db = new ApplicationDbContext())
            {
                var user = db.Users.Find(userId);
                if (user == null)
                    return RedirectToAction("Login");

                var random = new Random();
                var newPin = random.Next(100000, 999999).ToString();

                user.EmailConfirmationCode = newPin;
                user.CodeExpirationTime = DateTime.UtcNow.AddMinutes(10);
                db.SaveChanges();

                var emailService = new EmailService();
                await emailService.SendEmailAsync(user.Email, "Your New Verification Code",
                    $"Your new verification code is: <b>{newPin}</b>. The code is valid for 10 minutes.");

                TempData["SuccessMessage"] = "A new verification code has been sent.";
                return RedirectToAction("VerifyEmail", new { userId = user.Id });
            }
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
                user.Email = model.ProfileUpdate.Email;
                user.PhoneNumber = model.ProfileUpdate.PhoneNumber;
                user.Address = model.ProfileUpdate.Address;
                user.City = model.ProfileUpdate.City;
                user.Country = model.ProfileUpdate.Country;
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