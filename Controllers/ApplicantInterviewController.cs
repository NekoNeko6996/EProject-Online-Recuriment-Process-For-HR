using Sem3EProjectOnlineCPFH.Models.Data;
using Sem3EProjectOnlineCPFH.Models;
using Sem3EProjectOnlineCPFH.Models.Enum;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using System;
using Microsoft.AspNet.Identity;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    [Authorize(Roles = "interviewer")]
    public class ApplicantInterviewController : BaseController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public ActionResult Index(string searchName, string status, string vacancyId, int? page)
        {
            int pageSize = 12;
            int pageNumber = (page ?? 1);

           
            if (!string.IsNullOrEmpty(vacancyId))
            {
                ViewBag.Vacancy = _context.Vacancies
                    .Include(v => v.Department)
                    .FirstOrDefault(v => v.VacancyId == vacancyId);
            }

            string userId = User.Identity.GetUserId();  

            // Get all applicants me interview
            var applicants = _context.Interviews
                .Where(i => i.InterviewerId == userId && i.Applicant.Status == ApplicantStatus.Scheduled && i.Status == InterviewStatus.Scheduled)
                .Select(i => i.Applicant)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                applicants = applicants.Where(a => a.FullName.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(status))
            {
                ApplicantStatus statusEnum;
                if (Enum.TryParse(status, out statusEnum))
                {
                    applicants = applicants.Where(a => a.Status == statusEnum);
                }

            }

            if (!string.IsNullOrEmpty(vacancyId))
            {
                applicants = applicants.Where(a => a.ApplicantVacancies
                    .Any(av => av.VacancyId == vacancyId));
            }

        
            ViewBag.TotalCandidates = _context.Applicants.Count();
            ViewBag.NotApproved = _context.Applicants.Count(a => a.Status == ApplicantStatus.NotApproved);
            ViewBag.Added = _context.Applicants.Count(a => a.Status == ApplicantStatus.Added);
            ViewBag.Scheduled = _context.Applicants.Count(a => a.Status == ApplicantStatus.Scheduled);
            ViewBag.HasPassed = _context.Applicants.Count(a => a.Status == ApplicantStatus.HasPassed);
            ViewBag.Rejected = _context.Applicants.Count(a => a.Status == ApplicantStatus.Rejected);
            ViewBag.CurrentVacancyId = vacancyId;

            return View(applicants.OrderBy(a => a.FullName).ToPagedList(pageNumber, pageSize));
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