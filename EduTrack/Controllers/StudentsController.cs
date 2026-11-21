using EduTrack.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        // عرض جميع الطلاب المسجلين والكورس الخاص بكل طالب وعدد تسليماته
        [HttpGet]
        public IActionResult Index()
        {
            var students = _context.Students
                .Include(s => s.User)
                .Include(s => s.Course)
                .Include(s => s.Submissions)
                .ToList();

            return View(students);
        }
    }
}
