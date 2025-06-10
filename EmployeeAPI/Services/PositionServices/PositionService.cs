using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Repositories.Positions;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.PositionServices.ResponseModel;

namespace EmployeeAPI.Services.PositionServices
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly ILogger<PositionService> _logger;

        public PositionService( IPositionRepository PositionRepository, IHttpContextAccessor httpContextAccessor, AppDbContext context, ILogger<PositionService> logger)
        {
            _positionRepository = PositionRepository;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<PagedResult<PositionDTO>> GetAllAsync(string? name, Guid? departmentId,int? pageIndex, int? pageSize)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

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
                        IsDeleted = f.IsDeleted,
                        Department = f.Department.Name,
                        /*CreatedAt = f.CreatedAt,
                        CreatedBy = f.CreatedBy,
                        UpdatedAt = f.UpdatedAt,
                        UpdatedBy = f.UpdatedBy*/
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

        public async Task<ResponseModel.PositionDTO> GetByIdAsync(Guid id)
        {
            try
            {
                var position = await _positionRepository.GetByIdAsync(id);
                if (position == null) return null;

                return new ResponseModel.PositionDTO
                {
                    Id = position.Id,
                    Name = position.Name,
                    IsDeleted = position.IsDeleted
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving position. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.PositionDTO> AddAsync(ResponseModel.CreatePosition dto, ClaimsPrincipal claim)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //var currentUserFullName = claim.FindFirstValue("Fullname");
                if (dto.Name == null)
                    throw new ArgumentException("Position name cannot be null or empty");

                var userRole = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
                var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userIdClaim, out var userId))
                    throw new UnauthorizedAccessException("Invalid user");

                Guid departmentId;

                if (userRole == "Manager")
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null || user.DepartmentId == null)
                        throw new Exception("Manager does not have department, Please contact admin to add Department");

                    departmentId = user.DepartmentId.Value;
                }
                else
                {
                    if (dto.DepartmentId == null)
                        throw new ArgumentException("Admin must input department");
                    departmentId = dto.DepartmentId.Value;
                }

                var model = new Position
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    DepartmentId = departmentId,
                    /*CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserFullName,
                    UpdatedAt = DateTime.MinValue,
                    UpdatedBy = string.Empty,*/
                };

                await _positionRepository.AddAsync(model);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                //var result = await _context.Positions.Include(p => p.Department).FirstOrDefaultAsync(p => p.Id == entity.Id);
                var result = await _positionRepository.GetByIdAsync(model.Id);

                return new ResponseModel.PositionDTO
                {
                    Id = model.Id,
                    Name = model.Name,
                    Department = model.Department.Name,
                    /*UpdatedAt = model.UpdatedAt,
                    UpdatedBy = model.UpdatedBy,
                    CreatedAt = model.CreatedAt,
                    CreatedBy = model.CreatedBy,*/
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("An error occurred while adding User. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.UpdatePosition?> UpdateAsync(Guid id, string newName, ClaimsPrincipal claim)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //////var currentUserFullName = claim.FindFirstValue("Fullname") ?? null;
                var result = await _positionRepository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Cannot find position");

               /* result.UpdatedAt = DateTime.UtcNow;
                result.UpdatedBy = currentUserFullName;*/
                result.Name = newName;
                await _positionRepository.UpdateAsync(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.UpdatePosition
                {
                    PositionId = result.Id,
                    Name = result.Name,
                    /*CreatedAt = result.CreatedAt,
                    CreatedBy = result.CreatedBy,
                    UpdatedAt = result.UpdatedAt,
                    UpdatedBy = result.UpdatedBy*/
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); 
                _logger.LogError("An error occurred while updating User. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<string> SoftDeleteAsync(Guid id, ClaimsPrincipal claim)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                ////var currentUserFullName = claim.FindFirstValue("Fullname") ?? null;
                var result = await _positionRepository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Cannot find position");

                /*result.UpdatedAt = DateTime.UtcNow;
                result.UpdatedBy = currentUserFullName;*/
                result.IsDeleted = true;
                await _positionRepository.UpdateAsync(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Đã xóa position: " + result.Name;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("An error occurred while deleting User. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<PagedResult<UserFilter>> GetStaffByPositionAsync(Guid? departmentId, Guid positionId, int? pageSize, int? pageIndex)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = await _positionRepository.GetStaffByPositionAsync(positionId, pageSize, pageIndex);

                var result = await _positionRepository.GetByIdAsync(positionId);
                if (result == null)
                    throw new ArgumentException("Cannot find the Position");

                var allUsers = query
                    .SelectMany(d => d.Users
                    .Where(s => s.IsActive && !s.IsDeleted && (!departmentId.HasValue || s.DepartmentId == departmentId.Value)));

                var totalCount = allUsers.Count();

                var items = allUsers
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(st => new UserFilter
                    {
                        UserId = st.UserId,
                        Name = st.Fullname,
                        Position = st.Position.Name,
                        BasicSalary = st.BasicSalary,
                        ImageUrl = st.ImageUrl,
                        /*CreatedBy = st.CreatedBy,
                        CreatedAt = st.CreatedAt,
                        UpdatedBy = st.UpdatedBy,
                        UpdatedAt = st.UpdatedAt*/
                    })
                    .ToList();

                return new PagedResult<UserFilter>
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
