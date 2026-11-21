using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EduTrack.ViewModels
{
    // يستخدم في شاشتي Courses/Create و Courses/Edit لرفع أو تحديث صورة غلاف الكورس
    public class CourseFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الكورس مطلوب")]
        [Display(Name = "اسم الكورس")]
        public string Title { get; set; }

        [Required(ErrorMessage = "وصف الكورس مطلوب")]
        [Display(Name = "وصف الكورس")]
        public string Description { get; set; }

        [Required]
        [Range(1, 500, ErrorMessage = "عدد الساعات يجب أن يكون بين 1 و 500")]
        [Display(Name = "عدد الساعات التدريبية")]
        public int TotalHours { get; set; }

        [Required(ErrorMessage = "يرجى اختيار المحاضر")]
        [Display(Name = "المحاضر المسؤول")]
        public int InstructorId { get; set; }

        [Display(Name = "صورة غلاف الكورس")]
        public IFormFile? ThumbnailFile { get; set; }

        public string? ExistingThumbnailPath { get; set; }
    }
}
