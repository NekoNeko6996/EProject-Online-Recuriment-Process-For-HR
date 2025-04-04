using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Sem3EProjectOnlineCPFH.Models;
using Sem3EProjectOnlineCPFH.Models.Data;
using System.Data.Entity;
using PagedList;
using Microsoft.AspNet.Identity;
using Sem3EProjectOnlineCPFH.Models.Enum;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    [Authorize(Roles = "hrgroup")]
    public class VacancyController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

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
            return View(vacancies.OrderByDescending(v => v.CreatedAt).ToPagedList(pageNumber, pageSize));
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

            var departments = db.Departments.ToList();

            ViewBag.Departments = departments;
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
                        TempData["ErrorMessage"] = "Cannot create more than 5000 vacancies.";
                        ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "DepartmentName");
                        return View(vacancy);
                    }

                    // check Department is valid
                    var department = db.Departments.Find(vacancy.DepartmentId);

                    if (department == null)
                    {
                        ModelState.AddModelError("DepartmentId", "Department not found.");
                        TempData["ErrorMessage"] = "Department not found.";
                        ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "DepartmentName", vacancy.DepartmentId);
                        return View(vacancy);
                    }

                    vacancy.OwnerId = User.Identity.GetUserId();
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

        [HttpPost]
        public ActionResult DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid vacancy ID.";
                return RedirectToAction("Index");
            }

            var vacancy = db.Vacancies.Find(id);
            if (vacancy == null)
            {
                TempData["Error"] = "Vacancy not found.";
                return RedirectToAction("Index");
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Xóa tất cả Applicant_Vacancy liên quan
                    var applicantVacancies = db.Applicant_Vacancies.Where(av => av.VacancyId == id).ToList();
                    db.Applicant_Vacancies.RemoveRange(applicantVacancies);

                    // Xóa tất cả Interview liên quan
                    var interviews = db.Interviews.Where(i => i.VacancyId == id).ToList();
                    db.Interviews.RemoveRange(interviews);

                    // Xóa tất cả Applicant không còn liên kết với Vacancy nào khác
                    var applicantIds = applicantVacancies.Select(av => av.ApplicantId).Distinct().ToList();
                    foreach (var applicantId in applicantIds)
                    {
                        bool hasOtherVacancies = db.Applicant_Vacancies.Any(av => av.ApplicantId == applicantId);
                        if (!hasOtherVacancies)
                        {
                            var applicant = db.Applicants.Find(applicantId);
                            if (applicant != null)
                            {
                                db.Applicants.Remove(applicant);
                            }
                        }
                    }

                    // Xóa Vacancy
                    db.Vacancies.Remove(vacancy);
                    db.SaveChanges();

                    transaction.Commit();
                    TempData["SuccessMessage"] = "Vacancy deleted successfully!";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["ErrorMessage"] = "Error deleting vacancy.";
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    Console.WriteLine(ex.Message);
                }
            }

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

            ViewBag.Applicants = db.Applicant_Vacancies.Where(av => av.VacancyId == id).ToList();
            ViewBag.AllApplicants = db.Applicants.Where(a => a.Status == ApplicantStatus.NotApproved).ToList();

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

        public ActionResult RemoveApplicant(string ApplicantId, string VacancyId)
        {
            // check if the applicant is in the vacancy
            var applicantVacancy = db.Applicant_Vacancies.FirstOrDefault(av => av.ApplicantId == ApplicantId && av.VacancyId == VacancyId);
            if (applicantVacancy == null)
            {
                TempData["Error"] = "Applicant not found in this vacancy.";
                return RedirectToAction("Details", new { id = VacancyId });
            }

            try
            {
                db.Applicant_Vacancies.Remove(applicantVacancy);
                db.Applicants.Where(a => a.ApplicantId == ApplicantId).FirstOrDefault().Status = ApplicantStatus.NotApproved;

                db.SaveChanges();
                TempData["SuccessMessage"] = "Applicant removed from vacancy successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error removing applicant from vacancy.";
                System.Diagnostics.Debug.WriteLine(ex.Message); 
                Console.WriteLine(ex.Message);
            }

            return RedirectToAction("Details", new { id = VacancyId });
        }

        private string GenerateApplicantVacancyId()
        {
            var lastApplicantVacancy = db.Applicant_Vacancies
                .OrderByDescending(d => d.ApplicantVacancyId)
                .FirstOrDefault();

            int nextId = 1;
            if (lastApplicantVacancy != null)
            {
                if (int.TryParse(lastApplicantVacancy.ApplicantVacancyId.Substring(2), out int lastId))
                {
                    nextId = lastId + 1;
                }
            }

            if (nextId > 1000) return null; // Giới hạn ID đến D1000

            return "AV" + nextId.ToString("0000");
        }

        public ActionResult AddApplicantToVacancy(string VacancyId, string ApplicantId)
        {
            // check if the applicant is in the vacancy
            var applicantVacancy = db.Applicant_Vacancies.FirstOrDefault(av => av.ApplicantId == ApplicantId && av.VacancyId == VacancyId);
            if (applicantVacancy != null)
            {
                TempData["Error"] = "Applicant already in this vacancy.";
                return RedirectToAction("Details", new { id = VacancyId });
            }

            try
            {
                db.Applicant_Vacancies.Add(new Applicant_Vacancy
                {
                    ApplicantId = ApplicantId,
                    VacancyId = VacancyId,
                    ApplyAt = DateTime.Now,
                    ApplicantVacancyId = GenerateApplicantVacancyId(),
                    Approver = User.Identity.GetUserId()
                });

                db.Applicants.FirstOrDefault(a => a.ApplicantId == ApplicantId).Status = ApplicantStatus.Added;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Applicant added to vacancy successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error adding applicant to vacancy.";
                System.Diagnostics.Debug.WriteLine(ex.Message);
                Console.WriteLine(ex.Message);
            }

            return RedirectToAction("Details", new { id = VacancyId });
        }
    }
}