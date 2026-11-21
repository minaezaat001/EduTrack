using System.ComponentModel.DataAnnotations;

namespace EduTrack.ViewModels
{
    // يستخدم في شاشة Submissions/Grade لرصد الدرجة والـ Feedback من المحاضر
    public class GradeSubmissionViewModel
    {
        [Required]
        public int SubmissionId { get; set; }

        public int AssignmentId { get; set; }
        public string? StudentName { get; set; }
        public string? AssignmentTitle { get; set; }
        public int MaxDegree { get; set; } = 100;
        public string? SolutionFilePath { get; set; }
        public string? StudentNotes { get; set; }
        public DateTime SubmittedAt { get; set; }

        [Required(ErrorMessage = "يرجى إدخال الدرجة")]
        [Range(0, 1000, ErrorMessage = "الدرجة غير صحيحة")]
        [Display(Name = "الدرجة المستحقة")]
        public int Grade { get; set; }

        [Display(Name = "ملاحظات وتقييم المحاضر (Feedback)")]
        public string? InstructorFeedback { get; set; }
    }
}
