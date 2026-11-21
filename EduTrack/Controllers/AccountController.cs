using EduTrack.Data;
using EduTrack.Models;
using EduTrack.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EduTrack.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ============================================================
        // 1. تهيئة النظام لأول مرة (إنشاء الـ 3 Roles + حساب Admin افتراضي)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> SetupSystem()
        {
            // إنشاء الرولز الثلاثة بشكل صريح ومباشر لو مش موجودين
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await _roleManager.RoleExistsAsync("Instructor"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Instructor"));
            }

            if (!await _roleManager.RoleExistsAsync("Student"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Student"));
            }

            // إنشاء حساب الأدمِن الافتراضي لو مش موجود
            string adminEmail = "admin@edutrack.com";
            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "مدير الأكاديمية (Admin)"
                };

                var result = await _userManager.CreateAsync(adminUser, "123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            TempData["Success"] = "تمت تهيئة النظام بنجاح! يمكنك الدخول الآن بحساب الأدمِن: admin@edutrack.com والباسورد: 123456";
            return RedirectToAction("Login");
        }

        // ============================================================
        // 2. تسجيل طالب جديد (Register) وربطه بالكورس المختار
        // ============================================================
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Courses = new SelectList(_context.Courses.ToList(), "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = new SelectList(_context.Courses.ToList(), "Id", "Title", model.CourseId);
                return View(model);
            }

            // 1) إنشاء حساب الدخول في جدول AspNetUsers
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // 2) إعطاء المستخدم صلاحية Student
                await _userManager.AddToRoleAsync(user, "Student");

                // 3) إنشاء سجل الطالب في جدول Students وربطه بـ UserId و CourseId
                var student = new Student
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    University = model.University,
                    CourseId = model.CourseId,
                    UserId = user.Id
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                // 4) تسجيل دخول الطالب تلقائياً بعد التسجيل
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["Success"] = "تم إنشاء حسابك كطالب وتسجيلك في الكورس بنجاح!";
                return RedirectToAction("Index", "Courses");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.Courses = new SelectList(_context.Courses.ToList(), "Id", "Title", model.CourseId);
            return View(model);
        }

        // ============================================================
        // 3. تسجيل الدخول (Login)
        // ============================================================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    TempData["Success"] = $"أهلاً بك يا {user.FullName}!";
                    return RedirectToAction("Index", "Courses");
                }
            }

            ModelState.AddModelError("", "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            return View(model);
        }

        // ============================================================
        // 4. تسجيل الخروج (Logout) وشاشة عدم الصلاحية (AccessDenied)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
