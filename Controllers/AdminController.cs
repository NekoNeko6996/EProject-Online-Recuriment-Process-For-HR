using Sem3EProjectOnlineCPFH.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sem3EProjectOnlineCPFH.Models.Auth;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using Sem3EProjectOnlineCPFH.Libraries;
using Microsoft.AspNet.Identity.EntityFramework;
using Sem3EProjectOnlineCPFH.Services;
using System.Collections.Generic;
using Sem3EProjectOnlineCPFH.Models.Log;
using System.Data.Entity;
using System.Drawing.Printing;
using System.Web.UI;
using Sem3EProjectOnlineCPFH.Models.Data;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : BaseController
    {
        public AdminController() { }
        private readonly ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        public AdminController(ApplicationUserManager userManager)
        {
            _userManager = userManager;
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

        public ActionResult Index(int page = 1)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            int PageSize = 7;

            string currentUserId = User.Identity.GetUserId();

            int totalUsers = db.Users.Where(u => u.Id != currentUserId).Count();
            int MaxPage = (int)Math.Ceiling((double)totalUsers / PageSize);

            if (MaxPage < 1) MaxPage = 1;
            if (page < 1) page = 1;
            if (page > MaxPage) page = MaxPage;

            var roleDict = db.Set<IdentityUserRole>()
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .ToDictionary(x => x.UserId, x => x.Name);

            var users = db.Users
                .Where(u => u.Id != currentUserId)
                .OrderBy(u => u.CreatedAt)
                .Skip(PageSize * (page - 1))
                .Take(PageSize)
                .ToList()
                .Select(u => new Sem3EProjectOnlineCPFH.Models.user.ManageUserAccountViewModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    isActive = u.IsActive,
                    RoleName = roleDict.ContainsKey(u.Id) ? roleDict[u.Id] : "No Role",
                    CreatedAt = u.CreatedAt
                }).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.MaxPage = MaxPage;
            ViewBag.Users = users;

            return View(users);
        }

        // GET: Add new user
        public ActionResult AddUser()
        {
            return View();
        }

        // POST: Add new user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser(NewUserRegistrationModelView model)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SystemLog.WriteLog(User.Identity.Name, "Add User", "Admin", Request.UserHostAddress, "True", $"Add new user [{model.Email}]");

            if (UserManager.FindByEmail(model.Email) != null)
            {
                TempData["ErrorMessage"] = "Email already exists!";
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
                IsActive = true,
            };

            var result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                SystemLog.WriteLog(User.Identity.Name, "Add User", "Admin", Request.UserHostAddress, "False", $"Error during create new user [{model.Email}]");
                foreach (var error in result.Errors)
                {
                    System.Diagnostics.Debug.WriteLine("Create User Error: " + error);
                }
                TempData["ErrorMessage"] = "Error during create new user";
                return View(model);
            }

            try
            {
                // Tạo profile user
                var userProfile = new UserProfile
                {
                    Id = user.Id,
                    AvatarUrl = ViewBag.DefaultAvatar,
                };

                db.UserProfiles.Add(userProfile);
                db.SaveChanges();

                try
                {
                    await UserManager.AddToRoleAsync(user.Id, model.Role);
                }
                catch (Exception ex)
                {
                    SystemLog.WriteLog(User.Identity.Name, "Add Role", "Admin", Request.UserHostAddress, "False", $"Add role [{model.Role}] to user [{model.Email}]");
                    System.Diagnostics.Debug.WriteLine("Add Role Error: " + ex.Message);
                    TempData["ErrorMessage"] = "Role Not Found";
                    return View(model);
                }

                SystemLog.WriteLog(User.Identity.Name, "Add New Account", "Admin", Request.UserHostAddress, "True", $"Add New Account {model.Email} success");
                TempData["SuccessMessage"] = "New Account created successfully!";
                return RedirectToAction(ViewBag.Page, ViewBag.Controller);
            }
            catch (Exception ex)
            {
                SystemLog.WriteLog(User.Identity.Name, "Add User", "Admin", Request.UserHostAddress, "False", $"Error during create new user [{model.Email}]");
                System.Diagnostics.Debug.WriteLine("DB Error: " + ex.Message);
                TempData["ErrorMessage"] = "DB Error";
                return View(model);
            }
        }

        // GET: View user details
        public ActionResult UserInfo(string id)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Edit user


        // POST: Delete user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(string Id)
        {
            if(Id == User.Identity.GetUserId())
            {
                TempData["ErrorMessage"] = "You cannot delete yourself!";
                return RedirectToAction(ViewBag.Page, ViewBag.Controller);
            }

            ApplicationDbContext db = new ApplicationDbContext();
            var user = db.Users.Find(Id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found!";
                return RedirectToAction(ViewBag.Page, ViewBag.Controller);
            }
            
            try
            {
                var userProfile = db.UserProfiles.Find(Id);
                if (userProfile != null)
                {
                    db.UserProfiles.Remove(db.UserProfiles.Find(Id));
                }
                UserManager.RemoveFromRoles(Id, UserManager.GetRoles(Id).ToArray());

                db.Users.Remove(user);
                db.SaveChanges();
                TempData["SuccessMessage"] = $"Deleted User [{Id}] successfully!";
                SystemLog.WriteLog(User.Identity.Name, "Delete User", "Admin", Request.UserHostAddress, "True", $"Delete User [{Id}] successfully!");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                SystemLog.WriteLog(User.Identity.Name, "Delete User", "Admin", Request.UserHostAddress, "False", $"Delete User Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("Delete User Error: " + ex.Message);
                TempData["ErrorMessage"] = "Delete User Error";
                return RedirectToAction(ViewBag.Page, ViewBag.Controller);
            }
        }

        // POST: Disable User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DisableUser(string Id)
        {
            if (Id == User.Identity.GetUserId())
            {
                TempData["ErrorMessage"] = "You cannot disable yourself!";
                return RedirectToAction(ViewBag.Page, ViewBag.Controller);
            }
            ApplicationDbContext db = new ApplicationDbContext();
            var user = db.Users.Find(Id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found!";
                return RedirectToAction(ViewBag.Page, ViewBag.Controller);
            }
            try
            {
                user.IsActive = !user.IsActive;
                db.SaveChanges();
                TempData["SuccessMessage"] = String.Concat(user.IsActive ? "Enable" : "Disable", $" User [{Id}] successfully!");
                SystemLog.WriteLog(User.Identity.Name, "Disable User", "Admin", Request.UserHostAddress, "True", String.Concat(user.IsActive ? "Enable" : "Disable", $" User [{Id}] successfully!"));
                return RedirectToAction(ViewBag.Page, ViewBag.Controller);
            }
            catch (Exception ex)
            {
                SystemLog.WriteLog(User.Identity.Name, "Disable User", "Admin", Request.UserHostAddress, "False", String.Concat(user.IsActive ? "Enable" : "Disable", $" User Error: {ex.Message}"));
                System.Diagnostics.Debug.WriteLine("Disable User Error: " + ex.Message);
                TempData["ErrorMessage"] = String.Concat(user.IsActive ? "Enable" : "Disable", "Disable User Error");
                return RedirectToAction(ViewBag.Page, ViewBag.Controller);
            }
        }

        // GET: Log Review
        public ActionResult LogReview()
        {
            List<LogEntry> logData = SystemLog.ReadLog();

            return View(logData.OrderByDescending(log => log.Timestamp).ToList());
        }

        // POST: Search USER
        [HttpPost]
        public ActionResult SearchUser(string role, string searchString, string status)
        {
            ApplicationDbContext db = new ApplicationDbContext();

            // Nhận giá trị từ form
            role = Request.Form["role"];
            status = Request.Form["status"];
            searchString = Request.Form["searchString"];

            // Include bảng Roles để tránh lỗi
            var users = db.Users.Include(u => u.Roles).AsQueryable();

            // Debug dữ liệu đầu vào
            System.Diagnostics.Debug.WriteLine($"Received searchString: {searchString}");
            System.Diagnostics.Debug.WriteLine($"Received role: {role}");
            System.Diagnostics.Debug.WriteLine($"Received status: {status}");

            // Tìm RoleId đúng từ bảng AspNetRoles
            var roleEntity = db.Roles.FirstOrDefault(r => r.Name == role);
            if (roleEntity != null)
            {
                users = users.Where(u => u.Roles.Any(r => r.RoleId == roleEntity.Id));
            }

            // Lọc theo status
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                bool isActive = (status == "active");
                users = users.Where(u => u.IsActive == isActive);
            }

            // Lọc theo searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Email.Contains(searchString) ||
                                         u.FirstName.Contains(searchString) ||
                                         u.LastName.Contains(searchString) ||
                                         u.Id.Contains(searchString));
            }

            // bỏ qua user hiện tại
            string currentUserId = User.Identity.GetUserId();
            users = users.Where(u => u.Id != currentUserId);

            // Debug truy vấn
            System.Diagnostics.Debug.WriteLine(users.ToString());

            // Truy vấn Role của từng User trước khi đưa vào ViewModel
            var userList = users.Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                u.IsActive,
                RoleName = db.Roles
                    .Where(r => r.Id == u.Roles.FirstOrDefault().RoleId)
                    .Select(r => r.Name)
                    .FirstOrDefault(),
                u.CreatedAt
            }).AsEnumerable()  // Chuyển sang LINQ to Objects
            .Select(u => new Sem3EProjectOnlineCPFH.Models.user.ManageUserAccountViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                isActive = u.IsActive,
                RoleName = u.RoleName ?? "No Role",
                CreatedAt = u.CreatedAt
            }).ToList();

            ViewBag.Users = userList;
            ViewBag.AdminPageSearchModelView = new AdminPageSearchModelView
            {
                SearchString = searchString,
                Role = role,
                Status = status
            };
            return View("Index", userList);
        }
    }
}