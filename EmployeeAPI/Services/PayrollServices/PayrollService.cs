using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.Payrolls;
using EmployeeAPI.Repositories.Users;
using EmployeeAPI.Services.CheckinServices;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static EmployeeAPI.Services.PayrollServices.ResponseModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EmployeeAPI.Services.PayrollServices
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly ICheckinRepository _checkinRepository;
        private readonly ILogger<PayrollService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        public PayrollService(IPayrollRepository payrollRepository, IUserRepository userRepository, ICheckinRepository checkinRepository, ILogger<PayrollService> logger, AppDbContext context)
        {
            _payrollRepository = payrollRepository;
            _userRepository = userRepository;
            _checkinRepository = checkinRepository;
            _logger = logger;
            _context = context;
        }

        public async Task<PagedResult<ResponseModel.PayrollDto>> GetAllPayrolls(Guid currentUserId, IList<string> currentUserRoles, string? name, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _context.Payrolls
                .Include(p => p.Users)
                .Where(p => !p.IsDeleted);

            if (currentUserRoles.Contains("Administrator"))
            {
                // Admin có thể xem tất cả payrolls → không cần lọc thêm gì
            }
            else if (currentUserRoles.Contains("Manager"))
            {
                var manager = await _context.Users.FindAsync(currentUserId);
                if (manager == null)
                    throw new ArgumentException("Manager not found");

                var departmentId = manager.DepartmentId;

                query = query.Where(p => p.Users.DepartmentId == departmentId);
            }
            else
            {
                throw new UnauthorizedAccessException("Access Denied");
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
                .Select(c => new ResponseModel.PayrollDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Name = c.Users.Fullname,
                    Salary = c.Salary,
                    DaysWorked = c.DaysWorked,
                    CreatedDate = c.CreatedDate,
                    Note = c.Note,
                    IsDeleted = c.IsDeleted,
                }).ToListAsync();

            return new PagedResult<ResponseModel.PayrollDto>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }


        //public async Task<ResponseModel.PayrollDto> GetPayrollById(Guid id)
        //{
        //    var result = await _payrollRepository.GetPayrollById(id);
        //    if (result == null)
        //    {
        //        return null;
        //    }
        //    return new PayrollDto
        //    {
        //        Id = result.Id,
        //        UserId = result.UserId,
        //        CreatedDate = result.CreatedDate,
        //        Note = result.Note,
        //    };

        //}

        /*public async Task<ResponseModel.PayrollDto> UpdatePayroll(ResponseModel.UpdatePayroll dto)
        {
            var exsistingPayroll = await _payrollRepository.GetPayrollById(dto.Id);
            if (exsistingPayroll == null)
            {
                return null;
            }
            
            exsistingPayroll.UserId = dto.UserId;
            exsistingPayroll.Note = dto.Note;

            await _payrollRepository.UpdatePayroll(exsistingPayroll);
            return new ResponseModel.PayrollDto
            {
                Id = exsistingPayroll.Id,
                UserId = exsistingPayroll.UserId,
                CreatedDate = exsistingPayroll.CreatedDate,
                Note = exsistingPayroll.Note,
            };
        }*/

        public async Task<string> SoftDeletePayroll(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            var existing = await _payrollRepository.GetPayrollById(id);
            if (existing == null)
                throw new ArgumentException("Cannot find checkin id");

            var employee = await _userRepository.GetByIdAsync(existing.UserId);
            if (employee == null)
                throw new ArgumentException("Cannot find employee for this checkin");

            var currentUser = await _userRepository.GetByIdAsync(currentUserId);
            if (currentUser == null)
                throw new ArgumentException("Cannot find current user");

            if (currentUserRoles.Contains("Administrator"))
            {

            }
            else if (currentUserRoles.Contains("Manager"))
            {
                if (currentUser.DepartmentId != employee.DepartmentId)
                    throw new UnauthorizedAccessException("Manager cannot delete payroll of an employeee from other department");
            }
            else
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            var result = await _payrollRepository.SoftDeletePayroll(id);
            if (result == null) return null;
            /*result.IsDeleted = true;
            await _payrollRepository.UpdatePayroll(result);*/
            return "Đã xóa payroll " + id;
        }

        public async Task<PagedResult<ResponseModel.PayrollDto>> GetPayrollByUser(Guid? staffId, Guid currentUserId, IList<string> currentUserRoles, int? pageIndex, int? pageSize)
        {
            try
            {
                // Gán ngầm staffId nếu user là employee
                if (!currentUserRoles.Contains("Administrator") && !currentUserRoles.Contains("Manager"))
                {
                    staffId = currentUserId;
                }
                else
                {
                    // Nếu admin hoặc manager thì staffId phải có giá trị
                    if (staffId == null || staffId == Guid.Empty)
                        throw new ArgumentException("Please input staffId");
                }

                pageIndex ??= 1;
                pageSize ??= 10;

                // Kiểm tra user được lấy có tồn tại không
                var user = await _userRepository.GetByIdAsync(staffId.Value);
                if (user == null)
                    throw new ArgumentException("Cannot find user id");

                // Manager chỉ lấy được dữ liệu trong phòng ban của mình
                if (currentUserRoles.Contains("Manager") && user.DepartmentId != (await _userRepository.GetByIdAsync(currentUserId)).DepartmentId)
                    throw new UnauthorizedAccessException("Manager cannot access payroll from other departments");

                var query = _context.Payrolls
                    .Where(p => !p.IsDeleted && p.UserId == staffId.Value)
                    .Include(p => p.Users); 

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(c => new ResponseModel.PayrollDto
                    {
                        Id = c.Id,
                        CreatedDate = c.CreatedDate,
                        UserId = c.UserId,
                        Salary = c.Salary,
                        Note = c.Note,
                        Name = c.Users.Fullname ?? "null",
                    }).ToListAsync();

                return new PagedResult<ResponseModel.PayrollDto>
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

        public async Task<PaidPayroll> CalculatePayrollAsync(Guid staffId, Guid currentUserId, IList<string> currentUserRoles)
        {
            // Lấy thông tin người dùng cần chấm công
            var staff = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == staffId && !u.IsDeleted);

            if (staff == null)
                throw new Exception("Cannot find staff id");

            // Nếu là Manager thì chỉ được chấm công cho nhân viên trong phòng ban của mình
            if (currentUserRoles.Contains("Manager"))
            {
                var currentUser = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);

                if (currentUser == null)
                    throw new Exception("Cannot find current user");

                if (staff.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("You can only calculate payrolls for employees in your department");
            }

            // Check nếu đã tồn tại bảng lương tháng này
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            if (await _payrollRepository.ExistsPayrollForMonth(staffId, month, year))
                throw new InvalidOperationException("Payroll for this month already exists");

            var configs = await _context.CheckinStatusConfigs.ToListAsync();

            double GetMultiplier(CheckinStatus status)
            {
                return configs.First(c => c.Id == (int)status).SalaryMultiplier;
            }

            var validCheckins = await _payrollRepository.CountValidCheckins(staffId, month, year);
            var lateCheckins = await _payrollRepository.CountLateCheckins(staffId, month, year);
            //var leaveEarlyCheckins = await _payrollRepository.CountLeaveEarlyCheckins(staffId, month, year);
            var absentCheckins = await _payrollRepository.CountAbsentCheckins(staffId, month, year);
            var absentPermissionCheckins = await _payrollRepository.CountAbsentPermissionCheckins(staffId, month, year);
            var overtimeCheckins = await _payrollRepository.CountOvertimeCheckins(staffId, month, year);

            var basic = staff.BasicSalary;

            double totalSalary =
                validCheckins * basic * GetMultiplier(CheckinStatus.OnTime) +
                lateCheckins * basic * GetMultiplier(CheckinStatus.Late) +
                //leaveEarlyCheckins * basic * GetMultiplier(CheckinStatus.LeaveEarly) +
                absentCheckins * basic * GetMultiplier(CheckinStatus.Absent) +
                absentPermissionCheckins * basic * GetMultiplier(CheckinStatus.LeaveWithPermission) +
                overtimeCheckins * basic * GetMultiplier(CheckinStatus.Overtime);

            var totalDayWorked = await _payrollRepository.CountDayWorked(staffId, month, year);

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

            return new PaidPayroll
            {
                Id = payroll.Id,
                UserId = staffId,
                DaysWorked = totalDayWorked,
                Salary = totalSalary,
                CreatedDate = payroll.CreatedDate,
                Note = payroll.Note
            };
        }
    }
}
