using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Helpers
{
    public static class CalculateSalaryPerDay
    {
        public static async Task<double> CalculateSalaryPerDayAsync(
            AppDbContext context,
            User user,
            CheckinStatus checkinStatus,
            CheckinStatus checkoutStatus)
        {
            var checkinConfig = await context.CheckinStatusConfigs.FirstOrDefaultAsync(c => c.Id == (int)checkinStatus);
            var checkoutConfig = await context.CheckinStatusConfigs.FirstOrDefaultAsync(c => c.Id == (int)checkoutStatus);

            if (checkinConfig == null || checkoutConfig == null)
                throw new Exception("Không tìm thấy hệ số lương cho trạng thái Checkin hoặc Checkout.");

            var baseSalary = user.BasicSalary;

            var halfSalary = baseSalary / 2.0;

            var salaryToday = (halfSalary * checkinConfig.SalaryMultiplier) + (halfSalary * checkoutConfig.SalaryMultiplier);

            return salaryToday;
        }
    }

}
