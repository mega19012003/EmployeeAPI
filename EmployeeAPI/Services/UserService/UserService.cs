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

        public async Task<ResponseModel.UserResultDto> UpdateStaffAsync(ResponseModel.AdminUpdateDto dto, Guid currentUserId, IList<string> currentUserRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingUser = await _userRepository.GetUserInfoAsync(dto.UserId);
                if (existingUser == null)
                    throw new ArgumentException("Cannot find user");

                var isAdmin = currentUserRole.Contains("Administrator");
                var isManager = currentUserRole.Contains("Manager");
                Guid? departmentId = null;

                if (!string.IsNullOrWhiteSpace(dto.Fullname)) existingUser.Fullname = dto.Fullname;
                if (!string.IsNullOrWhiteSpace(dto.Address)) existingUser.Address = dto.Address;
                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) existingUser.PhoneNumber = dto.PhoneNumber;
                if (dto.BasicSalary.HasValue) existingUser.BasicSalary = dto.BasicSalary.Value;
                if (dto.IsActive.HasValue) existingUser.IsActive = dto.IsActive.Value;

                if (dto.ImageUrl != null)
                {
                    if (!string.IsNullOrEmpty(existingUser.ImageUrl))
                    {
                        var oldPublicId = _cloudImageService.ExtractPublicId(existingUser.ImageUrl);
                        Console.WriteLine($"Old ImageUrl: {existingUser.ImageUrl}");
                        Console.WriteLine($"Extracted publicId: {oldPublicId}");

                        if (!string.IsNullOrEmpty(oldPublicId))
                        {
                            await _cloudImageService.DeleteImageAsync(oldPublicId);
                        }
                    }

                    var uploadedImageUrl = await _cloudImageService.UploadImageAsync(dto.ImageUrl);
                    existingUser.ImageUrl = uploadedImageUrl;
                }

                if (isManager)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser == null)
                        throw new ArgumentException("Current user not found");
                    else if (currentUser?.DepartmentId == null)
                        throw new ArgumentException("Manager does not have department, please contact admin to add department.");
                    departmentId = currentUser.DepartmentId;
                    if( existingUser.DepartmentId != departmentId)
                        throw new ArgumentException("Manager can only update users in their own department");
                }
                else if (isAdmin)
                {
                    if (dto.Role.HasValue) existingUser.Role = (RoleType)dto.Role;
                    if (dto.DepartmentId.HasValue)
                    {
                        existingUser.DepartmentId = dto.DepartmentId;

                        if (dto.PositionId.HasValue)
                        {
                            var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId.Value);
                            if (department == null)
                                throw new ArgumentException("Department not found");

                            bool isValidPosition = department.Positions.Any(p => p.Id == dto.PositionId.Value);
                            if (!isValidPosition)
                                throw new ArgumentException("Position does not belong to this department");

                            existingUser.PositionId = dto.PositionId;
                        }
                    }
                    else if (dto.PositionId.HasValue)
                    {
                        if (!existingUser.DepartmentId.HasValue)
                            throw new ArgumentException("User does not have a department");

                        var department = await _departmentRepository.GetByIdAsync(existingUser.DepartmentId.Value);
                        if (department == null)
                            throw new ArgumentException("Department not found");

                        bool isValidPosition = department.Positions.Any(p => p.Id == dto.PositionId.Value);
                        if (!isValidPosition)
                            throw new ArgumentException("Position does not belong to this department");

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
                    Fullname = existingUser.Fullname,
                    RoleName = existingUser.Role.ToString(),
                    Address = existingUser.Address,
                    PhoneNumber = existingUser.PhoneNumber,
                    BasicSalary = existingUser.BasicSalary,
                    DepartmentId = existingUser.DepartmentId,
                    DepartmentName = existingUser.Department?.Name,
                    PositionId = existingUser.PositionId,
                    PositionName = existingUser.Position?.Name,
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
                    throw new ArgumentException("Cannot find current user");

                var existingUser = await _userRepository.GetUserInfoAsync(Id);
                if (existingUser == null)
                    throw new ArgumentException("Cannot find user");

                var isAdmin = currentUserRoles.Contains("Administrator");
                var isManager = currentUserRoles.Contains("Manager");

                if (isManager && !isAdmin)
                {
                    if (!currentUser.DepartmentId.HasValue)
                        throw new ArgumentException("Manager does not belong to any department");

                    if (existingUser.DepartmentId != currentUser.DepartmentId)
                        throw new ArgumentException("Manager cannot delete user from other department");
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
        public async Task<PagedResult<ResponseModel.UserResultDto>> GetAllAsync(string? SearchTerm, Guid? positionId, Guid? departmentId, Guid currentUserId, IList<string> currentUserRoles, int? pageIndex, int? pageSize)
        {
            try
            {
                var query = _userRepository.GetAll();

                var isAdmin = currentUserRoles.Contains("Administrator");
                var isManager = currentUserRoles.Contains("Manager");

                if (isManager)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser == null || currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager doesn't belong to any department");

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
                        RoleName = f.Role.ToString(),
                        Address = f.Address,
                        PhoneNumber = f.PhoneNumber,
                        DepartmentName = f.Department != null ? f.Department.Name : string.Empty,
                        PositionName = f.Position != null ? f.Position.Name : string.Empty,
                        BasicSalary = f.BasicSalary,
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

            var results = await _userRepository.GetUserInfoAsync(id);
            if (results == null)
                throw new ArgumentException("Cannot find User");

            if (isManager) 
            {
                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Current user not found");
                else if (currentUser?.DepartmentId == null)
                    throw new ArgumentException("Manager does not belong to any department");

                if (results.DepartmentId != currentUser.DepartmentId)
                    throw new UnauthorizedAccessException("Manager can only access list users in their department");
            }

            return new ResponseModel.UserResultDto
            {
                UserId = results.UserId,
                Fullname = results.Fullname,
                RoleName = results.Role.ToString(),
                Address = results.Address,
                PhoneNumber = results.PhoneNumber,
                DepartmentName = results.Department?.Name ?? null,
                PositionName = results.Position?.Name ?? null,
                BasicSalary = results.BasicSalary,
                ImageUrl = results.ImageUrl,
            };
        }
    }
}
