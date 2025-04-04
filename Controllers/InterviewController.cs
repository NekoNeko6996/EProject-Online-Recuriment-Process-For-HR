using Sem3EProjectOnlineCPFH.Models;
using Sem3EProjectOnlineCPFH.Models.Data;
using Sem3EProjectOnlineCPFH.Models.Enum;
using Sem3EProjectOnlineCPFH.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    [Authorize(Roles = "interviewer")]
    public class InterviewController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public InterviewController()
        {
            _context = new ApplicationDbContext();
        }

        public ActionResult Index(string applicantId)
        {
            if (string.IsNullOrEmpty(applicantId))
            {
                return HttpNotFound();
            }

            var interview = _context.Interviews
                .Include(i => i.Applicant)
                .Include(i => i.Vacancy)
                .FirstOrDefault(i => i.ApplicantId == applicantId);

            if (interview == null)
            {
                var applicant = _context.Applicants
                    .Include(a => a.ApplicantVacancies.Select(av => av.Vacancy))
                    .FirstOrDefault(a => a.ApplicantId == applicantId);

                if (applicant == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new Interview
                {
                    Applicant = applicant,
                    ApplicantId = applicantId,
                    Status = InterviewStatus.Scheduled
                };

                return View(viewModel);
            }

            interview.Applicant.ApplicantVacancies = _context.Applicant_Vacancies
                .Include(av => av.Vacancy)
                .Where(av => av.ApplicantId == applicantId)
                .ToList();

            return View(interview);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateResult(string interviewId, string result, string applicantId)
        {
            try
            {
                var interview = _context.Interviews
                    .Include(i => i.Applicant)
                    .FirstOrDefault(i => i.InterviewId == interviewId);

                if (interview != null)
                {
                    interview.Result = result;
                    interview.Status = InterviewStatus.Completed;

                    string emailSubject = "Interview Result Notification";
                    string emailBody = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; }}
                            .email-container {{ max-width: 600px; margin: auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0px 0px 10px #ccc; }}
                            .header {{ text-align: center; padding: 10px; background-color: #007bff; color: white; font-size: 20px; font-weight: bold; border-radius: 8px 8px 0 0; }}
                            .content {{ padding: 20px; line-height: 1.6; }}
                            .footer {{ text-align: center; font-size: 12px; color: gray; margin-top: 20px; }}
                            .button {{ display: inline-block; padding: 10px 15px; color: white; background-color: #28a745; text-decoration: none; border-radius: 5px; font-weight: bold; }}
                            .rejected {{ background-color: #dc3545; }}
                        </style>
                    </head>
                    <body>
                        <div class='email-container'>
                            <div class='header'>Interview Result Notification</div>
                            <div class='content'>
                                <p>Dear <b>{interview.Applicant.FullName}</b>,</p>
                                <p>We are pleased to inform you that your interview result has been updated.</p>";

                        if (result == "Selected")
                        {
                            interview.Applicant.Status = ApplicantStatus.HasPassed;
                            emailBody += @"
                        <p>🎉 <b>Congratulations!</b> You have been <span style='color: green; font-weight: bold;'>Selected</span> for the <strong>" + interview.Vacancy.Department.ToString() + @"</strong> position.</p>
                        <p>We will contact you soon with the next steps.</p>";
                        }
                        else
                        {
                            interview.Applicant.Status = ApplicantStatus.Rejected;
                            emailBody += @"
                        <p>❌ <b>Unfortunately, you have not been selected</b> for the <strong>"+ interview.Vacancy.Department.ToString() + @"</strong> position.</p>
                        <p>We truly appreciate your time and encourage you to apply again in the future.</p>";
                        }

                        emailBody += @"
                            </div>
                            <div class='footer'>
                                <p>Best Regards, <br>HR Team | Hyprics</p>
                                <p>&copy; 2024 Hyprics. All Rights Reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                    _context.SaveChanges();

                    // Gửi email với nội dung HTML
                    var emailService = new EmailService();
                    await emailService.SendEmailAsync(interview.Applicant.Email, emailSubject, emailBody);

                    // Check if enough quantity then close vacancy
                    // count selected applicants of this vacancy
                    var selectedApplicants = _context.Interviews
                        .Where(i => i.VacancyId == interview.VacancyId && i.Result == "Selected")
                        .Count();

                    var numberOfPositions = _context.Vacancies.Find(interview.VacancyId).NumberOfPositions;
                    System.Diagnostics.Debug.WriteLine("Selected: " + selectedApplicants);
                    System.Diagnostics.Debug.WriteLine("Number of Positions: " + numberOfPositions);

                    if (selectedApplicants >= numberOfPositions)
                    {
                        var vacancy = _context.Vacancies.Find(interview.VacancyId);
                        vacancy.Status = "Close";

                        // Send email to notify vacancy closed
                        string emailCloseVSubject = "Vacancy Closed Notification - " + interview.Vacancy.Title;
                        string emailCloseVBody = $@"
                            <html>
                            <head>
                                <style>
                                    body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; }}
                                    .email-container {{ max-width: 600px; margin: auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0px 0px 10px #ccc; }}
                                    .header {{ text-align: center; padding: 10px; background-color: #007bff; color: white; font-size: 20px; font-weight: bold; border-radius: 8px 8px 0 0; }}
                                    .content {{ padding: 20px; line-height: 1.6; }}
                                    .footer {{ text-align: center; font-size: 12px; color: gray; margin-top: 20px; }}
                                </style>
                            </head>
                            <body>
                                <div class='email-container'>
                                    <div class='header'>Vacancy Closed Notification</div>
                                    <div class='content'>
                                        <p>Dear HR Team,</p>
                                        <p>The vacancy for the <b>{interview.Vacancy.Title}</b> position in the <b>{interview.Vacancy.Department.DepartmentName}</b> department has been automatically closed.</p>
                                        <p>This vacancy has reached the required number of selected applicants (<b>{interview.Vacancy.NumberOfPositions}</b>).</p>
                                        <p>Please review the selected candidates and proceed with the next steps.</p>
                                    </div>
                                    <div class='footer'>
                                        <p>Best Regards, <br>Recruitment System | Hyprics</p>
                                        <p>&copy; 2024 Hyprics. All Rights Reserved.</p>
                                    </div>
                                </div>
                            </body>
                            </html>";


                        // get HR Group email
                        var hrGroupEmail = _context.Users
                            .FirstOrDefault(u => u.Roles.Any(r => r.RoleId == "2" && u.Id.Equals(interview.Vacancy.OwnerId))).Email;

                        System.Diagnostics.Debug.WriteLine("HR Group Email: " + hrGroupEmail);

                        // Gửi email thông báo đến HR Group
                        await emailService.SendEmailAsync(hrGroupEmail, emailCloseVSubject, emailCloseVBody);
                    }

                    _context.SaveChanges();
                    TempData["Message"] = $"Results updated: {result}";
                }
                return RedirectToAction("Index", "InterviewResult", new { resultType = result });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the results: " + ex.Message;
                return RedirectToAction("Index", new { applicantId = applicantId });
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