using System;
using Microsoft.EntityFrameworkCore;
namespace EmployeeAPI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        //public DbSet<Staff> Staffs { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Duty> Duties { get; set; }
        public DbSet<Checkin> Checkins { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<DutyDetail> DutyDetail { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<ScheduleTime> ScheduleTimes { get; set; }
        public DbSet<CheckinStatusConfig> CheckinStatusConfigs { get; set; }
        public DbSet<AllowedIP> AllowedIPs { get; set; }
        //public DbSet<Fine> Fines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<CheckinStatusConfig>().HasData(
                //new CheckinStatusConfig { Id = 0, Name = "OnTime", SalaryMultiplier = 1.0, Note = "Đi đúng giờ" },
                //new CheckinStatusConfig { Id = 1, Name = "Late", SalaryMultiplier = 0.7, Note = "Đi trễ" },
                //new CheckinStatusConfig { Id = 2, Name = "LeaveEarly", SalaryMultiplier = 0.7, Note = "Về sớm" },
                //new CheckinStatusConfig { Id = 3, Name = "Overtime", SalaryMultiplier = 1.3, Note = "Làm thêm giờ" },
                //new CheckinStatusConfig { Id = 4, Name = "Absent", SalaryMultiplier = 0.5, Note = "Nghỉ không phép" },
                //new CheckinStatusConfig { Id = 5, Name = "LeaveWithPermission", SalaryMultiplier = 0.9, Note = "Nghỉ có phép" },
                //new CheckinStatusConfig { Id = 6, Name = "Others", SalaryMultiplier = 1.0, Note = "Khác" }
                new CheckinStatusConfig { Id = 0, Name = "OnTime", SalaryMultiplier = 1.0, Note = "Đi đúng giờ" },
                new CheckinStatusConfig { Id = 1, Name = "Late", SalaryMultiplier = 0.7, Note = "Đi trễ" },
                new CheckinStatusConfig { Id = 2, Name = "Overtime", SalaryMultiplier = 1.3, Note = "Làm thêm giờ" },
                new CheckinStatusConfig { Id = 3, Name = "Absent", SalaryMultiplier = 0.5, Note = "Nghỉ không phép" },
                new CheckinStatusConfig { Id = 4, Name = "LeaveWithPermission", SalaryMultiplier = 0.9, Note = "Nghỉ có phép" },
                new CheckinStatusConfig { Id = 5, Name = "Others", SalaryMultiplier = 1.0, Note = "Khác" }
            );
            base.OnModelCreating(modelBuilder);

            /*modelBuilder.Entity<DutyDetail>()
            .HasKey(dd => new { dd.StaffId, dd.DutyId }); */

            modelBuilder.Entity<DutyDetail>()
                .HasOne(dd => dd.Users)
                .WithMany(s => s.DutyDetails)
                .HasForeignKey(dd => dd.UserId);

            modelBuilder.Entity<DutyDetail>()
                .HasOne(dd => dd.Duty)
                .WithMany(d => d.DutyDetails)
                .HasForeignKey(dd => dd.DutyId);


            //DeleteBehavior.Restrict: cấm xóa nếu còn bản ghi liên quan (rõ ràng, an toàn)
            // Department - Position
            modelBuilder.Entity<Position>()
                .HasOne(p => p.Department)
                .WithMany(d => d.Positions)
                .HasForeignKey(p => p.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict); // Không cascade

            // Department - Employee
            modelBuilder.Entity<User>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict); // Không cascade

            // Position - Employee
            modelBuilder.Entity<User>()
                .HasOne(e => e.Position)
                .WithMany(p => p.Users)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict); // Không cascade

            ///////////////////////////
            // 1. Duty.AssignedBy → User
            modelBuilder.Entity<Duty>()
                .HasOne(d => d.AssignedBy)
                .WithMany()
                .HasForeignKey(d => d.AssignedById)
                .OnDelete(DeleteBehavior.Restrict); // không cascade

            // 2. DutyDetail.User → User
            modelBuilder.Entity<DutyDetail>()
                .HasOne(dd => dd.Users)
                .WithMany(u => u.DutyDetails)
                .HasForeignKey(dd => dd.UserId)
                .OnDelete(DeleteBehavior.Restrict); // không cascade

            // 3. DutyDetail.Duty → Duty
            modelBuilder.Entity<DutyDetail>()
                .HasOne(dd => dd.Duty)
                .WithMany(d => d.DutyDetails)
                .HasForeignKey(dd => dd.DutyId)
                .OnDelete(DeleteBehavior.Restrict); // không cascade

        }
    }
}
