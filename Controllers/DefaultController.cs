using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    public class DefaultController : BaseController
    {
        // GET: Default
        public ActionResult Unauthorized()
        {
            return View();
        }
    }
}