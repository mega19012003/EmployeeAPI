using EmployeeAPI.Enums;
using EmployeeAPI.Helpers;
using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Seeds
{
    public class UserSeed
    {
        public static void SeedAdminUser(AppDbContext context)
        {
            // Kiểm tra trong DB chính xác
            var adminUser = context.Users.AsNoTracking().FirstOrDefault(u => u.Username == "Admin123");
            if (adminUser == null)
            {
                context.Users.Add(new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "Admin123",
                    Password = HashPassword.ComputeHash("anno123"),
                    Fullname = "Admin",
                    Role = RoleType.Administrator,
                    PhoneNumber = "",
                    Address = "",
                    BasicSalary = 0,
                    IsActive = true,
                    IsDeleted = false,
                    ImageUrl = null,
                    DepartmentId = null,
                    PositionId = null
                });

                context.SaveChanges();
            }
        }
    }
}
