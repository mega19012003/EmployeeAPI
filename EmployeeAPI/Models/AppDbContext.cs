using System;
using EmployeeAPI.Helpers;
using Microsoft.EntityFrameworkCore;
namespace EmployeeAPI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Duty> Duties { get; set; }
        public DbSet<Checkin> Checkins { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<DutyDetail> DutyDetails { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<ScheduleTime> ScheduleTimes { get; set; }
        public DbSet<LogStatusConfig> LogStatusConfigs { get; set; }
        public DbSet<AllowedIP> AllowedIPs { get; set; }
        public DbSet<Holiday> Holidays { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<LogStatusConfig>().HasData(
                new LogStatusConfig { Id = 0, Name = "None", SalaryMultiplier = 0.0, Note = "Chưa checkin/checkout" },
                new LogStatusConfig { Id = 1, Name = "OnTime", SalaryMultiplier = 1.0, Note = "Đi đúng giờ" },
                new LogStatusConfig { Id = 2, Name = "Late", SalaryMultiplier = 0.7, Note = "Đi trễ" },
                new LogStatusConfig { Id = 3, Name = "LeaveEarly", SalaryMultiplier = 0.7, Note = "Về sớm" },
                new LogStatusConfig { Id = 4, Name = "OnHoliday", SalaryMultiplier = 2.0, Note = "Làm vào ngày nghỉ" },
                new LogStatusConfig { Id = 5, Name = "Overtime", SalaryMultiplier = 1.3, Note = "Làm thêm giờ" },
                new LogStatusConfig { Id = 6, Name = "Absent", SalaryMultiplier = 0, Note = "Nghỉ không phép" },
                new LogStatusConfig { Id = 7, Name = "LeaveWithPermission", SalaryMultiplier = 0.9, Note = "Nghỉ có phép" },
                new LogStatusConfig { Id = 8, Name = "Others", SalaryMultiplier = 1.0, Note = "Khác" }
                //new CheckinMorningStatusConfig { Id = 0, Name = "OnTime", SalaryMultiplier = 1.0, Note = "Đi đúng giờ" },
                //new CheckinMorningStatusConfig { Id = 1, Name = "Late", SalaryMultiplier = 0.7, Note = "Đi trễ" },
                //new CheckinMorningStatusConfig { Id = 2, Name = "Overtime", SalaryMultiplier = 1.3, Note = "Làm thêm giờ" },
                //new CheckinMorningStatusConfig { Id = 3, Name = "Absent", SalaryMultiplier = 0.5, Note = "Nghỉ không phép" },
                //new CheckinMorningStatusConfig { Id = 4, Name = "LeaveWithPermission", SalaryMultiplier = 0.9, Note = "Nghỉ có phép" },
                //new CheckinMorningStatusConfig { Id = 5, Name = "Others", SalaryMultiplier = 1.0, Note = "Khác" }
            );

            //modelBuilder.Entity<Checkin>().HasData(
            //    new Checkin
            //    {
            //        Id = Guid.NewGuid(),
            //        UserId = Guid.Empty, // Sẽ cập nhật sau
            //        CheckoutMorning = DateTime.Now,
            //        CheckinMorning = DateTime.Now,
            //        CheckinAfternoon = DateTime.Now,
            //        CheckoutAfternoon = DateTime.Now,
            //        CheckinMorningStatus = Enums.LogStatus.None, 
            //        CheckoutMorningStatus = Enums.LogStatus.None, // OnTime
            //        CheckinAfternoonStatus = Enums.LogStatus.None, // OnTime
            //        CheckoutAfternoonStatus = Enums.LogStatus.None, // OnTime
            //        IsDeleted = false
            //    }
            //);

            /*modelBuilder.Entity<Holiday>().HasData(
                new Holiday
                {
                    Id = Guid.NewGuid(),
                    name = "Tết Dương lịch",
                    startDate = new DateTime(2025, 1, 1),
                    endDate = new DateTime(2025, 1, 1),
                    IsDeleted = false
                },
                new Holiday
                {
                    Id = Guid.NewGuid(),
                    name = "Tết Nguyên đán",
                    startDate = new DateTime(2025, 1, 28),
                    endDate = new DateTime(2025, 2, 1),
                    IsDeleted = false
                },
                new Holiday
                {
                    Id = Guid.NewGuid(),
                    name = "Giỗ Tổ Hùng Vương",
                    startDate = new DateTime(2025, 4, 10),
                    endDate = new DateTime(2025, 4, 10),
                    IsDeleted = false
                },
                new Holiday
                {
                    Id = Guid.NewGuid(),
                    name = "Ngày Giải phóng miền Nam",
                    startDate = new DateTime(2025, 4, 30),
                    endDate = new DateTime(2025, 4, 30),
                    IsDeleted = false
                },
                new Holiday
                {
                    Id = Guid.NewGuid(),
                    name = "Ngày Quốc tế Lao động",
                    startDate = new DateTime(2025, 5, 1),
                    endDate = new DateTime(2025, 5, 1),
                    IsDeleted = false
                },
                new Holiday
                {
                    Id = Guid.NewGuid(),
                    name = "Ngày Quốc khánh",
                    startDate = new DateTime(2025, 9, 2),
                    endDate = new DateTime(2025, 9, 2),
                    IsDeleted = false
                }
            );*/

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
