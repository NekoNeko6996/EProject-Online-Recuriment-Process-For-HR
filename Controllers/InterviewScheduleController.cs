using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Sem3EProjectOnlineCPFH.Models;
using Sem3EProjectOnlineCPFH.Models.ViewModels;
using Sem3EProjectOnlineCPFH.Models.Data;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    public class InterviewScheduleController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Interview
        public ActionResult Index()
        {
            var interviews = db.Interviews.Include(i => i.Applicant).Include(i => i.Vacancy).Include(i => i.Interviewer).ToList();
            return View(interviews);
        }

        // GET: Interview/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var interview = db.Interviews.Find(id);
            if (interview == null)
            {
                return HttpNotFound();
            }

            return View(interview);
        }

        // GET: Interview/Create
        public ActionResult Create(string vacancyId)
        {
            // ✅ Load danh sách Vacancy
            ViewBag.Vacancies = new SelectList(db.Vacancies, "VacancyId", "Title", vacancyId);

            // ✅ Xử lý danh sách Applicants
            if (!string.IsNullOrEmpty(vacancyId))
            {
                var vacancy = db.Vacancies
                                .Include(v => v.ApplicantVacancies.Select(av => av.Applicant)) // Load thông qua bảng trung gian
                                .FirstOrDefault(v => v.VacancyId == vacancyId);

                ViewBag.Applicants = (vacancy != null && vacancy.ApplicantVacancies.Any())
                    ? new SelectList(vacancy.ApplicantVacancies.Select(av => av.Applicant), "ApplicantId", "FullName") // Fix "Name" thành "FullName"
                    : new SelectList(Enumerable.Empty<SelectListItem>());
            }
            else
            {
                ViewBag.Applicants = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            List<Interviewer> interviewer = db.Users.Where(u => u.Roles.Any(r => r.RoleId == "3"))
                .Select(u => new Interviewer
                {
                    InterviewerId = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Phone = u.PhoneNumber
                }).ToList();

            ViewBag.Interviewers = interviewer;
            return View();
        }

        private string GenerateInterViewId()
        {
            var lastInterView = db.Interviews
                .OrderByDescending(d => d.InterviewerId)
                .FirstOrDefault();

            int nextId = 1;
            if (lastInterView != null)
            {
                if (int.TryParse(lastInterView.InterviewId.Substring(1), out int lastId))
                {
                    nextId = lastId + 1;
                }
            }

            if (nextId > 5000) return null; // Giới hạn ID đến D1000

            return "I" + nextId.ToString("0000");
        }

        // POST: Interview/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Interview interview)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid data. Please check your input!";
                return View(interview);
            }

            try
            {
                interview.InterviewId = GenerateInterViewId();
                db.Interviews.Add(interview);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return View(interview);
            }

            TempData["SuccessMessage"] = "Interview created successfully!"; 
            return RedirectToAction("Details", "VacancySchedule", new { id = interview.VacancyId });
        }

        // GET: Interview/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var interview = db.Interviews.Find(id);
            if (interview == null)
            {
                return HttpNotFound();
            }

            ViewBag.Applicants = new SelectList(db.Applicants, "ApplicantId", "Name", interview.ApplicantId);
            ViewBag.Vacancies = new SelectList(db.Vacancies, "VacancyId", "Title", interview.VacancyId);

            //var interviewers = (from user in db.Users
            //                    from role in user.Roles
            //                    join identityRole in db.Roles on role.RoleId equals identityRole.Id
            //                    where identityRole.Name == "Interviewer"
            //                    select new Interviewer
            //                    {
            //                        InterviewerId = user.Id,
            //                        Name = user.UserName,
            //                        Email = user.Email
            //                    }).ToList();

            //ViewBag.Interviewers = new SelectList(interviewers, "InterviewerId", "Name", interview.InterviewerId);

            return View(interview);
        }

        // POST: Interview/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Interview interview)
        {
            if (!ModelState.IsValid)
            {
                return View(interview);
            }

            var existingInterview = db.Interviews.Find(interview.InterviewId);
            if (existingInterview == null)
            {
                TempData["ErrorMessage"] = "Interview not found!";
                return RedirectToAction("Index");
            }

            db.Entry(existingInterview).CurrentValues.SetValues(interview);

            try
            {
                db.SaveChanges();
                TempData["SuccessMessage"] = "Interview updated successfully!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Data was modified by another user. Please try again!";
                return View(interview);
            }
        }

        // GET: Interview/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var interview = db.Interviews.Find(id);
            if (interview == null)
            {
                return HttpNotFound();
            }

            return View(interview);
        }

        // POST: Interview/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var interview = db.Interviews.Find(id);
            if (interview == null)
            {
                TempData["ErrorMessage"] = "Interview not found!";
                return RedirectToAction("Index");
            }

            db.Interviews.Remove(interview);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Interview deleted successfully!";
            return RedirectToAction("Index");
        }

        // GET: API to fetch Applicants by VacancyId for AJAX
        public JsonResult GetApplicantsByVacancy(string vacancyId)
        {
            if (string.IsNullOrEmpty(vacancyId))
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            var applicants = db.Applicant_Vacancies
                .Where(av => av.VacancyId == vacancyId) // Lọc theo vacancyId
                .Select(av => new
                {
                    Id = av.Applicant.ApplicantId,
                    Name = av.Applicant.FullName // Sửa lại tên property theo model
                })
                .ToList();

            return Json(applicants, JsonRequestBehavior.AllowGet);
        }
    }
}
    