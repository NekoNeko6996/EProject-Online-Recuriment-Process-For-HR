using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using Sem3EProjectOnlineCPFH.Models;
using Sem3EProjectOnlineCPFH.Models.Data;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    [Authorize(Roles = "interviewer")]
    public class VacancyInterviewController : BaseController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public ActionResult Index(string searchTitle, string searchDepartment, string statusFilter,
                                string sortOrder, int? page)
        {
            int pageSize = 12;
            int pageNumber = (page ?? 1);

            var vacancies = _context.Vacancies
                .Include(v => v.Department)
                .Include(v => v.ApplicantVacancies)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTitle))
                vacancies = vacancies.Where(v => v.Title.Contains(searchTitle));

            if (!string.IsNullOrEmpty(searchDepartment))
                vacancies = vacancies.Where(v => v.Department != null &&
                                               v.Department.DepartmentName.Contains(searchDepartment));

            if (!string.IsNullOrEmpty(statusFilter))
            {
                vacancies = vacancies.Where(v => v.Status == statusFilter);
            }

            vacancies = ApplySorting(vacancies, sortOrder);

            ViewBag.TotalCount = _context.Vacancies.Count();
            ViewBag.SearchCount = vacancies.Count();
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";

            ViewBag.ApplicantByVacancy = _context.Applicant_Vacancies
                .Where(av => av.Vacancy != null)
                .GroupBy(av => av.Vacancy.VacancyId)
                .ToDictionary(g => g.Key, g => g.Count());

            return View(vacancies.ToPagedList(pageNumber, pageSize));
        }

        private IQueryable<Vacancy> ApplySorting(IQueryable<Vacancy> query, string sortOrder)
        {
            switch (sortOrder)
            {
                case "date_desc":
                    return query.OrderByDescending(v => v.Deadline);
                case "Date":
                    return query.OrderBy(v => v.Deadline);
                default:
                    return query.OrderByDescending(v => v.Deadline);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}