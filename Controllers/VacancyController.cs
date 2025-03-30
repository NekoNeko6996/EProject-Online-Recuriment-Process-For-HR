using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Sem3EProjectOnlineCPFH.Models;
using Sem3EProjectOnlineCPFH.Models.Data;
using System.Data.Entity;
using PagedList;

namespace RecruitmentProces.Controllers
{
    [Authorize(Roles = "hrgroup")]
    public class VacancyController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Phương thức sinh VacancyId tự động (V0001 - V5000)
        private string GenerateVacancyId()
        {
            const string prefix = "V";
            string lastId = db.Vacancies
                              .OrderByDescending(v => v.VacancyId)
                              .Select(v => v.VacancyId)
                              .FirstOrDefault();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastId) && int.TryParse(lastId.Substring(1), out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return nextNumber > 5000 ? null : $"{prefix}{nextNumber:D4}";
        }

        // API lấy danh sách phòng ban để gợi ý
        public JsonResult SearchDepartment(string term)
        {
            var departments = db.Departments
                                .Where(d => d.DepartmentName.Contains(term))
                                .Select(d => new { id = d.DepartmentId, name = d.DepartmentName })
                                .ToList();
            return Json(departments, JsonRequestBehavior.AllowGet);
        }

        // GET: Vacancy
        public ActionResult Index(string search, int? page)
        {
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            var vacancies = db.Vacancies.Include(v => v.Department).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                vacancies = vacancies.Where(v => v.Title.Contains(search) || v.Department.DepartmentName.Contains(search));
            }

            ViewBag.Search = search;
            return View(vacancies.OrderBy(v => v.Title).ToPagedList(pageNumber, pageSize));
        }

        // GET: Vacancy/Create
        public ActionResult Create()
        {
            string newId = GenerateVacancyId();
            if (newId == null)
            {
                TempData["Error"] = "Cannot create more than 5000 vacancies.";
                return RedirectToAction("Index");
            }

            var vacancy = new Vacancy
            {
                VacancyId = newId,
                CreatedAt = DateTime.Now // Đảm bảo CreatedDate không bị null
            };

            ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "DepartmentName");
            return View(vacancy);
        }

        // POST: Vacancy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Vacancy vacancy)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(vacancy.VacancyId))
                    {
                        vacancy.VacancyId = GenerateVacancyId();
                    }

                    if (vacancy.VacancyId == null)
                    {
                        ModelState.AddModelError("", "Cannot create more than 5000 vacancies.");
                        ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "DepartmentName");
                        return View(vacancy);
                    }

                    vacancy.CreatedAt = DateTime.Now;
                    vacancy.Deadline = vacancy.CreatedAt.Date.AddDays(30);

                    db.Vacancies.Add(vacancy);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    var innerException = ex.InnerException?.InnerException;
                    Console.WriteLine(innerException?.Message);
                    throw;
                }
            }

            ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "DepartmentName", vacancy.DepartmentId);
            return View(vacancy);
        }

        // GET: Vacancy/Edit/5
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Vacancy vacancy = db.Vacancies.Find(id);
            if (vacancy == null)
                return HttpNotFound();

            ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "DepartmentName", vacancy.DepartmentId);
            return View(vacancy);
        }

        // POST: Vacancy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Vacancy vacancy)
        {
            if (ModelState.IsValid)
            {
                vacancy.Deadline = vacancy.CreatedAt.Date.AddDays(30);
                db.Entry(vacancy).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "DepartmentName", vacancy.DepartmentId);
            return View(vacancy);
        }
        // DELETE Vacancy without confirmation page
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid vacancy ID.";
                return RedirectToAction("Index");
            }

            Vacancy vacancy = db.Vacancies.Find(id);
            if (vacancy == null)
            {
                TempData["Error"] = "Vacancy not found.";
                return RedirectToAction("Index");
            }

            db.Vacancies.Remove(vacancy);
            db.SaveChanges();
            TempData["Success"] = "Vacancy deleted successfully!";
            return RedirectToAction("Index");
        }


        // GET: Vacancy/Details/5
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Vacancy vacancy = db.Vacancies.Find(id);
            if (vacancy == null)
                return HttpNotFound();

            return View(vacancy);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}