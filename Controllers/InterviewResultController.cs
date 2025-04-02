using Sem3EProjectOnlineCPFH.Models;
using Sem3EProjectOnlineCPFH.Models.Data;
using Sem3EProjectOnlineCPFH.Models.Enum;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using PagedList;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    [Authorize(Roles = "interviewer")]
    public class InterviewResultController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public InterviewResultController()
        {
            _context = new ApplicationDbContext();
        }

        public ActionResult Index(string resultType, string searchName, string searchDepartment, string searchPosition, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            IQueryable<Interview> baseQuery = _context.Interviews
                .Include(i => i.Applicant)
                .Include(i => i.Vacancy)
                .Include(i => i.Vacancy.Department)
                .Where(i => i.Status == InterviewStatus.Completed);

            if (!string.IsNullOrEmpty(resultType))
            {
                baseQuery = baseQuery.Where(i => i.Result == resultType);
            }

          
            if (!string.IsNullOrEmpty(searchName))
            {
                baseQuery = baseQuery.Where(i => i.Applicant.FullName.Contains(searchName));
            }

           
            if (!string.IsNullOrEmpty(searchDepartment))
            {
                baseQuery = baseQuery.Where(i => i.Vacancy.Department.DepartmentName.Contains(searchDepartment));
            }

           
            if (!string.IsNullOrEmpty(searchPosition))
            {
                baseQuery = baseQuery.Where(i => i.Vacancy.Title.Contains(searchPosition));
            }

            var results = baseQuery.OrderBy(i => i.Applicant.FullName)
                                .ToPagedList(pageNumber, pageSize);

            ViewBag.ResultType = resultType ?? "Selected";
            ViewBag.SearchName = searchName;
            ViewBag.SearchDepartment = searchDepartment;
            ViewBag.SearchPosition = searchPosition;

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View(results);
        }

        public ActionResult Selected(string searchName, string searchDepartment, string searchPosition, int? page)
        {
            return RedirectToAction("Index", new
            {
                resultType = "Selected",
                searchName,
                searchDepartment,
                searchPosition,
                page
            });
        }

        public ActionResult Rejected(string searchName, string searchDepartment, string searchPosition, int? page)
        {
            return RedirectToAction("Index", new
            {
                resultType = "Rejected",
                searchName,
                searchDepartment,
                searchPosition,
                page
            });
        }
    }
}