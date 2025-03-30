using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Sem3EProjectOnlineCPFH.Models;
using System.Data.Entity;
using System.Linq;
using Sem3EProjectOnlineCPFH.Libraries;
using Microsoft.AspNet.Identity.EntityFramework;

public class BaseController : Controller
{
    public static readonly string AvatarSavePath = "/Content/Resources/Avatars/";

    public void SetDefaultPage(string roleName)
    {
        switch (roleName)
        {
            case "admin":
                ViewBag.Controller = "Admin";
                ViewBag.Page = "Index";
                break;
            case "hrgroup":
                ViewBag.Controller = "Vacancy";
                ViewBag.Page = "Index";
                break;
            case "interviewer":
                ViewBag.Controller = "Interview";
                ViewBag.Page = "Index";
                break;
            default:
                ViewBag.Controller = "Home";
                ViewBag.Page = "Index";
                break;
        }
    }

    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        base.OnActionExecuting(filterContext);

        using (var db = new ApplicationDbContext())
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();

                var user = db.Users.Include(u => u.UserProfile)
                                   .FirstOrDefault(u => u.Id == userId);

                var roleName = (from userRole in db.Set<IdentityUserRole>()
                                join role in db.Roles on userRole.RoleId equals role.Id
                                where userRole.UserId == userId
                                select role.Name).FirstOrDefault();

                ViewBag.CurrentUser = user;
                ViewBag.CurrentUserRole = roleName;
                ViewBag.UserId = userId;

                SetDefaultPage(roleName);
            }
            else
            {
                ViewBag.CurrentUser = null;
                ViewBag.CurrentUserRole = null;
                SetDefaultPage(null);
            }

            ViewBag.AnyUserInDb = db.Users.Any();
        }
        ViewBag.WebsiteName = "Hyprics";
        ViewBag.WebIcon = "~/Content/Resources/WebLogo.png";
        ViewBag.DefaultAvatar = "~/Content/Resources/DefaultImg.png";
    }
}
