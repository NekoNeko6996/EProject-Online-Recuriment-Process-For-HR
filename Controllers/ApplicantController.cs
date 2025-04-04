using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Sem3EProjectOnlineCPFH.Models;
using Sem3EProjectOnlineCPFH.Models.Data;
using Sem3EProjectOnlineCPFH.Models.Enum;
using System.Data.Entity;
using PagedList;
using System.Web;
using System.IO;

namespace RecruitmentProces.Controllers
{
    [Authorize(Roles = "hrgroup")]
    public class ApplicantController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly string savePath = "~/Content/Resources/Applicant/Upload/";

        // Generate unique ApplicantId (A0001 - A5000)
        private string GenerateApplicantId()
        {
            const string prefix = "A";
            string lastId = db.Applicants.OrderByDescending(a => a.ApplicantId)
                                         .Select(a => a.ApplicantId)
                                         .FirstOrDefault();

            int nextNumber = lastId == null ? 1 : int.Parse(lastId.Substring(1)) + 1;
            if (nextNumber > 5000) return null;

            string newId;
            do
            {
                newId = $"{prefix}{nextNumber:D4}";
                nextNumber++;
                if (nextNumber > 5000) return null;
            } while (db.Applicants.Any(a => a.ApplicantId == newId));

            return newId;
        }

        // GET: Applicant (Index + Search + Pagination)
        public ActionResult Index(string search, int? page)
        {
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            var applicants = db.Applicants.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                applicants = applicants.Where(a => a.FullName.Contains(search) ||
                                                    a.Email.Contains(search) ||
                                                    a.PhoneNumber.Contains(search));
            }

            ViewBag.Search = search;
            return View(applicants.OrderByDescending(a => a.CreatedAt).ToPagedList(pageNumber, pageSize));
        }

        // GET: Applicant/Create
        public ActionResult Create()
        {
            string newId = GenerateApplicantId();
            if (newId == null)
            {
                TempData["Error"] = "Cannot create more than 5000 applicants.";
                return RedirectToAction("Index");
            }

            ViewBag.ApplicantId = newId;
            return View();
        }

        // POST: Applicant/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Applicant applicant, HttpPostedFileBase AvatarFile, HttpPostedFileBase CVFile, string VacancyInput)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(applicant.ApplicantId))
                {
                    applicant.ApplicantId = GenerateApplicantId();
                    if (applicant.ApplicantId == null)
                    {
                        ModelState.AddModelError("", "Cannot create more than 5000 applicants.");
                        return View(applicant);
                    }
                }

                // Lưu Avatar
                if (AvatarFile != null && AvatarFile.ContentLength > 0)
                {
                    string avatarExt = Path.GetExtension(AvatarFile.FileName);
                    string avatarFileName = Guid.NewGuid().ToString() + avatarExt;
                    string avatarPath = Path.Combine(Server.MapPath(savePath + "Avatars/"), avatarFileName);

                    AvatarFile.SaveAs(avatarPath);
                    applicant.AvatarPath = savePath + "Avatars/" + avatarFileName;
                }
                else
                {
                    applicant.AvatarPath = ViewBag.DefaultAvatar;
                }

                // Lưu CV với tên file ngẫu nhiên
                if (CVFile != null && CVFile.ContentLength > 0)
                {
                    string cvExt = Path.GetExtension(CVFile.FileName);
                    string cvFileName = Guid.NewGuid().ToString() + cvExt; // Tạo tên file ngẫu nhiên
                    string cvPath = Path.Combine(Server.MapPath(savePath + "CVs/"), cvFileName);

                    CVFile.SaveAs(cvPath);
                    applicant.CVPath = savePath + "CVs/" + cvFileName; // Lưu đường dẫn
                }

                // Lưu Vacancy Name (nếu có)
                applicant.AttachedVacancies = VacancyInput;
                applicant.Status = ApplicantStatus.NotApproved;

                // Lưu vào database
                db.Applicants.Add(applicant);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.ApplicantId = applicant.ApplicantId;
            return View(applicant);
        }


        // GET: Applicant/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Applicant applicant = db.Applicants.Find(id);
            if (applicant == null) return HttpNotFound();
            return View(applicant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Applicant model, HttpPostedFileBase AvatarFile, HttpPostedFileBase CVFile)
        {
            if (ModelState.IsValid)
            {
                var applicant = db.Applicants.Find(model.ApplicantId);
                if (applicant == null)
                {
                    return HttpNotFound();
                }

                if (AvatarFile != null && AvatarFile.ContentLength > 0)
                {
                    // generate new avatar name
                    string avatarName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);
                    string avatarPath = savePath + "Avatars/" + avatarName;
                    AvatarFile.SaveAs(Server.MapPath(avatarPath));


                    // check if not default avatar delete old avatar
                    if (!string.IsNullOrEmpty(applicant.AvatarPath) && !applicant.AvatarPath.Contains(ViewBag.DefaultAvatar))
                    {
                        string fullPath = Server.MapPath(applicant.AvatarPath);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }

                    applicant.AvatarPath = avatarPath;
                }

                if (CVFile != null && CVFile.ContentLength > 0)
                {
                    // generate new cv name
                    string cvName = Guid.NewGuid().ToString() + Path.GetExtension(CVFile.FileName);
                    string cvPath = savePath + "CVs/" + cvName;
                    CVFile.SaveAs(Server.MapPath(cvPath));
                    applicant.CVPath = cvPath;
                }

                // Cập nhật thông tin khác
                applicant.FullName = model.FullName;
                applicant.Email = model.Email;
                applicant.PhoneNumber = model.PhoneNumber;
                applicant.CreatedAt = model.CreatedAt;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Applicant/Delete/5 (Hiển thị trang xác nhận)
        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Applicant applicant = db.Applicants.Find(id);
            if (applicant == null) return HttpNotFound();

            return View(applicant);
        }

        // POST: Applicant/Delete/5 (Xóa dữ liệu)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var applicant = db.Applicants.Find(id);
            if (applicant == null) return HttpNotFound();

            // Xóa Avatar nếu có
            if (!string.IsNullOrEmpty(applicant.AvatarPath) && !applicant.AvatarPath.Contains(ViewBag.DefaultAvatar))
            {
                string fullPath = Server.MapPath(applicant.AvatarPath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            // Xóa CV nếu có
            if (!string.IsNullOrEmpty(applicant.CVPath))
            {
                string fullPath = Server.MapPath(applicant.CVPath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            // Xóa Applicant khỏi database
            db.Applicants.Remove(applicant);
            db.SaveChanges();

            return RedirectToAction("Index");
        }


        // GET: Applicant/Details/5
        public ActionResult Details(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Applicant applicant = db.Applicants.Find(id);
            if (applicant == null) return HttpNotFound();
            return View(applicant);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
