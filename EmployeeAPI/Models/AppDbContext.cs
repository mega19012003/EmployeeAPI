using CloudinaryDotNet.Actions;
using EmployeeAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.Design;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
        public DbSet<Company> Companies { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //modelBuilder.Entity<LogStatusConfig>().HasData(
            //    new LogStatusConfig { Id = 0,  Name = "None",                       SalaryMultiplier = 0.0, Note = "Chưa checkin/checkout" },
            //    new LogStatusConfig { Id = 1,  Name = "OnTime",                     SalaryMultiplier = 1.0, Note = "Đi đúng giờ" },
            //    new LogStatusConfig { Id = 2,  Name = "Late",                       SalaryMultiplier = 0.7, Note = "Đi trễ" },
            //    new LogStatusConfig { Id = 3,  Name = "LeaveEarly",                 SalaryMultiplier = 0.7, Note = "Về sớm" },
            //    new LogStatusConfig { Id = 4,  Name = "LateAndLeaveEarly",          SalaryMultiplier = 0.5, Note = "Đi trễ và về sớm" },
            //    new LogStatusConfig { Id = 5,  Name = "Overtime",                   SalaryMultiplier = 1.3, Note = "Làm thêm giờ" },
            //    new LogStatusConfig { Id = 6,  Name = "LateAndOvertime",            SalaryMultiplier = 0.7, Note = "Đi trễ và làm thêm giờ" },
            //    new LogStatusConfig { Id = 7,  Name = "Absent",                     SalaryMultiplier = 0.0, Note = "Vắng" },
            //    new LogStatusConfig { Id = 8,  Name = "OnHoliday",                  SalaryMultiplier = 2.0, Note = "Làm vào ngày nghỉ" },
            //    new LogStatusConfig { Id = 9,  Name = "OnHolidayLate",              SalaryMultiplier = 1.5, Note = "Đi trễ vào ngày nghỉ" },
            //    new LogStatusConfig { Id = 10, Name = "OnHolidayLeaveEarly",        SalaryMultiplier = 1.5, Note = "Về sớm vào ngày nghỉ" },
            //    new LogStatusConfig { Id = 11, Name = "OnHolidayOvertime",          SalaryMultiplier = 3.0, Note = "Làm thêm giờ vào ngày nghỉ" },
            //    new LogStatusConfig { Id = 12, Name = "OnHolidayLateAndOvertime",   SalaryMultiplier = 1.5, Note = "Đi trễ và làm thêm giờ vào ngày nghỉ" },
            //    new LogStatusConfig { Id = 13, Name = "OnHolidayLateAndLeaveEarly", SalaryMultiplier = 2.0, Note = "Đi trễ và về sớm vào ngày nghỉ" },
            //    new LogStatusConfig { Id = 14, Name = "Others",                     SalaryMultiplier = 0.5, Note = "Khác" }
            //);

            //modelBuilder.Entity<Department>().HasData(
            //    new Department { Id = Guid.NewGuid(), Name = "Phòng nhân sự", isDeleted = false },
            //    new Department { Id = Guid.NewGuid(), Name = "Phòng Kế toán", isDeleted = false },
            //    new Department { Id = Guid.NewGuid(), Name = "Phòng Kinh doanh", isDeleted = false },
            //    new Department { Id = Guid.NewGuid(), Name = "Phòng Kỹ thuật", isDeleted = false },
            //    new Department { Id = Guid.NewGuid(), Name = "Phòng Marketing", isDeleted = false },
            //    new Department { Id = Guid.NewGuid(), Name = "Phòng Chăm sóc Khách hàng", isDeleted = false }
            //);

            //modelBuilder.Entity<Position>().HasData(
            //    new Position { Id = Guid.NewGuid(), Name = "Trưởng phòng" },
            //    new Position { Id = Guid.NewGuid(), Name = "Phó phòng" },
            //    new Position { Id = Guid.NewGuid(), Name = "Nhân viên Kinh doanh" },
            //    new Position { Id = Guid.NewGuid(), Name = "Nhân viên Kế toán" },
            //    new Position { Id = Guid.NewGuid(), Name = "Nhân viên Nhân sự" },
            //    new Position { Id = Guid.NewGuid(), Name = "Nhân viên IT" },
            //    new Position { Id = Guid.NewGuid(), Name = "Chuyên viên Marketing" },
            //    new Position { Id = Guid.NewGuid(), Name = "Nhân viên Chăm sóc Khách hàng" }
            //);

            //modelBuilder.Entity<User>().HasData(
            //    //System Admin - quản lý dc tất cả
            //    new User { UserId = Guid.NewGuid(), Username = "AdminSystem123", Password = HashPassword.Hash("ADMIN!SYSTEM"), Fullname = "Phạm Đức Cường", Role = Enums.RoleType.SystemAdmin, 
            //        Address = "TP. HCM", PhoneNumber = "0901000011", ImageUrl = "", SalaryPerHour = 20000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty },
            //    //Admin - quản lý 1 cty
            //    new User { UserId = Guid.NewGuid(), Username = "Admin01", Password = HashPassword.Hash("ADMIN!SYSTEM"), Fullname = "Phạm Đức Cường", 
            //        Role = Enums.RoleType.Administrator, Address = "TP. HCM", PhoneNumber = "0901000012", ImageUrl = "", SalaryPerHour = 20000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty },
            //    new User { UserId = Guid.NewGuid(), Username = "Admin02", Password = HashPassword.Hash("ADMIN!SYSTEM"), Fullname = "Phạm Đức Cường", 
            //        Role = Enums.RoleType.Administrator, Address = "TP. HCM", PhoneNumber = "0901000013", ImageUrl = "", SalaryPerHour = 20000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty },
            //    ////Manager - quản lý nhóm employee
            //    new User { UserId = Guid.NewGuid(), Username = "Manager01", Password = HashPassword.Hash("Manager!234"), Fullname = "Nguyễn Văn Quang", 
            //        Role = Enums.RoleType.Manager, Address = "Hà Nội", PhoneNumber = "0901000001", ImageUrl = "", SalaryPerHour = 20000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty },
            //    new User { UserId = Guid.NewGuid(), Username = "Manager02", Password = HashPassword.Hash("Manager!234"), Fullname = "Lê Thị Hoa", 
            //        Role = Enums.RoleType.Manager, Address = "TP. HCM", PhoneNumber = "0901000002", ImageUrl = "", SalaryPerHour = 20000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty },
            //    new User { UserId = Guid.NewGuid(), Username = "Manager03", Password = HashPassword.Hash("Manager!234"), Fullname = "Nguyễn Phúc Hậu",
            //        Role = Enums.RoleType.Manager, Address = "Hà Nội", PhoneNumber = "0901000009", ImageUrl = "", SalaryPerHour = 18000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty },
            //    new User { UserId = Guid.NewGuid(), Username = "Manager04", Password = HashPassword.Hash("Manager!234"), Fullname = "Lê Bảo Nhân", 
            //        Role = Enums.RoleType.Manager, Address = "Hà Nội", PhoneNumber = "0901000010", ImageUrl = "", SalaryPerHour = 15000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty },
            //    //// employee
            //    new User { UserId = Guid.NewGuid(), Username = "User01", Password = HashPassword.Hash("User!234"), Fullname = "Trần Minh Quân", 
            //        Role = Enums.RoleType.Employee, Address = "Ninh Bình", PhoneNumber = "0901000008", ImageUrl = "", SalaryPerHour = 10000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty },
            //    new User { UserId = Guid.NewGuid(), Username = "User02", Password = HashPassword.Hash("User!234"), Fullname = "Phạm Văn An", 
            //        Role = Enums.RoleType.Employee, Address = "Hải Phòng", PhoneNumber = "0901000003", ImageUrl = "", SalaryPerHour = 12000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty },
            //    new User { UserId = Guid.NewGuid(), Username = "User03", Password = HashPassword.Hash("User!234"), Fullname = "Nguyễn Phúc Bảo", 
            //        Role = Enums.RoleType.Employee, Address = "Đà Nẵng", PhoneNumber = "0901000004", ImageUrl = "", SalaryPerHour = 15000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty },
            //    new User { UserId = Guid.NewGuid(), Username = "User04", Password = HashPassword.Hash("User!234"), Fullname = "Trần Minh Đức", 
            //        Role = Enums.RoleType.Employee, Address = "Huế", PhoneNumber = "0901000005", ImageUrl = "", SalaryPerHour = 8000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty },
            //    new User { UserId = Guid.NewGuid(), Username = "User05", Password = HashPassword.Hash("User!234"), Fullname = "Lê Văn Dũng", 
            //        Role = Enums.RoleType.Employee, Address = "Cần Thơ", PhoneNumber = "0901000006", ImageUrl = "", SalaryPerHour = 5000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty },
            //    new User { UserId = Guid.NewGuid(), Username = "User06", Password = HashPassword.Hash("User!234"), Fullname = "Vũ Thị Ngọc Bích", 
            //        Role = Enums.RoleType.Employee, Address = "Bình Dương", PhoneNumber = "0901000007", ImageUrl = "", SalaryPerHour = 12000, IsDeleted = false, IsActive = true, TokenVersion = 0, RefreshToken = string.Empty }
            //);

            /*modelBuilder.Entity<Holiday>().HasData(
                new Holiday { Id = Guid.NewGuid(), name = "Tết Dương lịch", startDate = new DateTime(2025, 1, 1), endDate = new DateTime(2025, 1, 1), IsDeleted = false },
                new Holiday { Id = Guid.NewGuid(), name = "Tết Nguyên đán", startDate = new DateTime(2025, 1, 28), endDate = new DateTime(2025, 2, 1), IsDeleted = false },
                new Holiday { Id = Guid.NewGuid(), name = "Giỗ Tổ Hùng Vương", startDate = new DateTime(2025, 4, 10), endDate = new DateTime(2025, 4, 10), },
                new Holiday { Id = Guid.NewGuid(), name = "Ngày Giải phóng miền Nam", startDate = new DateTime(2025, 4, 30), endDate = new DateTime(2025, 4, 30), IsDeleted = false },
                new Holiday { Id = Guid.NewGuid(), name = "Ngày Quốc tế Lao động", startDate = new DateTime(2025, 5, 1), endDate = new DateTime(2025, 5, 1), IsDeleted = false },
                new Holiday { Id = Guid.NewGuid(), name = "Ngày Quốc khánh", startDate = new DateTime(2025, 9, 2), endDate = new DateTime(2025, 9, 2), IsDeleted = false }
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
