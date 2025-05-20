using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Repositories.Staffs;
using EmployeeAPI.Services.PositionServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.PositionServices.ResponseModel;
using static EmployeeAPI.Services.StaffServices.ResponseModel;

namespace EmployeeAPI.Services.PositionServices
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<PositionService> _logger;

        public PositionService(IPositionRepository repository, AppDbContext context, ILogger<PositionService> logger)
        {
            _positionRepository = repository;
            _context = context;
            _logger = logger;
        }

        /*public async Task<IEnumerable<ResponseModel.PositionDTO>> GetAllAsync(string? SearchTerm, int? pageIndex, int? pageSize)
        {
            if (pageSize == null || pageSize <= 0)
            {
                pageSize = 10;
            }
            if (pageIndex == null || pageIndex <= 0)
            {
                pageIndex = 1;
            }
            var positions = await _positionRepository.GetAllAsync(SearchTerm, pageIndex, pageSize);
            return positions.Select(p => new ResponseModel.PositionDTO
            {
                Id = p.Id,
                Name = p.Name,
                IsDeleted = p.IsDeleted
            });
        }*/
        public async Task<PagedResult<ResponseModel.PositionDTO>> GetAllAsync(string? name, int? pageIndex, int? pageSize)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _context.Positions
                    .Where(f => string.IsNullOrEmpty(name) || f.Name.ToLower().Contains(name.ToLower()))
                    .Where(p => !p.IsDeleted);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(f => new ResponseModel.PositionDTO
                    {
                        Id = f.Id,
                        Name = f.Name,
                        IsDeleted = f.IsDeleted
                    }).ToListAsync();

                return new PagedResult<ResponseModel.PositionDTO>
                {
                    Items = items,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving positions. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
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
                _logger.LogError("An error occurred while retrieving position by ID. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.CreateAndUpdatePosition> AddAsync(string name)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var query = await _positionRepository.GetAllAsync(name, null, null);
                var model = new Position
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                };

                var entity = await _positionRepository.AddAsync(model);

                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();

                return new ResponseModel.CreateAndUpdatePosition
                {
                    PositionId = entity.Id,
                    Name = entity.Name,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("An error occurred while adding staff. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ResponseModel.CreateAndUpdatePosition?> UpdateAsync(Guid id, string newName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = await _positionRepository.GetByIdAsync(id);
                if (entity == null) return null;

                entity.Name = newName;
                var updated = await _positionRepository.UpdateAsync(entity);
                if (updated == null) return null;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ResponseModel.CreateAndUpdatePosition
                {
                    PositionId = updated.Id,
                    Name = updated.Name,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); 
                _logger.LogError("An error occurred while updating staff. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<string> SoftDeleteAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _positionRepository.GetByIdAsync(id);
                if (result == null) return "Không tìm thấy vị trí";

                result.IsDeleted = true;

                await _positionRepository.SoftDeleteAsync(id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Đã xóa vị trí: " + result.Name;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("An error occurred while deleting staff. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        /*public async Task<ResponseModel.PositionDTO?> GetAllEmployee(string name)
        {
            try
            {
                var entity = await _positionRepository.GetAllEmployee(name);
                if (entity == null) return null;

                return new ResponseModel.PositionDTO
                {
                    Id = entity.Id,
                    Name = entity.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving all employees. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }*/

        public async Task<PagedResult<StaffFilter>> GetStaffByPositionAsync(string positionName, int? pageSize, int? pageIndex)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _context.Positions
                    .Include(d => d.Staffs)
                    .Where(d => !d.IsDeleted);

                if (!string.IsNullOrEmpty(positionName))
                {
                    query = query.Where(d => d.Name.ToLower().Contains(positionName.ToLower()));
                }

                var allStaffs = query
                    .SelectMany(d => d.Staffs
                        .Where(s => s.IsActive && !s.IsDeleted));

                var totalCount = await allStaffs.CountAsync();

                var items = await allStaffs
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(st => new StaffFilter
                    {
                        StaffId = st.Id,
                        Name = st.Name,
                        BasicSalary = st.BasicSalary,
                        ImageUrl = st.ImageUrl,
                    })
                    .ToListAsync();

                return new PagedResult<StaffFilter>
                {
                    TotalCount = totalCount,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving staff by position. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
    }
}
