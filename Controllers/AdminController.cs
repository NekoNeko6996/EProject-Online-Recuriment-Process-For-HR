using Sem3EProjectOnlineCPFH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var users = db.Users.Select(u => new Sem3EProjectOnlineCPFH.Models.user.ManageUserAccountViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                isActive = u.IsActive,
                RoleName = db.Roles
                    .Where(r => u.Roles.Select(ur => ur.RoleId).Contains(r.Id))
                    .Select(r => r.Name)
                    .FirstOrDefault(),
                CreatedAt = u.CreatedAt
            }).ToList();

            ViewBag.Users = users;
            return View(users);
        }

        // GET: Add new user
        public ActionResult AddUser()
        {
            return View();
        }
    }
}