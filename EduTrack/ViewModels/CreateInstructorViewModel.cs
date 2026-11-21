using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EduTrack.ViewModels
{
    // يستخدم في شاشة Instructors/Create لإنشاء محاضر + حساب Identity + رفع صورته الشخصية
    public class CreateInstructorViewModel
    {
        [Required(ErrorMessage = "اسم المحاضر مطلوب")]
        [Display(Name = "اسم المحاضر بالكامل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "التخصص مطلوب")]
        [Display(Name = "التخصص التقني")]
        public string Specialization { get; set; }

        [Display(Name = "نبذة مختصرة")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [Display(Name = "البريد الإلكتروني (للدخول)")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [Display(Name = "الصورة الشخصية للمحاضر")]
        public IFormFile? ProfileImage { get; set; }
    }
}
