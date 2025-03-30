using System;
using System.Linq;
using System.Web.Mvc;
using Sem3EProjectOnlineCPFH.Models;
using Sem3EProjectOnlineCPFH.Models.ViewModels;
using System.Data.Entity;
using System.Collections.Generic;
using Sem3EProjectOnlineCPFH.Models.Data;

public class VacancyScheduleController : Controller
{
    private readonly ApplicationDbContext _context = new ApplicationDbContext();

    // Danh sách Vacancy
    public ActionResult Index()
    {
        var vacancies = _context.Vacancies
            .Where(v => v.Status == "Open" && v.ApplicantVacancies.Count() > 0)
            .ToList();

        return View(vacancies);
    }

    // Danh sách Applicants theo VacancyId
    public ActionResult Applicants(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToAction("Index", "Vacancy");
        }

        var vacancy = _context.Vacancies.Find(id);
        if (vacancy == null)
        {
            return HttpNotFound("Vacancy not found!");
        }

        var interviews = _context.Interviews
            .Where(i => i.VacancyId == id)
            .Select(i => new InterviewViewModel
            {
                Id = i.InterviewId,
                ApplicantId = i.ApplicantId,
                InterviewerId = i.InterviewerId,
                ScheduledDate = i.ScheduledDate,
                StartTime = i.StartTime,
                EndTime = i.EndTime,
                Status = i.Status
            }).ToList();

        ViewBag.VacancyTitle = vacancy.Title;
        return View(interviews);
    }

    public ActionResult Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return HttpNotFound("Vacancy ID is missing!");
        }

        var vacancy = _context.Vacancies
            .AsNoTracking()
            .Include(v => v.ApplicantVacancies.Select(av => av.Applicant)) // Load Applicants qua ApplicantVacancy
            .FirstOrDefault(v => v.VacancyId == id);

        if (vacancy == null)
        {
            return HttpNotFound("Vacancy not found!");
        }

        // Lấy danh sách Applicants từ ApplicantVacancies
        var applicants = vacancy.ApplicantVacancies
            ?.Select(av => av.Applicant)
            .ToList() ?? new List<Applicant>();

        var interviews = _context.Interviews
            .Where(i => i.VacancyId == id)
            .Select(i => new InterviewViewModel
            {
                Id = i.InterviewId,
                ApplicantId = i.ApplicantId,
                ApplicantName = i.Applicant.FullName, // Lấy tên ứng viên
                InterviewerId = i.InterviewerId,
                InterviewerName = String.Concat(i.Interviewer.LastName, " ", i.Interviewer.FirstName) , // Lấy tên Interviewer
                ScheduledDate = i.ScheduledDate,
                StartTime = i.StartTime,
                EndTime = i.EndTime,
                InterviewMethod = i.InterviewMethod,
                MeetUrl = i.InterviewURL,
                Status = i.Status
            }).ToList();

        var viewModel = new VacancyViewModel
        {
            Vacancy = vacancy,
            Applicants = applicants, // Gán danh sách ứng viên vào ViewModel
            Interviews = interviews // Chưa có bảng Interview nên để rỗng
        };


        return View(viewModel);
    }
}
