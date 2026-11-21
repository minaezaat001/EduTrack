using EduTrack.Data;
using EduTrack.Models;
using EduTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Controllers
{
    [Authorize]
    public class AssignmentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AssignmentsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. فورم إضافة تاسك جديد داخل كورس معين (للمحاضر والأدمِن)
        [HttpGet]
        [Authorize(Roles = "Instructor,Admin")]
        public IActionResult Create(int courseId)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == courseId);
            if (course == null)
            {
                return NotFound();
            }

            var model = new CreateAssignmentViewModel
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                Deadline = DateTime.Now.AddDays(7),
                MaxDegree = 100
            };

            return View(model);
        }

        // 2. حفظ التاسك ورفع ملف الشرح PDF مع التحقق من الامتداد
        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAssignmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? pdfFileName = null;
            if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
            {
                string ext = Path.GetExtension(model.AttachmentFile.FileName).ToLower();
                if (ext != ".pdf")
                {
                    ModelState.AddModelError("AttachmentFile", "عفواً، يجب أن يكون ملف شرح التاسك بصيغة PDF فقط (.pdf)");
                    return View(model);
                }

                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "assignments");
                Directory.CreateDirectory(uploadsFolder);

                pdfFileName = Guid.NewGuid().ToString() + ext;
                string filePath = Path.Combine(uploadsFolder, pdfFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AttachmentFile.CopyToAsync(stream);
                }
            }

            var assignment = new Assignment
            {
                CourseId = model.CourseId,
                Title = model.Title,
                Description = model.Description,
                Deadline = model.Deadline,
                MaxDegree = model.MaxDegree,
                AttachmentFilePath = pdfFileName
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم نشر التاسك للطلاب بنجاح!";
            return RedirectToAction("Details", "Courses", new { id = model.CourseId });
        }

        // 3. عرض تفاصيل التاسك + جدول تسليمات الطلاب (للمحاضر والأدمِن)
        [HttpGet]
        public IActionResult Details(int id)
        {
            var assignment = _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Student)
                .FirstOrDefault(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }
    }
}
