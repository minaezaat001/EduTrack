using System.ComponentModel.DataAnnotations;

namespace EduTrack.ViewModels
{
    // يستخدم في شاشة Account/Register لتسجيل طالب جديد واختيار الكورس
    public class RegisterStudentViewModel
    {
        [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
        [Display(Name = "الاسم بالكامل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الموبايل / واتساب")]
        public string PhoneNumber { get; set; }

        [Display(Name = "الجامعة / الكلية")]
        public string? University { get; set; }

        [Required(ErrorMessage = "يرجى اختيار الكورس")]
        [Display(Name = "الكورس المراد التسجيل به")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        [Display(Name = "تأكيد كلمة المرور")]
        public string ConfirmPassword { get; set; }
    }
}
