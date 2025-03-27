using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    public class DefaultController : Controller
    {
        // GET: Default
        public ActionResult Unauthorized()
        {
            return View();
        }

        public ActionResult DatabaseConnectError()
        {
            return View();
        }

        public ActionResult Help()
        {
            return View();
        }
    }
}