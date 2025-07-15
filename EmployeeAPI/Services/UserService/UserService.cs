using CloudinaryDotNet.Actions;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Users;
using EmployeeAPI.Services.AuthServices;
using EmployeeAPI.Services.ImageServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static EmployeeAPI.Services.UserService.ResponseModel;

namespace EmployeeAPI.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ICloudImageService _cloudImageService;
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;
        public UserService(IUserRepository userRepository, ICloudImageService cloudImageService, AppDbContext context, ILogger<AuthService> logger, IDepartmentRepository departmentRepository)
        {
            _userRepository = userRepository;
            _departmentRepository = departmentRepository;
            _cloudImageService = cloudImageService;
            _context = context;
            _logger = logger;
        }

        public async Task<ResponseModel.UserResultDto> UpdateStaffAsync(ResponseModel.UpdateDto dto, Guid currentUserId, IList<string> currentUserRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingUser = await _userRepository.GetUserInfoAsync(dto.UserId);
                if (existingUser == null)
                    throw new ArgumentException("Không tìm thấy user");

                var isAdmin = currentUserRole.Contains("Administrator");
                var isSystemAdmin = currentUserRole.Contains("SystemAdmin");
                var isManager = currentUserRole.Contains("Manager");
                var isEmployee = currentUserRole.Contains("Employee");

                Guid? departmentId = Guid.Empty;

                if (!string.IsNullOrWhiteSpace(dto.Fullname)) existingUser.Fullname = dto.Fullname;
                if (!string.IsNullOrWhiteSpace(dto.Address)) existingUser.Address = dto.Address;
                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) existingUser.PhoneNumber = dto.PhoneNumber;

                if (dto.ImageUrl != null)
                {
                    if (!string.IsNullOrEmpty(existingUser.ImageUrl))
                    {
                        var oldPublicId = _cloudImageService.ExtractPublicId(existingUser.ImageUrl);
                        if (!string.IsNullOrEmpty(oldPublicId))
                        {
                            await _cloudImageService.DeleteImageAsync(oldPublicId);
                        }
                    }

                    var uploadedImageUrl = await _cloudImageService.UploadImageAsync(dto.ImageUrl);
                    existingUser.ImageUrl = uploadedImageUrl;
                }

                if (isSystemAdmin) // sửa dc tt cơ bản và companyId
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if(dto.CompanyId.HasValue) existingUser.CompanyId = dto.CompanyId.Value;
                    if (dto.IsActive.HasValue) existingUser.IsActive = dto.IsActive.Value;
                    if (dto.Role.HasValue)
                    {
                        if (dto.Role == RoleType.SystemAdmin || dto.Role == RoleType.Administrator)
                            existingUser.Role = dto.Role.Value;
                        else
                            throw new Exception("Chỉ có thể cập nhật role của user sang Admin hoặc SystemAdmin");
                    }
                }
                else if (isAdmin) // sửa dc tt cơ bản, departmentId và positionId
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (isAdmin && currentUser.CompanyId == null)
                        throw new Exception("Bạn chưa có công ty. Vui lòng liên hệ người quản trị hệ thống để cập nhật công ty.");

                    if (dto.SalaryPerHour.HasValue) existingUser.SalaryPerHour = dto.SalaryPerHour.Value;
                    if (dto.IsActive.HasValue) existingUser.IsActive = dto.IsActive.Value;
                    if (dto.Role.HasValue && (dto.Role == RoleType.Manager || dto.Role == RoleType.Employee)) existingUser.Role = (RoleType)dto.Role;
                    else throw new Exception("Chỉ có thể cập nhật role của user sang Manager hoặc Employee");

                    if (currentUser.CompanyId != existingUser.CompanyId)
                        throw new Exception("Admin chỉ có thể cập nhật nhân viên cùng công ty");

                    if (existingUser.DepartmentId.HasValue)
                    {
                        departmentId = existingUser.DepartmentId;
                    }

                    if (dto.DepartmentId.HasValue)
                    {
                        if (dto.PositionId.HasValue)
                        {
                            var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId.Value);
                            if (department == null)
                                throw new ArgumentException("Không tìm thấy phòng ban");

                            bool isValidPosition = department.Positions.Any(p => p.Id == dto.PositionId.Value);
                            if (!isValidPosition)
                                throw new ArgumentException("Chức vụ này không thuộc phòng ban");

                            existingUser.PositionId = dto.PositionId;
                            existingUser.DepartmentId = dto.DepartmentId;////////////////////////////////
                        }
                    }
                    else if (dto.PositionId.HasValue)
                    {
                        if (!existingUser.DepartmentId.HasValue)
                            throw new ArgumentException("User chưa có phòng ban");

                        var department = await _departmentRepository.GetByIdAsync(existingUser.DepartmentId.Value);
                        if (department == null)
                            throw new ArgumentException("Không tìm thấy phòng ban");

                        bool isValidPosition = department.Positions.Any(p => p.Id == dto.PositionId.Value);
                        if (!isValidPosition)
                            throw new ArgumentException("Chức vụ này không thuộc phòng ban");

                        existingUser.PositionId = dto.PositionId;
                    }
                }
                else if (isManager) // sửa dc tt cơ bản và positionId
                {
                    if (dto.SalaryPerHour.HasValue) existingUser.SalaryPerHour = dto.SalaryPerHour.Value;
                    if (dto.IsActive.HasValue) existingUser.IsActive = dto.IsActive.Value;

                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban.");
                    //if (currentUser.CompanyId.HasValue)
                    //    departmentId = currentUser.DepartmentId;

                    if (existingUser.DepartmentId != existingUser.CompanyId)
                        throw new ArgumentException("Manager chỉ có thể cập nhật user cùng phòng ban");

                    if (dto.PositionId.HasValue)
                    {
                        var department = await _departmentRepository.GetByIdAsync(departmentId.Value);
                        if (department == null)
                            throw new ArgumentException("Không tìm thấy phòng ban");

                        bool isValidPosition = department.Positions.Any(p => p.Id == dto.PositionId.Value);
                        if (!isValidPosition)
                            throw new ArgumentException("Chức vụ này không thuộc phòng ban");

                        existingUser.PositionId = dto.PositionId;
                    }
                }

                await _userRepository.UpdateAsync(existingUser);
                await _context.SaveChangesAsync();

                await _context.Entry(existingUser).Reference(u => u.Department).LoadAsync();
                await _context.Entry(existingUser).Reference(u => u.Position).LoadAsync();

                await transaction.CommitAsync();

                return new ResponseModel.UserResultDto
                {
                    UserId = existingUser.UserId,
                    Username = existingUser.Username,
                    Fullname = existingUser.Fullname,
                    RoleName = existingUser.Role.ToString(),
                    Address = existingUser.Address,
                    PhoneNumber = existingUser.PhoneNumber,
                    SalaryPerHour = existingUser.SalaryPerHour,
                    DepartmentId = existingUser.DepartmentId,
                    DepartmentName = existingUser.Department?.Name,
                    PositionId = existingUser.PositionId,
                    PositionName = existingUser.Position?.Name,
                    CompanyId = existingUser.CompanyId,
                    CompanyName = existingUser.Company?.Name,
                    ImageUrl = existingUser.ImageUrl,
                    IsActive = existingUser.IsActive
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi cập nhật nhân viên: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<string> SoftDeleteAsync(Guid Id, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                var existingUser = await _userRepository.GetUserInfoAsync(Id);
                if (existingUser == null)
                    throw new ArgumentException("Không tìm thấy user");

                if (Id == currentUserId)
                    throw new ArgumentException("Không thể xóa chính mình.");

                var isAdmin = currentUserRoles.Contains("Administrator");
                var isSystemAdmin = currentUserRoles.Contains("SystemAdmin");
                var isManager = currentUserRoles.Contains("Manager");

                if (isSystemAdmin)
                {
                    if (existingUser.Role != RoleType.SystemAdmin && existingUser.Role != RoleType.Administrator)
                        throw new ArgumentException("SystemAdmin chỉ có thể xóa user SystemAdmin hoặc Admin");
                }
                else if (isAdmin)
                {
                    if (!currentUser.DepartmentId.HasValue)
                        throw new ArgumentException("Bạn chưa có công ty. Vui lòng liên hệ người quản trị hệ thống để cập nhật công ty.");

                    if (existingUser.CompanyId != currentUser.CompanyId)
                        throw new ArgumentException("Admin không thể xóa user khác công ty");
                }
                else if (isManager)
                {
                    if (!currentUser.DepartmentId.HasValue)
                        throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                    if (existingUser.DepartmentId != currentUser.DepartmentId)
                        throw new ArgumentException("Manager không thể xóa user khác phòng ban");
                }

                existingUser.IsDeleted = true;
                existingUser.IsActive = false;

                await _userRepository.UpdateAsync(existingUser);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Đã xóa user: " + existingUser.Fullname;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting User. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
       
        public async Task<PagedResult<ResponseModel.UserResultDto>> GetAllAsync(string? SearchTerm, Guid? positionId, Guid? departmentId, Guid? companyId, Guid currentUserId, IList<string> currentUserRoles, int? pageIndex, int? pageSize)
        {
            try
            {
                var query = _userRepository.GetAll();

                var isAdmin = currentUserRoles.Contains("Administrator");
                var isSystemAdmin = currentUserRoles.Contains("SystemAdmin");
                var isManager = currentUserRoles.Contains("Manager");

                if (isManager)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser == null || currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban.");

                    var managerDeptId = currentUser.DepartmentId.Value;

                    query = query.Where(u => u.DepartmentId.HasValue && u.DepartmentId.Value == managerDeptId);
                }

                if (isAdmin && departmentId.HasValue)
                {
                    query = query.Where(u => u.DepartmentId == departmentId.Value);
                }

                if (positionId.HasValue)
                {
                    query = query.Where(u => u.PositionId == positionId.Value);
                }

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query = query.Where(u => u.Fullname.ToLower().Contains(SearchTerm.ToLower()));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(f => new ResponseModel.UserResultDto
                    {
                        UserId = f.UserId,
                        Fullname = f.Fullname,
                        Username = f.Username,
                        RoleName = f.Role.ToString(),
                        Address = f.Address,
                        PhoneNumber = f.PhoneNumber,
                        DepartmentName = f.Department.Name ?? string.Empty,
                        DepartmentId = f.Department.Id,
                        PositionName = f.Position.Name ?? string.Empty,
                        PositionId = f.PositionId,
                        CompanyName = f.Company.Name ?? string.Empty,
                        CompanyId = f.CompanyId,
                        IsActive = f.IsActive,
                        SalaryPerHour = f.SalaryPerHour,
                        ImageUrl = f.ImageUrl,
                    })
                    .ToListAsync();

                return new PagedResult<ResponseModel.UserResultDto>
                {
                    TotalCount = totalCount,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all employee. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;  
            }
        }

        public async Task<ResponseModel.UserResultDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            var isAdmin = currentUserRoles.Contains("Administrator");
            var isManager = currentUserRoles.Contains("Manager");
            var isEmployee = currentUserRoles.Contains("Employee");

            var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
            if (currentUser == null)
                throw new ArgumentException("Không tìm thấy user hiện tại");

            if (id == Guid.Empty)
            {
                id = currentUser.UserId;
            }

            if (isEmployee)
            {
                id = currentUser.UserId;
            }

            var results = await _userRepository.GetUserInfoAsync(id);
            if (results == null)
                throw new ArgumentException("Không tìm thấy user");

            if (isAdmin)
            {
                if (currentUser.CompanyId == null)
                    throw new ArgumentException("Admin chưa có công ty. Vui lòng liên hệ SystemAdmin để cập nhật công ty");

                if (results.CompanyId != currentUser.CompanyId)
                    throw new UnauthorizedAccessException("Admin chỉ có thể truy cập thông tin user cùng công ty");
            }
            else if (isManager)
            {
                if (currentUser.DepartmentId == null)
                    throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                if (results.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("Manager chỉ có thể truy cập thông tin user cùng phòng ban");
            }

            return new ResponseModel.UserResultDto
            {
                UserId = results.UserId,
                Fullname = results.Fullname,
                Username = results.Username,
                RoleName = results.Role.ToString(),
                Address = results.Address,
                PhoneNumber = results.PhoneNumber,
                DepartmentName = results.Department.Name ?? string.Empty,
                DepartmentId = results.Department.Id,
                PositionName = results.Position.Name ?? string.Empty,
                PositionId = results.PositionId,
                CompanyName = results.Company.Name ?? string.Empty,
                CompanyId = results.CompanyId,
                SalaryPerHour = results.SalaryPerHour,
                ImageUrl = results.ImageUrl,
            };
        }
    }
}
