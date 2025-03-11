using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Sem3EProjectOnlineCPFH.Models;
using System.Data.Entity;
using System.Linq;

public class BaseController : Controller
{
    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        base.OnActionExecuting(filterContext);

        if (User.Identity.IsAuthenticated)
        {
            using (var db = new ApplicationDbContext())
            {
                var userId = User.Identity.GetUserId();
                var user = db.Users.Include(u => u.UserProfile)
                                   .FirstOrDefault(u => u.Id == userId);

                ViewBag.CurrentUser = user;
            }
        }
    }
}
