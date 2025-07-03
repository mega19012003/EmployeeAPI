using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.PositionServices.ResponseModel;

namespace EmployeeAPI.Services.PositionServices
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly ILogger<PositionService> _logger;

        public PositionService( IPositionRepository PositionRepository, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, AppDbContext context, ILogger<PositionService> logger)
        {
            _positionRepository = PositionRepository;
            _userRepository = userRepository;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<PagedResult<PositionDTO>> GetAllAsync(string? name, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRole)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;
                var isManager = currentUserRole.Contains("Manager");
                var isAdmin = currentUserRole.Contains("Administrator");
                Guid? departmentId = null;

                if (isManager)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser == null)
                        throw new ArgumentException("Current user not found");
                    else if (currentUser == null || currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager does not have department, Please contact admin to add Department");
                    departmentId = currentUser.DepartmentId;
                }

                var query = _positionRepository.GetQueryable(); 

                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(f => f.Name.ToLower().Contains(name.ToLower()));
                }

                if (departmentId.HasValue)
                {
                    query = query.Where(f => f.DepartmentId == departmentId.Value);
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(f => new PositionDTO
                    {
                        Id = f.Id,
                        Name = f.Name,
                        DepartmentName = f.Department.Name,
                    })
                    .ToListAsync();

                return new PagedResult<PositionDTO>
                {
                    Items = items,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ResponseModel.PositionDTO> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRole)
        {
            try
            {
                var isManager = currentUserRole.Contains("Manager");
                var isAdmin = currentUserRole.Contains("Administrator");
                Guid? departmentId = null;

                var position = await _positionRepository.GetByIdAsync(id);
                if (position == null) throw new ArgumentException("Position not found");

                if (isManager)
                {
                    var currentUser = await _userRepository.GetUserInfoAsync(currentUserId);
                    if (currentUser == null)
                        throw new ArgumentException("Current user not found");
                    if (currentUser == null || currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager does not have department, Please contact admin to add Department");

                    departmentId = currentUser.DepartmentId;

                    if (position.DepartmentId != departmentId)
                        throw new UnauthorizedAccessException("Manager can only view positions in their department.");
                }

                return new ResponseModel.PositionDTO
                {
                    Id = position.Id,
                    Name = position.Name,
                    DepartmentName = position.Department.Name,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving position. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.PositionDTO> AddAsync(ResponseModel.CreatePositionDto dto, Guid currentUserId, IList<string> currentUserRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                    throw new ArgumentException("Position name cannot be null or empty");

                Guid departmentId;

                var isManager = currentUserRole.Contains("Manager");
                var isAdmin = currentUserRole.Contains("Administrator");

                if (isManager)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser == null)
                        throw new ArgumentException("Current user not found");
                    else if (currentUser?.DepartmentId == null)
                        throw new ArgumentException("Manager does not belong to any department. Please contact admin to add department id.");

                    departmentId = currentUser.DepartmentId.Value;
                }
                else if (isAdmin)
                {
                    if (!dto.DepartmentId.HasValue)
                        throw new ArgumentException("Admin must provide a department ID.");

                    departmentId = dto.DepartmentId.Value;
                }
                else
                {
                    throw new UnauthorizedAccessException("Only administrators or managers can add a position.");
                }

                var model = new Position
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    DepartmentId = departmentId,
                };

                await _positionRepository.AddAsync(model);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await _positionRepository.GetByIdAsync(model.Id);

                return new ResponseModel.PositionDTO
                {
                    Id = model.Id,
                    Name = model.Name,
                    DepartmentName = model.Department.Name,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("An error occurred while adding User. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.PositionDTO?> UpdateAsync(Guid id, string newName, Guid currentUserId, IList<string> currentUserRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //var isAdmin = roles.Contains("Admin");
                var isManager = currentUserRole.Contains("Manager");

                var result = await _positionRepository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Position not found");

                if (isManager)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser?.DepartmentId == null)
                        throw new ArgumentException("Manager does not belong to any department");

                    if (result.DepartmentId != currentUser.DepartmentId)
                        throw new UnauthorizedAccessException("Manager can only update positions in their department.");
                }

                result.Name = newName;

                await _positionRepository.UpdateAsync(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.PositionDTO
                {
                    Id = result.Id,
                    Name = result.Name,
                    DepartmentName = result.Department.Name,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("An error occurred while updating Position. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<string> SoftDeleteAsync(Guid id, Guid currentUserId, IList<string> currentUserRole)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var isManager = currentUserRole.Contains("Manager");

                var result = await _positionRepository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Cannot find position");

                if(isManager)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser == null)
                        throw new ArgumentException("Current user not found");
                    else if (currentUser?.DepartmentId == null)
                        throw new ArgumentException("Manager does not belong to any department");

                    if (result.DepartmentId != currentUser.DepartmentId)
                        throw new UnauthorizedAccessException("Manager can only delete positions in their department.");
                }

                result.IsDeleted = true;
                await _positionRepository.UpdateAsync(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Position " + result.Name + " deleted";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("An error occurred while deleting User. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<PagedResult<UserFilterDto>> GetStaffByPositionAsync(Guid positionId, int? pageSize, int? pageIndex, Guid currentUserId, IList<string> currentUserRole)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;
                Guid? departmentId = null;
                var query = await _positionRepository.GetStaffByPositionAsync(positionId, pageSize, pageIndex);
                
                var isManager = currentUserRole.Contains("Manager");
                if (isManager)
                {
                    var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                    if (currentUser == null)
                        throw new ArgumentException("Current user not found");
                    else if (currentUser?.DepartmentId == null)
                        throw new ArgumentException("Manager does not have department, please contact admin to add department.");
                    departmentId = currentUser.DepartmentId;
                }

                var result = await _positionRepository.GetByIdAsync(positionId);
                if (result == null)
                    throw new ArgumentException("Cannot find the Position");

                var allUsers = query
                    .SelectMany(d => d.Users
                    .Where(s => !s.IsDeleted && (!departmentId.HasValue || s.DepartmentId == departmentId.Value)));

                var totalCount = allUsers.Count();

                var items = allUsers
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(st => new UserFilterDto
                    {
                        UserId = st.UserId,
                        Name = st.Fullname,
                        Position = st.Position.Name,
                        BasicSalary = st.BasicSalary,
                        ImageUrl = st.ImageUrl,
                    })
                    .ToList();

                return new PagedResult<UserFilterDto>
                {
                    TotalCount = totalCount,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving User by position. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
    }
}
