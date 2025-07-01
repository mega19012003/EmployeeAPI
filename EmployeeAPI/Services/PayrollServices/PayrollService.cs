using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.LogStatusConfigs;
using EmployeeAPI.Repositories.Payrolls;
using EmployeeAPI.Repositories.Users;
using EmployeeAPI.Services.CheckinServices;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static EmployeeAPI.Services.PayrollServices.ResponseModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EmployeeAPI.Services.PayrollServices
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly ICheckinRepository _checkinRepository;
        private readonly ILogStatusConfigRepository _logStatusConfigRepository;
        private readonly ILogger<PayrollService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        public PayrollService(IPayrollRepository payrollRepository, ILogStatusConfigRepository logStatusConfigRepository, IUserRepository userRepository, ICheckinRepository checkinRepository, ILogger<PayrollService> logger, AppDbContext context)
        {
            _payrollRepository = payrollRepository;

            _userRepository = userRepository;
            _checkinRepository = checkinRepository;
            _logStatusConfigRepository = logStatusConfigRepository;
            _logger = logger;
            _context = context;
        }

        public async Task<PagedResult<ResponseModel.PayrollResultDto>> GetAllPayrolls(Guid currentUserId, IList<string> currentUserRoles, string? name, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _context.Payrolls
                .Include(p => p.Users)
                .Where(p => !p.IsDeleted);

            if (currentUserRoles.Contains("Manager"))
            {
                var manager = await _context.Users.FindAsync(currentUserId);
                if (manager == null)
                    throw new ArgumentException("Manager not found");

                if (manager.DepartmentId == null)
                    throw new ArgumentException("Manager does not belong to any department");

                var departmentId = manager.DepartmentId;

                query = query.Where(p => p.Users.DepartmentId == departmentId);
            }

            if (!string.IsNullOrEmpty(name))
            {
                var nameLower = name.ToLower();
                query = query.Where(p => p.Users.Fullname.ToLower().Contains(nameLower));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new ResponseModel.PayrollResultDto
                {
                    Id = c.Id,
                    Name = c.Users.Fullname,
                    Salary = c.Salary,
                    DaysWorked = c.DaysWorked,
                    CreatedDate = c.CreatedDate,
                    Note = c.Note,
                }).ToListAsync();

            return new PagedResult<ResponseModel.PayrollResultDto>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }
        public async Task<ResponseModel.PayrollResultDto> GetById(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            var payroll = await _payrollRepository.GetPayrollById(id);
            if (payroll == null)
                throw new ArgumentException("Cannot find payroll");

            var manager = currentUserRoles.Contains("Manager");
            var employee = currentUserRoles.Contains("Employee");

            if (manager)
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (currentUser.DepartmentId == null)
                    throw new ArgumentException("Manager does not belong to any department");
                if (payroll.Users.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("Manager cannot access payroll of an User from other department");
            }
            else if (employee)
            {
                if (payroll.UserId != currentUserId)
                    throw new UnauthorizedAccessException("Employee can only access their own payroll");
            }

            return new ResponseModel.PayrollResultDto
            {
                Id = payroll.Id,
                Name = payroll.Users.Fullname,
                Salary = payroll.Salary,
                DaysWorked = payroll.DaysWorked,
                CreatedDate = payroll.CreatedDate,
                Note = payroll.Note
            };
        }
        public async Task<string> SoftDeletePayroll(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _payrollRepository.GetPayrollById(id);
                if (existing == null)
                    throw new ArgumentException("Cannot find checkin");

                var employee = await _userRepository.GetByIdAsync(existing.UserId);
                if (employee == null)
                    throw new ArgumentException("Cannot find employee for this checkin");

                var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                if (currentUserRoles.Contains("Manager"))
                {
                    if (currentUser.DepartmentId != employee.DepartmentId)
                        throw new UnauthorizedAccessException("Manager cannot delete payroll of an User from other department");

                    if (currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager does not belong to any department");
                }

                var result = await _payrollRepository.SoftDeletePayroll(id);
                if (result == null) return null;
                result.IsDeleted = true;

                await _payrollRepository.SoftDeletePayroll(result.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Payroll " + id + " deleted";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting payroll. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<PagedResult<ResponseModel.PayrollResultDto>> GetPayrollByUser(Guid? staffId, Guid currentUserId, IList<string> currentUserRoles, int? pageIndex, int? pageSize)
        {
            try
            {
                // Gán ngầm staffId nếu user là employee
                if (!currentUserRoles.Contains("Administrator") && !currentUserRoles.Contains("Manager"))
                {
                    staffId = currentUserId;
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                    if (currentUser == null)
                        throw new ArgumentException("User not found");

                    if (currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager does not belong to any department");


                    // Kiểm tra user được lấy có tồn tại không
                    var findUser = await _userRepository.GetByIdAsync(staffId.Value);
                    if (findUser == null)
                        throw new ArgumentException("Cannot find user");

                    if (findUser.DepartmentId != currentUser.DepartmentId)
                        throw new UnauthorizedAccessException("Manager cannot access checkins from other departments");

                }
                else if (currentUserRoles.Contains("Administrator"))
                {
                    // Admin: bắt buộc phải nhập staffId
                    if (staffId == null || staffId == Guid.Empty)
                        throw new ArgumentException("Please input userId");
                }
                else
                {
                    throw new UnauthorizedAccessException("You do not have permission");
                }

                pageIndex ??= 1;
                pageSize ??= 10;

                var user = await _userRepository.GetByIdAsync(staffId.Value);
                if (user == null)
                    throw new ArgumentException("Cannot find user");

                var query = _context.Payrolls
                    .Where(p => !p.IsDeleted && p.UserId == staffId.Value)
                    .Include(p => p.Users);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(c => new ResponseModel.PayrollResultDto
                    {
                        Id = c.Id,
                        CreatedDate = c.CreatedDate,
                        Salary = c.Salary,
                        Note = c.Note,
                        Name = c.Users.Fullname ?? null,
                    }).ToListAsync();

                return new PagedResult<ResponseModel.PayrollResultDto>
                {
                    Items = items,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting checkin. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        ////////////////////////////////////////////////////////

        public async Task<ResponseModel.PayrollResultDto> CalculatePayrollAsync(Guid staffId, Guid currentUserId, IList<string> currentUserRoles)
        {
            var staff = await _userRepository.GetByIdAsync(staffId);

            if (staff == null)
                throw new ArgumentException("Cannot find User");

            if (currentUserRoles.Contains("Manager"))
            {
                var currentUser = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);

                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                if (currentUser.DepartmentId == null)
                    throw new ArgumentException("Manager does not belong to any department");

                if (staff.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("You can only calculate payrolls for User in your department");
            }

            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            if (await _payrollRepository.ExistsPayrollForMonth(staffId, month, year))
                throw new InvalidOperationException("Payroll for this month already exists");

            var checkinsInMonth = await _context.Checkins
                .Where(c => c.UserId == staffId
                    && c.CheckinMorning.Year == year
                    && c.CheckinMorning.Month == month
                    && !c.IsDeleted)
                .ToListAsync();

            var schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();
            if (schedule == null)
                throw new ArgumentException("Schedule not found");

            //var overtimeDuration = 1;


            double totalSalary = 0;
            //var salaryForDay = 0.0;
            //double baseSalary = staff.BasicSalary;
            foreach (var checkin in checkinsInMonth)
            {
                // Convert checkouTime to Vietnam time (UTC+7)
                //var utcTime = DateTime.SpecifyKind(checkin.CheckoutAfternoon, DateTimeKind.Utc);
                //var vnTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                //var checkouTime = vnTime.Hour;
                //var endTime = schedule.EndTimeAfternoon.Hour;
                //if (checkouTime > endTime)
                //{
                //    overtimeDuration = checkouTime - endTime;
                //}
                ////salaryForDay = await CalculateSalaryPerDayAsync(checkin, baseSalary);
                //salaryForDay = await CalculateSalaryPerDayAsync(staffId, checkin.CheckinMorningStatus, checkin.CheckoutMorningStatus, checkin.CheckinAfternoonStatus, checkin.CheckoutAfternoonStatus, overtimeDuration);
                ////_logger.LogInformation("Calculated salary for day {Date}: {Salary}", checkin.CheckinMorning.Date, salaryForDay);
                ////_logger.LogInformation("CheckinMorningStatus: {CheckinMorningStatus}, CheckoutMorningStatus: {CheckoutMorningStatus}, CheckinAfternoonStatus: {CheckinAfternoonStatus}, CheckoutAfternoonStatus: {CheckoutAfternoonStatus}", 
                ////    checkin.CheckinMorningStatus, checkin.CheckoutMorningStatus, checkin.CheckinAfternoonStatus, checkin.CheckoutAfternoonStatus);
                ////_logger.LogInformation("Thoi gian lam them gio {endtime} - {overtime}", endTime, checkouTime);
                totalSalary += checkin.SalaryPerDay;
                _logger.LogInformation("Tien luong {money}", totalSalary);
            }

            var totalDayWorked = checkinsInMonth
                .Where(p => (p.CheckinMorningStatus != LogStatus.None && p.CheckoutMorningStatus != LogStatus.None
                            && p.CheckinMorningStatus != LogStatus.Absent && p.CheckoutMorningStatus != LogStatus.Absent)
                            || (p.CheckinAfternoonStatus != LogStatus.None && p.CheckoutAfternoonStatus != LogStatus.None
                            && p.CheckinAfternoonStatus != LogStatus.Absent && p.CheckoutAfternoonStatus != LogStatus.Absent))
                .Select(c => c.CheckinMorning.Date)
                .Distinct()
                .Count();

            var payroll = new Payroll
            {
                Id = Guid.NewGuid(),
                UserId = staffId,
                Salary = totalSalary,
                DaysWorked = totalDayWorked,
                CreatedDate = DateTime.Now,
                Note = $"Lương tháng {month}/{year}"
            };

            await _payrollRepository.CreatePayrollAsync(payroll);
            await _context.SaveChangesAsync();

            return new PayrollResultDto
            {
                Id = payroll.Id,
                Name = payroll.Users.Fullname,
                DaysWorked = totalDayWorked,
                Salary = totalSalary,
                CreatedDate = payroll.CreatedDate,
                Note = payroll.Note
            };
        }

        //public async Task<double> CalculateSalaryPerDayAsync(Guid userId, Enums.LogStatus? CheckinMorningStatus, Enums.LogStatus? CheckoutMorningStatus, Enums.LogStatus? CheckinAfternoonStatus, Enums.LogStatus? CheckoutAfternoonStatus, double overtimeDuration)
        //{
        //    var logStatus = await _logStatusConfigRepository.GetAllAsync();
        //    var user = await _userRepository.GetByIdAsync(userId);
        //    ScheduleTime schedule;
        //    schedule = await _context.ScheduleTimes.FirstOrDefaultAsync();


        //    double checkinMorningMultiply = 0;
        //    double checkoutMorningMultiply = 0;
        //    double checkinAfternoonMultiply = 0;
        //    double checkoutAfternoonMultiply = 0;

        //    foreach (var item in logStatus)
        //    {
        //        if (item.Id == (int)(CheckinMorningStatus ?? Enums.LogStatus.None))
        //        {
        //            checkinMorningMultiply = item.SalaryMultiplier;
        //            //_logger.LogInformation("CheckinMorningStatus: {status}", CheckinMorningStatus);
        //        }
        //        if (item.Id == (int)(CheckoutMorningStatus ?? Enums.LogStatus.None))
        //        {
        //            checkoutMorningMultiply = item.SalaryMultiplier;
        //            //_logger.LogInformation("CheckoutMorningStatus: {status}", CheckoutMorningStatus);
        //        }
        //        if (item.Id == (int)(CheckoutAfternoonStatus ?? Enums.LogStatus.None))
        //        {
        //            checkoutAfternoonMultiply = item.SalaryMultiplier;
        //            //_logger.LogInformation("CheckoutAfternoonStatus: {status}", CheckoutMorningStatus);
        //        }
        //        if (item.Id == (int)(CheckinAfternoonStatus ?? Enums.LogStatus.None))
        //        {
        //            checkinAfternoonMultiply = item.SalaryMultiplier;
        //            //_logger.LogInformation("CheckinAfternoonStatus: {status}", CheckinAfternoonStatus);
        //        }
        //    }

        //    double baseSalary = user.BasicSalary;
        //    double quarterSalary = baseSalary / 4.0;
        //    double salaryToday = 0;

        //    if (CheckoutAfternoonStatus == Enums.LogStatus.Overtime)
        //    {
        //        salaryToday = (quarterSalary * checkinMorningMultiply) + (quarterSalary * checkoutMorningMultiply) + (quarterSalary * checkinAfternoonMultiply) + (quarterSalary * checkoutAfternoonMultiply * overtimeDuration);
        //        //_logger.LogInformation("sang 1 {moneys} - {status}", quarterSalary * checkinMorningMultiply, checkinMorningMultiply);
        //        //_logger.LogInformation("sang 2 {moneys} - {status}", quarterSalary * checkoutMorningMultiply, checkoutMorningMultiply);
        //        //_logger.LogInformation("chieu 1 {moneys} - {status}", quarterSalary * checkinAfternoonMultiply, checkinAfternoonMultiply);
        //        //_logger.LogInformation("chieu 2 {moneys} - {status}", quarterSalary * checkoutAfternoonMultiply, checkoutAfternoonMultiply);
        //        //_logger.LogInformation("Over TIme: {OT}", overtimeDuration);
        //    }
        //    else salaryToday = (quarterSalary * checkinMorningMultiply) + (quarterSalary * checkoutMorningMultiply) + (quarterSalary * checkinAfternoonMultiply) + (quarterSalary * checkoutAfternoonMultiply);

        //    return salaryToday;
        //}
    }
}
