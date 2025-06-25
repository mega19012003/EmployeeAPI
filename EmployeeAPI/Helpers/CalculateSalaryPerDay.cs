//using EmployeeAPI.Enums;
//using EmployeeAPI.Models;
//using Microsoft.EntityFrameworkCore;

//namespace EmployeeAPI.Helpers
//{
//    public static class CalculateSalaryPerDay
//    {
//        public static async Task<double> CalculateSalaryPerDayAsync(
//            AppDbContext context,
//            User user,
//            Enums.LogStatus CheckinMorningStatus,
//            Enums.LogStatus CheckoutAfternoonStatus)
//        {
//            var checkinConfig = await context.LogStatusConfigs.FirstOrDefaultAsync(c => c.Id == (int)CheckinMorningStatus);
//            var checkoutConfig = await context.LogStatusConfigs.FirstOrDefaultAsync(c => c.Id == (int)CheckoutAfternoonStatus);

//            if (checkinConfig == null || checkoutConfig == null)
//                throw new Exception("Không tìm thấy hệ số lương cho trạng thái Checkin hoặc Checkout.");

//            var baseSalary = user.BasicSalary;

//            var halfSalary = baseSalary / 2.0;

//            var salaryToday = (halfSalary * checkinConfig.SalaryMultiplier) + (halfSalary * checkoutConfig.SalaryMultiplier);

//            return salaryToday;
//        }
//    }

//}
