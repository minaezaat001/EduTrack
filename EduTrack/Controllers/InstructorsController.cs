using EduTrack.Data;
using EduTrack.Models;
using EduTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InstructorsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public InstructorsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // 1. عرض كل المحاضرين مع حساباتهم وعدد كورساتهم
        [HttpGet]
        public IActionResult Index()
        {
            var instructors = _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Courses)
                .ToList();

            return View(instructors);
        }

        // 2. عرض فورم إضافة محاضر جديد
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. حفظ المحاضر + رفع صورته الشخصية + إنشاء حساب الدخول برول Instructor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInstructorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // أولاً: إنشاء حساب ApplicationUser للمحاضر
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // إعطاء الحساب صلاحية Instructor
            await _userManager.AddToRoleAsync(user, "Instructor");

            // ثانياً: رفع الصورة الشخصية للمحاضر (إن وجدت) إلى wwwroot/images/instructors
            string? uniqueImageName = null;
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "instructors");
                Directory.CreateDirectory(uploadsFolder);

                string extension = Path.GetExtension(model.ProfileImage.FileName);
                uniqueImageName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(uploadsFolder, uniqueImageName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(fileStream);
                }
            }

            // ثالثاً: حفظ بيانات المحاضر مربوطة بـ UserId
            var instructor = new Instructor
            {
                FullName = model.FullName,
                Specialization = model.Specialization,
                Bio = model.Bio,
                ImagePath = uniqueImageName,
                UserId = user.Id
            };

            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"تمت إضافة المحاضر ({model.FullName}) وإنشاء حساب الدخول له بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // 4. عرض تفاصيل المحاضر والكورسات المسندة إليه
        [HttpGet]
        public IActionResult Details(int id)
        {
            var instructor = _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Courses)
                .FirstOrDefault(i => i.Id == id);

            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }
    }
}
