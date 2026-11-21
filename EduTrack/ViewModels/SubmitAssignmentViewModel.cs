using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EduTrack.ViewModels
{
    // يستخدم في شاشة Submissions/Submit لرفع ملف الحل من الطالب
    public class SubmitAssignmentViewModel
    {
        [Required]
        public int AssignmentId { get; set; }

        public string? AssignmentTitle { get; set; }
        public string? CourseTitle { get; set; }
        public DateTime? Deadline { get; set; }

        [Required(ErrorMessage = "يرجى إرفاق ملف الحل (PDF أو ZIP)")]
        [Display(Name = "ملف الحل (PDF أو ZIP)")]
        public IFormFile SolutionFile { get; set; }

        [Display(Name = "ملاحظات للمحاضر (اختياري)")]
        public string? StudentNotes { get; set; }
    }
}
