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
        private readonly GoogleSheetHelper _googleSheetHelper;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ICloudImageService _cloudImageService;
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly IAuthRepository _authRepository;
        public UserService(IUserRepository userRepository, ICloudImageService cloudImageService, AppDbContext context, ILogger<AuthService> logger, IDepartmentRepository departmentRepository, GoogleSheetHelper googleSheetHelper, IAuthRepository authRepository)
        {
            _userRepository = userRepository;
            _departmentRepository = departmentRepository;
            _cloudImageService = cloudImageService;
            _context = context;
            _logger = logger;
            _googleSheetHelper = googleSheetHelper;
            _authRepository = authRepository;
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
                if (dto.IsActive.HasValue && dto.IsActive.Value == false && dto.UserId == currentUserId)
                {
                    throw new Exception("Bạn không thể tự vô hiệu hóa chính mình.");
                }
                if (!string.IsNullOrEmpty(dto.Email) && dto.Email != existingUser.Email)
                {
                    var emailExists = await _authRepository.GetUserByEmailAsync(dto.Email);
                    if (emailExists != null)
                        throw new ArgumentException("Email đã tồn tại. Vui lòng dùng Email khác");
                    existingUser.Email = dto.Email;
                }

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
                    if (existingUser.Role != RoleType.SystemAdmin && existingUser.Role != RoleType.Administrator)
                    {
                        throw new Exception("SystemAdmin chỉ có thể chỉnh sửa user có role là Admin hoặc SystemAdmin");
                    }
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

                    if (dto.IsActive.HasValue) existingUser.IsActive = dto.IsActive.Value;
                    if (dto.Role.HasValue)
                    {
                        if (dto.Role == RoleType.SystemAdmin)
                            throw new Exception("Admin không được phép gán role SystemAdmin");
                        existingUser.Role = dto.Role.Value;
                    }

                    if (currentUser.CompanyId != existingUser.CompanyId)
                        throw new Exception("Admin chỉ có thể cập nhật nhân viên cùng công ty");

                    if (existingUser.DepartmentId.HasValue)
                    {
                        departmentId = existingUser.DepartmentId;
                    }

                    if (dto.DepartmentId.HasValue)
                    {
                        var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId.Value);
                        if (department == null)
                            throw new ArgumentException("Không tìm thấy phòng ban");

                        existingUser.DepartmentId = dto.DepartmentId.Value;
                    }

                    if (dto.PositionId.HasValue)
                    {
                        Guid? checkDeptId = dto.DepartmentId ?? existingUser.DepartmentId;

                        if (!checkDeptId.HasValue)
                            throw new ArgumentException("Chưa có phòng ban để kiểm tra chức vụ");

                        var department = await _departmentRepository.GetByIdAsync(checkDeptId.Value);
                        if (department == null)
                            throw new ArgumentException("Không tìm thấy phòng ban");

                        bool isValidPosition = department.Positions.Any(p => p.Id == dto.PositionId.Value);
                        if (!isValidPosition)
                            throw new ArgumentException("Chức vụ không thuộc phòng ban");

                        existingUser.PositionId = dto.PositionId;
                    }
                }
                else if (isManager) 
                {
                    if (dto.IsActive.HasValue) existingUser.IsActive = dto.IsActive.Value;

                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban.");
                    //if (currentUser.CompanyId.HasValue)
                    //    departmentId = currentUser.DepartmentId;

                    if (existingUser.DepartmentId != existingUser.DepartmentId)
                        throw new ArgumentException("Manager chỉ có thể cập nhật user cùng phòng ban");

                    if (dto.PositionId.HasValue)
                    {
                        var department = await _departmentRepository.GetByIdAsync(existingUser.DepartmentId.Value);
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
                    Email = existingUser.Email,
                    Address = existingUser.Address,
                    PhoneNumber = existingUser.PhoneNumber,
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
                    if (!currentUser.CompanyId.HasValue)
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
                //existingUser.IsActive = false;

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
        public async Task<PagedResult<ResponseModel.UserResultDto>> GetAllAsync(string? SearchTerm, bool? IsActive, Guid? positionId, Guid? departmentId, Guid? companyId, Guid currentUserId, IList<string> currentUserRoles, int? pageIndex, int? pageSize, int? Month)
        {
            try
            {
                var query = _userRepository.GetAll();

                var isAdmin = currentUserRoles.Contains("Administrator");
                var isSystemAdmin = currentUserRoles.Contains("SystemAdmin");
                var isManager = currentUserRoles.Contains("Manager");

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    var keyword = SearchTerm.Trim().ToLower();
                    query = query.Where(u => u.Fullname.ToLower().Contains(keyword) || u.Username.ToLower().Contains(keyword));
                }
                if( IsActive.HasValue)
                    query = query.Where(u => u.IsActive == IsActive.Value);

                if (isSystemAdmin)
                {
                    if (companyId.HasValue)
                        query = query.Where(u => u.CompanyId == companyId.Value);

                    if (departmentId.HasValue)
                        query = query.Where(u => u.DepartmentId == departmentId.Value);

                    if (positionId.HasValue)
                        query = query.Where(u => u.PositionId == positionId.Value);
                }
                else if (isAdmin)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser?.CompanyId == null)
                        throw new ArgumentException("Admin chưa có công ty. Vui lòng liên hệ System Admin để cập nhật công ty.");

                    query = query.Where(u => u.CompanyId == currentUser.CompanyId.Value);

                    if (departmentId.HasValue)
                        query = query.Where(u => u.DepartmentId == departmentId.Value);

                    if (positionId.HasValue)
                        query = query.Where(u => u.PositionId == positionId.Value);
                }
                else if (isManager)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser == null || currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban.");

                    query = query.Where(u => u.DepartmentId.HasValue && u.DepartmentId.Value == currentUser.DepartmentId.Value);

                    if (positionId.HasValue)
                        query = query.Where(u => u.PositionId == positionId.Value);
                }

                // Gán giá trị mặc định nếu không có
                var now = DateTime.Now;
                int Year = now.Year;

                var users = await query.ToListAsync();
                var allDutyDetails = await _googleSheetHelper.GetAllDutyDetailsWithDutiesCachedAsync();

                // Lọc công việc hoàn thành theo tháng/năm
                var completedDutyDetails = allDutyDetails.Where(d => d.Status == Enums.DutyStatus.Completed && d.Duty != null && d.CompletedDate.Value.Year == Year);

                if (Month.HasValue)
                {
                    completedDutyDetails = completedDutyDetails.Where(d => d.CompletedDate.HasValue && d.CompletedDate.Value.Month == Month.Value);
                }

                var completedGrouped = completedDutyDetails
                    .GroupBy(d => d.UserId)
                    .ToDictionary(g => g.Key, g => g.Count());

                var inProgressGrouped = allDutyDetails
                    .Where(d => d.Status != DutyStatus.Completed)
                    .GroupBy(d => d.UserId)
                    .ToDictionary(g => g.Key, g => g.Count());

                var totalCount = users.Count;

                var items = users
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(f => new ResponseModel.UserResultDto
                    {
                        UserId = f.UserId,
                        Fullname = f.Fullname,
                        Username = f.Username,
                        RoleName = f.Role.ToString(),
                        Email = f.Email,
                        Address = f.Address,
                        PhoneNumber = f.PhoneNumber,
                        DepartmentName = f.Department?.Name ?? string.Empty,
                        DepartmentId = f.DepartmentId,
                        PositionName = f.Position?.Name ?? string.Empty,
                        PositionId = f.PositionId,
                        CompanyName = f.Company?.Name ?? string.Empty,
                        CompanyId = f.CompanyId,
                        IsActive = f.IsActive,
                        ImageUrl = f.ImageUrl,
                        CompletedDuties = completedGrouped.TryGetValue(f.UserId, out var completed) ? completed : 0,
                        InProgressDuties = inProgressGrouped.TryGetValue(f.UserId, out var inProgress) ? inProgress : 0
                    })
                    .ToList();

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
                _logger.LogError(ex, "Error occurred while retrieving all users. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<PagedResult<ResponseModel.UserResultDto>> GetActiveEmployeesAndManagersAsync(string? SearchTerm, Guid? positionId, Guid? departmentId, Guid? companyId, bool? employeeOnly, Guid currentUserId, IList<string> currentUserRoles, int? pageIndex, int? pageSize)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;
                employeeOnly ??= false;

                var query = _userRepository.GetAll().Where(u => !u.IsDeleted && u.IsActive && (u.Role == RoleType.Employee || u.Role == RoleType.Manager));

                var isAdmin = currentUserRoles.Contains("Administrator");
                var isSystemAdmin = currentUserRoles.Contains("SystemAdmin");
                var isManager = currentUserRoles.Contains("Manager");

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    var keyword = SearchTerm.Trim().ToLower();
                    query = query.Where(u => u.Fullname.ToLower().Contains(keyword) || u.Username.ToLower().Contains(keyword));
                }


                if (companyId.HasValue)
                    query = query.Where(u => u.CompanyId == companyId.Value);

                if (departmentId.HasValue)
                    query = query.Where(u => u.DepartmentId == departmentId.Value);

                if (positionId.HasValue)
                    query = query.Where(u => u.PositionId == positionId.Value);

                if (isAdmin)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser?.CompanyId == null)
                        throw new ArgumentException("Admin chưa có công ty. Vui lòng liên hệ System Admin để cập nhật công ty.");

                    query = query.Where(u => (u.Role == RoleType.Employee || u.Role == RoleType.Manager) && u.CompanyId == currentUser.CompanyId.Value);
                }
                else if (isManager)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser?.DepartmentId == null)
                        throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban.");

                    query = query.Where(u => (u.Role == RoleType.Employee || u.Role == RoleType.Manager) && u.DepartmentId == currentUser.DepartmentId.Value);
                }

                if(employeeOnly == true) query = query.Where(u => u.Role == RoleType.Employee);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(u => new ResponseModel.UserResultDto
                    {
                        UserId = u.UserId,
                        Fullname = u.Fullname,
                        Username = u.Username,
                        RoleName = u.Role.ToString(),
                        Email = u.Email,
                        Address = u.Address,
                        PhoneNumber = u.PhoneNumber,
                        DepartmentName = u.Department.Name ?? string.Empty,
                        DepartmentId = u.Department.Id,
                        PositionName = u.Position.Name ?? string.Empty,
                        PositionId = u.PositionId,
                        CompanyName = u.Company.Name ?? string.Empty,
                        CompanyId = u.CompanyId,
                        IsActive = u.IsActive,
                        ImageUrl = u.ImageUrl,
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
                _logger.LogError(ex, "Lỗi khi lấy danh sách Employee và Manager: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
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

            if (isEmployee)
            {
                id = currentUser.UserId;
            }

            var results = await _userRepository.GetUserInfoAsync(id);
            if (results == null)
                throw new ArgumentException("Không tìm thấy user");

            if (isAdmin)
            {

                if (results.CompanyId != currentUser.CompanyId)
                    throw new UnauthorizedAccessException("Admin chỉ có thể truy cập thông tin user cùng công ty");
            }
            else if (isManager)
            {

                if (results.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("Manager chỉ có thể truy cập thông tin user cùng phòng ban");
            }

            return new ResponseModel.UserResultDto
            {
                UserId = results.UserId,
                Fullname = results.Fullname,
                Username = results.Username,
                RoleName = results.Role.ToString(),
                Email = results.Email,
                Address = results.Address,
                PhoneNumber = results.PhoneNumber,
                DepartmentName = results.Department?.Name ?? string.Empty,
                DepartmentId = results.Department?.Id,
                PositionName = results.Position?.Name ?? string.Empty,
                PositionId = results.Position?.Id,
                CompanyName = results.Company?.Name ?? string.Empty,
                CompanyId = results.Company?.Id,
                ImageUrl = results.ImageUrl,
                IsActive = results.IsActive,
            };
        }
    }
}
