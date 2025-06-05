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

            if (currentUserRoles.Contains("Manager"))
            {
                var manager = await _context.Users.FindAsync(currentUserId);
                if (manager == null)
                    throw new ArgumentException("Manager not found");

                if (manager.DepartmentId == null)
                    throw new Exception("Manager does not belong to any department");

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
                        throw new Exception("Manager does not belong to any department");
                }

                var result = await _payrollRepository.SoftDeletePayroll(id);
                if (result == null) return null;
                result.IsDeleted = true;

                await _payrollRepository.SoftDeletePayroll(result.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Đã xóa payroll " + id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting payroll. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                await transaction.RollbackAsync();
                throw;
            }
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
                else if (currentUserRoles.Contains("Manager"))
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                    if (currentUser == null)
                        throw new ArgumentException("User not found");

                    if (currentUser.DepartmentId == null)
                        throw new Exception("Manager does not belong to any department");


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
                        throw new ArgumentException("Please input staffId");
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
            //var staff = await _context.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.UserId == staffId && (u.IsDeleted == false && u.IsActive == true)); //fix this
            var staff = await _userRepository.GetByIdAsync(staffId);

            if (staff == null)
                throw new Exception("Cannot find User");

            // Kiểm tra quyền Manager
            if (currentUserRoles.Contains("Manager"))
            {
                var currentUser = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);

                if (currentUser == null)
                    throw new Exception("Cannot find current user");

                if (currentUser.DepartmentId == null)
                    throw new Exception("Manager does not belong to any department");

                if (staff.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("You can only calculate payrolls for User in your department");
            }

            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            if (await _payrollRepository.ExistsPayrollForMonth(staffId, month, year))
                throw new InvalidOperationException("Payroll for this month already exists");

            // Lấy tất cả checkin trong tháng (đã được xác nhận hợp lệ)
            var checkinsInMonth = await _context.Checkins
                .Where(c => c.UserId == staffId
                    && c.CheckinDate.Year == year
                    && c.CheckinDate.Month == month
                    && !c.IsDeleted
                    && c.SalaryPerDay > 0)
                .ToListAsync();

            // Tổng lương = tổng SalaryPerDay của các ngày checkin
            double totalSalary = checkinsInMonth.Sum(c => c.SalaryPerDay);

            // Tính tổng ngày làm việc dựa vào số checkin hợp lệ
            var totalDayWorked = checkinsInMonth.Select(c => c.CheckinDate.Date).Distinct().Count();

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
