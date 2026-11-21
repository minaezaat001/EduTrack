using EduTrack.Data;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // عرض الصفحة الرئيسية مع إحصائيات سريعة في الـ ViewBag
        public IActionResult Index()
        {
            ViewBag.CoursesCount = _context.Courses.Count();
            ViewBag.InstructorsCount = _context.Instructors.Count();
            ViewBag.StudentsCount = _context.Students.Count();
            ViewBag.SubmissionsCount = _context.Submissions.Count();

            return View();
        }
    }
}
