using EduTrack.Data;
using EduTrack.Models;
using EduTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public CoursesController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // ============================================================
        // 1. عرض الكورسات مع الفلترة الذكية حسب صلاحية المستخدم
        // ============================================================
        [HttpGet]
        public IActionResult Index()
        {
            var query = _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Assignments)
                .Include(c => c.Students)
                .AsQueryable();

            string? currentUserId = _userManager.GetUserId(User);

            if (User.IsInRole("Instructor"))
            {
                // المحاضر يرى فقط الكورسات المسندة إليه
                var instructor = _context.Instructors.FirstOrDefault(i => i.UserId == currentUserId);
                if (instructor != null)
                {
                    query = query.Where(c => c.InstructorId == instructor.Id);
                }
                else
                {
                    query = query.Where(c => false);
                }
            }
            else if (User.IsInRole("Student"))
            {
                // الطالب يرى الكورس المسجل به
                var student = _context.Students.FirstOrDefault(s => s.UserId == currentUserId);
                if (student != null)
                {
                    query = query.Where(c => c.Id == student.CourseId);
                }
            }

            return View(query.ToList());
        }

        // ============================================================
        // 2. إضافة كورس جديد مع رفع صورة الغلاف (للأدمِن فقط)
        // ============================================================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Instructors = new SelectList(_context.Instructors.ToList(), "Id", "FullName");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Instructors = new SelectList(_context.Instructors.ToList(), "Id", "FullName", model.InstructorId);
                return View(model);
            }

            string? uniqueThumbnailName = null;
            if (model.ThumbnailFile != null && model.ThumbnailFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "courses");
                Directory.CreateDirectory(uploadsFolder);

                string ext = Path.GetExtension(model.ThumbnailFile.FileName);
                uniqueThumbnailName = Guid.NewGuid().ToString() + ext;
                string filePath = Path.Combine(uploadsFolder, uniqueThumbnailName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ThumbnailFile.CopyToAsync(stream);
                }
            }

            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                TotalHours = model.TotalHours,
                InstructorId = model.InstructorId,
                ThumbnailPath = uniqueThumbnailName
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إنشاء الكورس التدريبي بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // 3. تعديل الكورس واستبدال الصورة القديمة (للأدمِن فقط)
        // ============================================================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            var viewModel = new CourseFormViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                TotalHours = course.TotalHours,
                InstructorId = course.InstructorId,
                ExistingThumbnailPath = course.ThumbnailPath
            };

            ViewBag.Instructors = new SelectList(_context.Instructors.ToList(), "Id", "FullName", course.InstructorId);
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Instructors = new SelectList(_context.Instructors.ToList(), "Id", "FullName", model.InstructorId);
                return View(model);
            }

            var course = _context.Courses.FirstOrDefault(c => c.Id == model.Id);
            if (course == null)
            {
                return NotFound();
            }

            course.Title = model.Title;
            course.Description = model.Description;
            course.TotalHours = model.TotalHours;
            course.InstructorId = model.InstructorId;

            // إذا رفع الأدمِن صورة غلاف جديدة نحذف القديمة ونرفع الجديدة
            if (model.ThumbnailFile != null && model.ThumbnailFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "courses");
                Directory.CreateDirectory(uploadsFolder);

                if (!string.IsNullOrEmpty(course.ThumbnailPath))
                {
                    string oldFilePath = Path.Combine(uploadsFolder, course.ThumbnailPath);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                string ext = Path.GetExtension(model.ThumbnailFile.FileName);
                string newFileName = Guid.NewGuid().ToString() + ext;
                string newFilePath = Path.Combine(uploadsFolder, newFileName);

                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await model.ThumbnailFile.CopyToAsync(stream);
                }

                course.ThumbnailPath = newFileName;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حفظ تعديلات الكورس بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // 4. عرض لوحة الكورس والتاسكات الخاصة به
        // ============================================================
        [HttpGet]
        public IActionResult Details(int id)
        {
            var course = _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Students)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Submissions)
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
    }
}
