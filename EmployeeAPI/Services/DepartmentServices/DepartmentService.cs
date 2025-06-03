using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Auth;
using EmployeeAPI.Repositories.Departments;
using EmployeeAPI.Services.UserService;
using Microsoft.EntityFrameworkCore;

using static EmployeeAPI.Services.DepartmentServices.ResponseModel;


namespace EmployeeAPI.Services.DepartmentServices
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repository;
        //private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(IDepartmentRepository repository, AppDbContext context, ILogger<DepartmentService> logger)
        {
            _repository = repository;
            _context = context;
            _logger = logger;
        }
        public async Task<PagedResult<ResponseModel.DepartmentDto>> GetAllAsync(string? name, int? pageIndex, int? pageSize)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _context.Departments
                    .Where(f => string.IsNullOrEmpty(name) || f.Name.ToLower().Contains(name.ToLower()))
                    .Where(p => !p.isDeleted);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(f => new ResponseModel.DepartmentDto
                    {
                        DepartmentId = f.Id,
                        Name = f.Name,
                        IsDeleted = f.isDeleted
                    }).ToListAsync();
                return new PagedResult<ResponseModel.DepartmentDto>
                {
                    Items = items,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<ResponseModel.DepartmentDto> GetByIdAsync(Guid id)
        {
            try
            {
                var departmant = await _repository.GetByIdAsync(id);

                return new DepartmentDto
                {
                    DepartmentId = departmant.Id,
                    Name = departmant.Name,
                    IsDeleted = departmant.isDeleted
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.CreateDepartment> AddAsync(string name)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if(name ==null)
                {
                    throw new ArgumentNullException(nameof(name), "Department name cannot be null or empty");
                }

                var model = new Department
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                };

                /*var entity =*/
                await _repository.AddAsync(model);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.CreateDepartment
                {
                    DepartmentId = model.Id,
                    Name = model.Name,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while adding department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.UpdateDepartment> UpdateAsync(Guid id, string newName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try {
                var result = await _repository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Cannot find department");

                result.Name = newName;

                await _repository.UpdateAsync(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new UpdateDepartment
                {
                    DepartmentId = result.Id,
                    Name = result.Name,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<string> SoftDeleteAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _repository.GetByIdAsync(id);
                if (result == null)
                    throw new ArgumentException("Cannot find department");

                result.isDeleted = true;
                await _repository.SoftDeleteAsync(result.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Đã xóa phòng ban: " + result.Name;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<PagedResult<UserFilter>> GetStaffByDepartmentAsync(Guid? departmentId, int? pageSize, int? pageIndex)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;
                var result = await _repository.GetByIdAsync(departmentId.Value);
                if (result == null)
                {
                    throw new ArgumentException("Cannot find department");
                }
                /*var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role).Value;

                var currentUser = await _context.Users.FindAsync(Guid.Parse(currentUserId));

                if (role == "Manager")
                {
                    if (departmentId.HasValue && departmentId != currentUser.DepartmentId)
                        throw new UnauthorizedAccessException("Mananager cannot use other departmentId");

                    departmentId = currentUser.DepartmentId;
                }

                else if (role == "Admin")
                {
                    if (!departmentId.HasValue)
                        throw new ArgumentException("Admi must input departmentId");
                }
                */
                var query = _context.Departments
                    .Include(d => d.Users)
                    .Where(d => !d.isDeleted && d.Id == departmentId);
                

                var allStaffs = query
                    .SelectMany(d => d.Users
                    .Where(s => s.IsActive && !s.IsDeleted));

                var totalCount = await allStaffs.CountAsync();

                var items = await allStaffs
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(st => new UserFilter
                    {
                        UserId = st.UserId,
                        Name = st.Fullname,
                        BasicSalary = st.BasicSalary,
                        ImageUrl = st.ImageUrl,
                        Department = st.Department.Name
                    })
                    .ToListAsync();

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
                _logger.LogError(ex, "Error occurred while retrieving User by department. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<PagedResult<PositionByDepartment>> GetListPositionAsync(Guid? departmentId, int? pageSize, int? pageIndex)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var result = await _repository.GetByIdAsync(departmentId.Value);
            if (result == null)
            {
                throw new ArgumentException("Cannot find department");
            }

            var query = await _repository.GetPositionsByDepartmentAsync(departmentId, pageSize, pageIndex);
            var lstPosition = query.SelectMany(d => d.Positions).ToList();

            var totalCount = query.Count();

            var items = lstPosition
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(p => new PositionByDepartment
                {
                    PositionId = p.Id,
                    PositionName = p.Name,
                    DepartmentName = p.Department.Name,
                })
                .ToList();

            return new PagedResult<PositionByDepartment>
            {
                TotalCount = totalCount,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                Items = items
            };
        }

    }
}
