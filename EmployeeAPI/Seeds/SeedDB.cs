//using EmployeeAPI.Enums;
//using EmployeeAPI.Helpers;
//using EmployeeAPI.Models;
//using Microsoft.EntityFrameworkCore;

//namespace EmployeeAPI.Seeds
//{
//    public class SeedDB
//    {
//        public static void SeedAdminUser(AppDbContext context)
//        {
//            // Kiểm tra trong DB chính xác
//            var adminUser = context.Users.AsNoTracking().FirstOrDefault(u => u.Username == "Admin123");
//            if (adminUser == null)
//            {
//                context.Users.Add(new User
//                {
//                    UserId = Guid.NewGuid(),
//                    Username = "Admin123",
//                    Password = HashPassword.ComputeHash("anno123"),
//                    Fullname = "Admin",
//                    Role = RoleType.Administrator,
//                    PhoneNumber = "",
//                    Address = "",
//                    BasicSalary = 0,
//                    IsActive = true,
//                    IsDeleted = false,
//                    ImageUrl = null,
//                    DepartmentId = null,
//                    PositionId = null
//                });

//                context.SaveChanges();
//            }
//        }

//        public static async Task SeedHolidayAsync(AppDbContext context)
//        {
//            Console.WriteLine("Start seeding holidays...");
//            if (!context.Holidays.Any())
//            {
//                Console.WriteLine("No holidays found, seeding...");

//                var holidays = new List<Holiday>
//                    {
//                        new Holiday
//                        {
//                            Id = Guid.NewGuid(),
//                            name = "Tết Dương lịch",
//                            startDate = new DateTime(2025, 1, 1),
//                            endDate = new DateTime(2025, 1, 1),
//                            IsDeleted = false
//                        },
//                        new Holiday
//                        {
//                            Id = Guid.NewGuid(),
//                            name = "Tết Nguyên đán",
//                            startDate = new DateTime(2025, 1, 28),
//                            endDate = new DateTime(2025, 2, 1),
//                            IsDeleted = false
//                        },
//                        new Holiday
//                        {
//                            Id = Guid.NewGuid(),
//                            name = "Giỗ Tổ Hùng Vương",
//                            startDate = new DateTime(2025, 4, 10),
//                            endDate = new DateTime(2025, 4, 10),
//                            IsDeleted = false
//                        },
//                        new Holiday
//                        {
//                            Id = Guid.NewGuid(),
//                            name = "Ngày Giải phóng miền Nam",
//                            startDate = new DateTime(2025, 4, 30),
//                            endDate = new DateTime(2025, 4, 30),
//                            IsDeleted = false
//                        },
//                        new Holiday
//                        {
//                            Id = Guid.NewGuid(),
//                            name = "Ngày Quốc tế Lao động",
//                            startDate = new DateTime(2025, 5, 1),
//                            endDate = new DateTime(2025, 5, 1),
//                            IsDeleted = false
//                        },
//                        new Holiday
//                        {
//                            Id = Guid.NewGuid(),
//                            name = "Ngày Quốc khánh",
//                            startDate = new DateTime(2025, 9, 2),
//                            endDate = new DateTime(2025, 9, 2),
//                            IsDeleted = false
//                        }
//                    };
//                context.Holidays.AddRange(holidays);
//                await context.SaveChangesAsync();
//                Console.WriteLine("Holidays seeded.");
//            }
//            else
//            {
//                Console.WriteLine("Holidays already exist.");
//            }
//        }
//    }
//}
