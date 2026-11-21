using EduTrack.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Data
{
    // يورث من IdentityDbContext<ApplicationUser> لينشئ جداول الـ Identity الـ 7 + جداول المشروع
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // منع تعارض الـ Cascade Delete في SQL Server بين الكورس والطالب والتسليمات
            builder.Entity<Student>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Submission>()
                .HasOne(sub => sub.Student)
                .WithMany(s => s.Submissions)
                .HasForeignKey(sub => sub.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
