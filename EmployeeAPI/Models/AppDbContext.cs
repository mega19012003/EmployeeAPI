using Microsoft.EntityFrameworkCore;
namespace EmployeeAPI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Absent> Absents { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Duty> Duties { get; set; }
        public DbSet<Checkin> Checkins { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<DutyDetail> DutyDetail { get; set; }
        public DbSet<Position> Positions { get; set; }
        //public DbSet<Fine> Fines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DutyDetail>()
            .HasKey(dd => new { dd.StaffId, dd.DutyId }); 

            modelBuilder.Entity<DutyDetail>()
                .HasOne(dd => dd.Staff)
                .WithMany(s => s.DutyDetails)
                .HasForeignKey(dd => dd.StaffId);

            modelBuilder.Entity<DutyDetail>()
                .HasOne(dd => dd.Duty)
                .WithMany(d => d.DutyDetails)
                .HasForeignKey(dd => dd.DutyId);

        }
    }
}
