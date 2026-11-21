using EduTrack.Data;
using EduTrack.Models;
using EduTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Controllers
{
    [Authorize]
    public class SubmissionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public SubmissionsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // ============================================================
        // 1. شاشة رفع الحل من الطالب (Submit)
        // ============================================================
        [HttpGet]
        [Authorize(Roles = "Student")]
        public IActionResult Submit(int assignmentId)
        {
            var assignment = _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefault(a => a.Id == assignmentId);

            if (assignment == null)
            {
                return NotFound();
            }

            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = assignment.Id,
                AssignmentTitle = assignment.Title,
                CourseTitle = assignment.Course?.Title,
                Deadline = assignment.Deadline
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitAssignmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // جلب الطالب الحالي عن طريق UserId
            string? currentUserId = _userManager.GetUserId(User);
            var student = _context.Students.FirstOrDefault(s => s.UserId == currentUserId);

            if (student == null)
            {
                TempData["Error"] = "لم يتم العثور على ملف الطالب الخاص بك.";
                return RedirectToAction("Index", "Home");
            }

            // التحقق من امتداد الملف المرفوع (.zip, .rar, .pdf)
            string ext = Path.GetExtension(model.SolutionFile.FileName).ToLower();
            if (ext != ".zip" && ext != ".rar" && ext != ".pdf")
            {
                ModelState.AddModelError("SolutionFile", "يرجى رفع ملف بصيغة ZIP أو RAR أو PDF فقط.");
                return View(model);
            }

            // حفظ الملف في wwwroot/uploads/submissions
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "submissions");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + ext;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.SolutionFile.CopyToAsync(stream);
            }

            var submission = new Submission
            {
                AssignmentId = model.AssignmentId,
                StudentId = student.Id,
                SolutionFilePath = uniqueFileName,
                StudentNotes = model.StudentNotes,
                SubmittedAt = DateTime.Now,
                Status = "Pending"
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إرسال حلك للمحاضر بنجاح! يمكنك متابعة حالة التصحيح من هنا.";
            return RedirectToAction(nameof(MySubmissions));
        }

        // ============================================================
        // 2. شاشة رصد الدرجة والـ Feedback من المحاضر (Grade)
        // ============================================================
        [HttpGet]
        [Authorize(Roles = "Instructor,Admin")]
        public IActionResult Grade(int id)
        {
            var submission = _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .FirstOrDefault(s => s.Id == id);

            if (submission == null)
            {
                return NotFound();
            }

            var model = new GradeSubmissionViewModel
            {
                SubmissionId = submission.Id,
                AssignmentId = submission.AssignmentId,
                StudentName = submission.Student?.FullName,
                AssignmentTitle = submission.Assignment?.Title,
                MaxDegree = submission.Assignment?.MaxDegree ?? 100,
                SolutionFilePath = submission.SolutionFilePath,
                StudentNotes = submission.StudentNotes,
                SubmittedAt = submission.SubmittedAt,
                Grade = submission.Grade ?? 0,
                InstructorFeedback = submission.InstructorFeedback
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(GradeSubmissionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var submission = _context.Submissions.FirstOrDefault(s => s.Id == model.SubmissionId);
            if (submission == null)
            {
                return NotFound();
            }

            submission.Grade = model.Grade;
            submission.InstructorFeedback = model.InstructorFeedback;
            submission.Status = "Graded";

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم رصد الدرجة واعتماد التقييم للطالب بنجاح!";
            return RedirectToAction("Details", "Assignments", new { id = submission.AssignmentId });
        }

        // ============================================================
        // 3. شاشة تسليماتي ودرجاتي للطالب (MySubmissions)
        // ============================================================
        [HttpGet]
        [Authorize(Roles = "Student")]
        public IActionResult MySubmissions()
        {
            string? currentUserId = _userManager.GetUserId(User);
            var student = _context.Students.FirstOrDefault(s => s.UserId == currentUserId);

            if (student == null)
            {
                return View(new List<Submission>());
            }

            var mySubmissions = _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Course)
                .Where(s => s.StudentId == student.Id)
                .OrderByDescending(s => s.SubmittedAt)
                .ToList();

            return View(mySubmissions);
        }
    }
}
