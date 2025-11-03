using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
 public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
   : base(options)
        {
        }

 public DbSet<Department> Departments { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Class> Classes { get; set; }
      public DbSet<Student> Students { get; set; }
    public DbSet<Course> Courses { get; set; }
public DbSet<Grade> Grades { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AttendanceSession> AttendanceSessions { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
      base.OnModelCreating(modelBuilder);

      // Configure composite primary key for Grade
         modelBuilder.Entity<Grade>()
         .HasKey(g => new { g.StudentId, g.CourseId });

            // Configure relationships
            
            // Department - Teacher relationship
            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.Department)
                .WithMany(d => d.Teachers)
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Class>()
                .HasOne(c => c.Department)
                .WithMany(d => d.Classes)
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Class>()
         .HasOne(c => c.Teacher)
          .WithMany(t => t.Classes)
      .HasForeignKey(c => c.TeacherId)
   .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
          .HasOne(s => s.Class)
                .WithMany(c => c.Students)
      .HasForeignKey(s => s.ClassId)
   .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Course>()
   .HasOne(c => c.Department)
         .WithMany(d => d.Courses)
   .HasForeignKey(c => c.DepartmentId)
 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
  .HasOne(c => c.Teacher)
          .WithMany(t => t.Courses)
                .HasForeignKey(c => c.TeacherId)
          .OnDelete(DeleteBehavior.Restrict);

   modelBuilder.Entity<Grade>()
                .HasOne(g => g.Student)
       .WithMany(s => s.Grades)
       .HasForeignKey(g => g.StudentId)
 .OnDelete(DeleteBehavior.Restrict);

          modelBuilder.Entity<Grade>()
         .HasOne(g => g.Course)
  .WithMany(c => c.Grades)
   .HasForeignKey(g => g.CourseId)
      .OnDelete(DeleteBehavior.Restrict);

    // Configure unique constraints
   modelBuilder.Entity<Teacher>()
     .HasIndex(t => t.Username)
          .IsUnique();

         modelBuilder.Entity<Student>()
           .HasIndex(s => s.Username)
   .IsUnique();

 modelBuilder.Entity<User>()
 .HasIndex(u => u.Username)
         .IsUnique();

            // Attendance relationships
            modelBuilder.Entity<AttendanceSession>()
                .HasOne(s => s.Course)
                .WithMany()
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AttendanceSession>()
                .HasOne(s => s.Teacher)
                .WithMany()
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Session)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.MarkedByTeacher)
                .WithMany()
                .HasForeignKey(a => a.MarkedByTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint for attendance
            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.SessionId, a.StudentId })
                .IsUnique();
}
    }
}
