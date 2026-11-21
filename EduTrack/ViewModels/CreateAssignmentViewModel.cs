using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EduTrack.ViewModels
{
    // يستخدم في شاشة Assignments/Create لإضافة تاسك جديد مع رفع ملف PDF للشرح
    public class CreateAssignmentViewModel
    {
        [Required]
        public int CourseId { get; set; }

        public string? CourseTitle { get; set; }

        [Required(ErrorMessage = "عنوان التاسك مطلوب")]
        [Display(Name = "عنوان التاسك")]
        public string Title { get; set; }

        [Required(ErrorMessage = "وصف التاسك مطلوب")]
        [Display(Name = "تفاصيل ومطلوبات التاسك")]
        public string Description { get; set; }

        [Required(ErrorMessage = "آخر موعد للتسليم مطلوب")]
        [Display(Name = "آخر موعد للتسليم (Deadline)")]
        public DateTime Deadline { get; set; } = DateTime.Now.AddDays(7);

        [Range(1, 1000)]
        [Display(Name = "الدرجة النهائية للتاسك")]
        public int MaxDegree { get; set; } = 100;

        [Display(Name = "ملف شرح التاسك (PDF اختياري)")]
        public IFormFile? AttachmentFile { get; set; }
    }
}
