using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Sem3EProjectOnlineCPFH.Models;
using Sem3EProjectOnlineCPFH.Models.ViewModels;
using Sem3EProjectOnlineCPFH.Models.Enum;
using Sem3EProjectOnlineCPFH.Models.Data;
using Sem3EProjectOnlineCPFH.Services;
using System.Threading.Tasks;

namespace Sem3EProjectOnlineCPFH.Controllers
{
    [Authorize(Roles = "hrgroup")]
    public class InterviewScheduleController : BaseController
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
        public async Task<ActionResult> Create(Interview interview)
        {
            ViewBag.VID = interview.VacancyId;
            ViewBag.AID = interview.ApplicantId;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid data. Please check your input!";
                return View(interview);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Kiểm tra applicant có tồn tại không
                    var applicant = db.Applicants.Find(interview.ApplicantId);
                    if (applicant == null)
                    {
                        TempData["ErrorMessage"] = "Applicant not found!";
                        return View(interview);
                    }

                    // Kiểm tra xem applicant đã có lịch phỏng vấn trong Vacancy này chưa
                    var existingInterview = db.Interviews.FirstOrDefault(i => i.ApplicantId == interview.ApplicantId && i.VacancyId == interview.VacancyId);
                    if (existingInterview != null)
                    {
                        TempData["ErrorMessage"] = "Applicant already has an interview for this vacancy.";
                        return View(interview);
                    }

                    // Gán ID phỏng vấn và cập nhật trạng thái applicant
                    interview.InterviewId = GenerateInterViewId();
                    applicant.Status = ApplicantStatus.Scheduled;

                    db.Interviews.Add(interview);
                    db.SaveChanges();

                    //
                    var email = new EmailService();
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
                        </style>
                    </head>
                    <body>
                        <div class='email-container'>
                            <div class='header'>Interview Schedule</div>
                            <div class='content'>
                                <p>Dear <b>{applicant.FullName}</b>,</p>
                                <p>You have an interview scheduled on <b>{interview.ScheduledDate:dddd, MMMM d, yyyy}</b> 
                                from <b>{interview.StartTime} - {interview.EndTime}</b>.</p>

                                <p><b>Interview Method:</b> {interview.InterviewMethod}</p>

                                {(interview.InterviewMethod == "Online"
                                    ? $"<p><b>Interview URL:</b> <a href='{interview.InterviewURL}' style='color: #0D6EFD;'>Join Meeting</a></p>"
                                    : "<p><b>Address:</b> 1st Ly Tu Trong, Ninh Kieu, Cantho City</p>")}

                                <p style='color: green;'><b>Note:</b> Please be prepared and join on time.</p>
                            </div>
                            <div class='footer'>
                                <p>Best Regards, <br>HR Team | Your Company</p>
                                <p>&copy; 2024 Your Company. All Rights Reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>";


                    await email.SendEmailAsync(applicant.Email, "Interview Schedule", emailBody);


                    transaction.Commit();
                    TempData["SuccessMessage"] = "Interview created successfully!";
                    return RedirectToAction("Details", "VacancySchedule", new { id = interview.VacancyId });
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error: " + e.Message);
                    if (e.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Inner Exception: " + e.InnerException.Message);
                    }

                    TempData["ErrorMessage"] = "An error occurred while updating the interview. Please check logs for details.";
                    return View(interview);
                }
            }
        }


        // GET: Interview/Edit/5
        public ActionResult Edit(string id)
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

            var interviewers = (from user in db.Users
                                from role in user.Roles
                                join identityRole in db.Roles on role.RoleId equals identityRole.Id
                                where identityRole.Name == "Interviewer"
                                select new Interviewer
                                {
                                    InterviewerId = user.Id,
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    Email = user.Email
                                }).ToList();

            ViewBag.Interviewers = new SelectList(interviewers, "InterviewerId", "Name", interview.InterviewerId);

            return View(interview);
        }

        // POST: Interview/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Interview interview)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(interview);
            }

            var existingInterview = db.Interviews.Find(interview.InterviewId);
            if (existingInterview == null)
            {
                TempData["ErrorMessage"] = "Interview not found!";
                return RedirectToAction("Index");
            }

            try
            {
                // Cập nhật thông tin từ form vào bản ghi trong database
                existingInterview.ApplicantId = interview.ApplicantId;
                existingInterview.VacancyId = interview.VacancyId;
                existingInterview.InterviewerId = interview.InterviewerId;
                existingInterview.ScheduledDate = interview.ScheduledDate;
                existingInterview.StartTime = interview.StartTime;
                existingInterview.EndTime = interview.EndTime;
                existingInterview.Status = interview.Status;
                existingInterview.InterviewMethod = interview.InterviewMethod;

                // Nếu phương thức là Online, cập nhật URL
                if (interview.InterviewMethod == "Online")
                {
                    existingInterview.InterviewURL = interview.InterviewURL;
                }
                else
                {
                    // Xóa URL nếu chuyển sang Offline để tránh dữ liệu không cần thiết
                    existingInterview.InterviewURL = null;
                }

                db.Entry(existingInterview).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Interview updated successfully!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Data was modified by another user. Please try again!";
                return View(interview);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                TempData["ErrorMessage"] = "An error occurred while updating the interview.";
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Postpone(string InterviewId, string VacancyId)
        {
            var interview = db.Interviews.Include(i => i.Applicant).FirstOrDefault(i => i.InterviewId == InterviewId);

            if (interview != null && (interview.Status == InterviewStatus.Scheduled || interview.Status == InterviewStatus.Postponed))
            {
                bool isPostponed = interview.Status == InterviewStatus.Scheduled;
                interview.Status = isPostponed ? InterviewStatus.Postponed : InterviewStatus.Scheduled;

                db.Entry(interview).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Interview status updated successfully!";

                // Gửi email thông báo cho ứng viên
                string emailSubject = isPostponed ? "Interview Postponed Notification" : "Interview Rescheduled Notification";
                string emailBody = $@"
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
                            <div class='header'>{emailSubject}</div>
                            <div class='content'>
                                <p>Dear <b>{interview.Applicant.FullName}</b>,</p>
                                <p>We would like to inform you that your interview for the <b>{interview.Vacancy.Title}</b> position has been {(isPostponed ? "postponed" : "rescheduled")}.</p>
                                <p>Please stay tuned for further updates.</p>
                            </div>
                            <div class='footer'>
                                <p>Best Regards, <br>HR Team | Hyprics</p>
                                <p>&copy; 2024 Hyprics. All Rights Reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                var emailService = new EmailService();
                await emailService.SendEmailAsync(interview.Applicant.Email, emailSubject, emailBody);
            }
            else
            {
                TempData["ErrorMessage"] = "Interview cannot be postponed or rescheduled!";
            }

            return RedirectToAction("InterviewDetails", "VacancySchedule", new { id = InterviewId, VID = VacancyId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Cancel(string InterviewId, string VacancyId)
        {
            var interview = db.Interviews.Include(i => i.Applicant).FirstOrDefault(i => i.InterviewId == InterviewId);

            if (interview != null && (interview.Status == InterviewStatus.Scheduled || interview.Status == InterviewStatus.Postponed))
            {
                interview.Status = InterviewStatus.Cancelled;
                interview.Applicant.Status = ApplicantStatus.Cancelled;

                db.Entry(interview).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Interview cancelled successfully!";

                // Gửi email thông báo cho ứng viên
                string emailSubject = "Interview Cancellation Notification";
                string emailBody = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; }}
                            .email-container {{ max-width: 600px; margin: auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0px 0px 10px #ccc; }}
                            .header {{ text-align: center; padding: 10px; background-color: #dc3545; color: white; font-size: 20px; font-weight: bold; border-radius: 8px 8px 0 0; }}
                            .content {{ padding: 20px; line-height: 1.6; }}
                            .footer {{ text-align: center; font-size: 12px; color: gray; margin-top: 20px; }}
                        </style>
                    </head>
                    <body>
                        <div class='email-container'>
                            <div class='header'>Interview Cancellation Notification</div>
                            <div class='content'>
                                <p>Dear <b>{interview.Applicant.FullName}</b>,</p>
                                <p>We regret to inform you that your interview for the <b>{interview.Vacancy.Title}</b> position has been cancelled.</p>
                                <p>We apologize for any inconvenience caused and appreciate your interest in our company.</p>
                            </div>
                            <div class='footer'>
                                <p>Best Regards, <br>HR Team | Hyprics</p>
                                <p>&copy; 2024 Hyprics. All Rights Reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                var emailService = new EmailService();
                await emailService.SendEmailAsync(interview.Applicant.Email, emailSubject, emailBody);
            }
            else
            {
                TempData["ErrorMessage"] = "Interview cannot be cancelled!";
            }

            return RedirectToAction("InterviewDetails", "VacancySchedule", new { id = InterviewId, VID = VacancyId });
        }

    }
}
    