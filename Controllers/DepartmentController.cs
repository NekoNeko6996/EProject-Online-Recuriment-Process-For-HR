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
    public class DepartmentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Department (có tìm kiếm & phân trang)
        public ActionResult Index(int? page, string search)
        {
            int pageSize = 8; // Số lượng thẻ hiển thị trên mỗi trang
            int pageNumber = (page ?? 1); // Mặc định là trang 1 nếu không có tham số

            var departments = db.Departments.AsQueryable();

            // Nếu có từ khóa tìm kiếm, lọc danh sách phòng ban
            if (!string.IsNullOrEmpty(search))
            {
                departments = departments.Where(d => d.DepartmentName.Contains(search));
            }

            ViewBag.Search = search;
            return View(departments.OrderBy(d => d.DepartmentId).ToPagedList(pageNumber, pageSize));
        }

        // GET: Department/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Department department)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên phòng ban
                if (db.Departments.Any(d => d.DepartmentName == department.DepartmentName))
                {
                    ModelState.AddModelError("DepartmentName", "Department name already exists.");
                    return View(department);
                }

                // Lấy ID mới (D0001, D0002,...)
                string newId = GenerateDepartmentId();
                if (newId == null)
                {
                    ModelState.AddModelError("", "Cannot create more departments.");
                    return View(department);
                }

                department.DepartmentId = newId;

                db.Departments.Add(department);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(department);
        }

        // Hàm tạo DepartmentId an toàn
        private string GenerateDepartmentId()
        {
            var lastDepartment = db.Departments
                .OrderByDescending(d => d.DepartmentId)
                .FirstOrDefault();

            int nextId = 1;
            if (lastDepartment != null)
            {
                if (int.TryParse(lastDepartment.DepartmentId.Substring(1), out int lastId))
                {
                    nextId = lastId + 1;
                }
            }

            if (nextId > 1000) return null; // Giới hạn ID đến D1000

            return "D" + nextId.ToString("0000");
        }

        // GET: Department/Edit/{id}
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Department/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Department department)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng tên nhưng bỏ qua chính nó
                if (db.Departments.Any(d => d.DepartmentName == department.DepartmentName && d.DepartmentId != department.DepartmentId))
                {
                    ModelState.AddModelError("DepartmentName", "Department name already exists.");
                    return View(department);
                }

                db.Entry(department).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(department);
        }

        // GET: Department/Delete/{id}
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Department/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Department department = db.Departments.Find(id);
            if (department != null)
            {
                db.Departments.Remove(department);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
