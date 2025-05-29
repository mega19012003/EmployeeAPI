using System.Reflection.Metadata.Ecma335;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Duties;
using EmployeeAPI.Services.FileServices;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Services.DutyServices
{
    public class DutyService : IDutyService
    {
        private readonly IDutyRepository _dutyRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<DutyService> _logger;
        public DutyService(IDutyRepository dutyRepository, AppDbContext context, ILogger<DutyService> logger)
        {
            _dutyRepository = dutyRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<ResponseModel.DutyDto>> GetAllAsync(Guid currentUserId, IList<string> currentUserRoles, string? name, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _context.Duties
                .Include(d => d.DutyDetails)
                    .ThenInclude(dd => dd.Users)
                .Where(d => !d.IsDeleted);

            if (currentUserRoles.Contains("Administrator"))
            {
                // Không lọc gì thêm
            }
            else if (currentUserRoles.Contains("Manager"))
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                //query = query.Where(d => d.DutyDetails.Any(dd => dd.Users.DepartmentId == currentUser.DepartmentId));
                query = query.Where(d => d.AssignedById == currentUserId); 
            }
            else
            {
                query = query.Where(d =>
                    d.DutyDetails.Any(dd => dd.UserId == currentUserId));
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                var loweredName = name.ToLower();
                query = query.Where(d => d.Name.ToLower().Contains(loweredName));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(d => d.StartDate)
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(d => new ResponseModel.DutyDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    IsCompleted = d.IsCompleted,
                    StartDate = d.StartDate,
                    AssignedById = d.AssignedById,
                    AssignedBy = d.AssignedBy.Fullname,
                    DutyDetails = d.DutyDetails.Select(dd => new ResponseModel.DutyDetailDto
                    {
                        DutyDetailId = dd.DutyDetailId,
                        userId = dd.UserId,
                        Description = dd.Description,
                        Name = dd.Users.Fullname,
                        
                    }).ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<ResponseModel.DutyDto>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }
        public async Task<ResponseModel.DutyDto> GetByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            var duty = await _context.Duties
                .Include(d => d.DutyDetails)
                .ThenInclude(dd => dd.Users)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (duty == null)
                throw new ArgumentException("Cannot find duty id");

            if (currentUserRoles.Contains("Administrator"))
            {
                // Admin: full quyền, không cần kiểm tra
            }
            else if (currentUserRoles.Contains("Manager"))
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                var sameDepartment = duty.DutyDetails.Any(dd => dd.Users.DepartmentId == currentUser.DepartmentId);
                if (!sameDepartment)
                    throw new UnauthorizedAccessException("Manager cannot access duties from other department");
            }
            else
            {
                var isInDuty = duty.DutyDetails.Any(dd => dd.UserId == currentUserId);
                if (!isInDuty)
                    throw new UnauthorizedAccessException("You do not have permission to access this duty");
            }

            return new ResponseModel.DutyDto
            {
                Id = duty.Id,
                Name = duty.Name,
                IsCompleted = duty.IsCompleted,
                StartDate = duty.StartDate,
                AssignedById = duty.AssignedById,
                AssignedBy = duty.AssignedBy?.Fullname,
                DutyDetails = duty.DutyDetails.Select(d => new ResponseModel.DutyDetailDto
                {
                    DutyDetailId = d.DutyDetailId,
                    userId = d.UserId,
                    Description = d.Description,
                    Name = d.Users?.Fullname,
                }).ToList()
            };
        }
        public async Task<ResponseModel.DutyDto> AddDutyAsync(ResponseModel.CreateDuty dto, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                if (currentUserRoles.Contains("Manager"))
                {
                    var userIdsToAssign = dto.DutyDetails.Select(d => d.userId).ToList();
                    var assignedUsers = await _context.Users
                        .Where(u => userIdsToAssign.Contains(u.UserId))
                        .ToListAsync();

                    if (assignedUsers.Count != userIdsToAssign.Count)
                        throw new Exception("Cannot asign employee form other department");

                    if (assignedUsers.Any(u => u.IsDeleted))
                        throw new Exception("Cannot assign duty to deleted users");

                    if (currentUserRoles.Contains("Manager") && assignedUsers.Any(u => u.DepartmentId != currentUser.DepartmentId))
                        throw new Exception("Manager can only assign users from the same department");

                }
                else if (currentUserRoles.Contains("Administrator"))
                {
                    var userIdsToAssign = dto.DutyDetails.Select(d => d.userId).ToList();

                    var assignedUsers = await _context.Users.Where(u => userIdsToAssign.Contains(u.UserId)).ToListAsync();

                    var anyDeletedUser = assignedUsers.Any(u => u.IsDeleted);
                    if (anyDeletedUser)
                        throw new Exception("Cannot assign duty to deleted users");
                }
                else if (!currentUserRoles.Contains("Administrator"))
                {
                    throw new UnauthorizedAccessException("You do not have permission to assign users to duty");
                }

                var duty = new Duty
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    StartDate = DateTime.Now,
                    AssignedById = currentUserId,
                    DutyDetails = dto.DutyDetails.Select(d => new DutyDetail
                    {
                        UserId = d.userId,
                        Description = d.Description,
                        IsDeleted = false
                    }).ToList()
                };

                var created = await _dutyRepository.AddAsync(duty);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await _context.Duties
                   .Include(d => d.AssignedBy)
                   .Include(d => d.DutyDetails)
                       .ThenInclude(dd => dd.Users)
                   .FirstOrDefaultAsync(d => d.Id == created.Id);

                if (result == null)
                    throw new Exception("Cannot load result info after creation");

                return new ResponseModel.DutyDto
                {
                    Id = result.Id,
                    Name = result.Name,
                    IsCompleted = result.IsCompleted,
                    StartDate = result.StartDate,
                    AssignedById = result.AssignedById,
                    AssignedBy = result.AssignedBy?.Fullname,
                    DutyDetails = result.DutyDetails.Select(d => new ResponseModel.DutyDetailDto
                    {
                        DutyDetailId = d.DutyDetailId,
                        userId = d.UserId,
                        Description = d.Description,
                        Name = d.Users?.Fullname,
                        IsDeleted = d.IsDeleted
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting checkin. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<ResponseModel.DutyDto> AddDutyDetailAsync(ResponseModel.CreateDuty dto, Guid DutyId, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                if (currentUserRoles.Contains("Manager"))
                {
                    var userIdsToAssign = dto.DutyDetails.Select(d => d.userId).ToList();

                    var assignedUsers = await _context.Users
                        .Where(u => userIdsToAssign.Contains(u.UserId))
                        .ToListAsync();

                    var anyInvalidUser = assignedUsers.Any(u => u.DepartmentId != currentUser.DepartmentId);
                    if (anyInvalidUser)
                        throw new UnauthorizedAccessException("Manager can only assign users from the same department");
                }

                else if (!currentUserRoles.Contains("Administrator"))
                {
                    throw new UnauthorizedAccessException("You do not have permission to assign users to duty");
                }

                var duty = new Duty
                {
                    Id = DutyId,
                    DutyDetails = dto.DutyDetails.Select(d => new DutyDetail
                    {
                        UserId = d.userId,
                        Description = d.Description
                    }).ToList()
                };

                var created = await _dutyRepository.AddAsync(duty);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await _context.Duties
                   .Include(d => d.AssignedBy)
                   .Include(d => d.DutyDetails)
                       .ThenInclude(dd => dd.Users)
                   .FirstOrDefaultAsync(d => d.Id == created.Id);

                if (result == null)
                    throw new Exception("Cannot load result info after creation");

                return new ResponseModel.DutyDto
                {
                    Id = result.Id,
                    Name = result.Name,
                    AssignedById = result.AssignedById,
                    AssignedBy = result.AssignedBy?.Fullname,
                    DutyDetails = result.DutyDetails.Select(d => new ResponseModel.DutyDetailDto
                    {
                        DutyDetailId = d.DutyDetailId,
                        userId = d.UserId,
                        Description = d.Description,
                        Name = d.Users?.Fullname,
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("An error occurred while adding the duty detail", ex);
            }
        }

        public async Task<ResponseModel.DutyDto> UpdateDutyAsync(ResponseModel.UpdateDuty dto, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                if (currentUserRoles.Contains("Manager"))
                {
                    var userIdsToAssign = dto.DutyDetails.Select(d => d.userId).ToList();

                    var assignedUsers = await _context.Users
                        .Where(u => userIdsToAssign.Contains(u.UserId))
                        .ToListAsync();

                    var anyInvalidUser = assignedUsers.Any(u => u.DepartmentId != currentUser.DepartmentId);
                    if (anyInvalidUser)
                        throw new UnauthorizedAccessException("Manager can only assign users from the same department");
                }

                else if (!currentUserRoles.Contains("Administrator"))
                {
                    throw new UnauthorizedAccessException("You do not have permission to assign users to duty");
                }

                var existingDuty = await _dutyRepository.GetDutyByIdAsync(dto.Id);
                if (existingDuty == null)
                    throw new ArgumentException("Duty not found");

                var existingStaff = await _context.Users
                    .Where(s => dto.DutyDetails.Any(d => d.userId == s.UserId))
                    .AsNoTracking()
                    .ToListAsync();
                if (existingStaff == null)
                    throw new ArgumentException("Staff not found");

                var existingDutyDetails = await _context.DutyDetail
                    .Where(d => dto.DutyDetails.Any(dd => dd.Id == d.DutyDetailId))
                    .AsNoTracking()
                    .ToListAsync();
                if (existingDutyDetails == null)
                    throw new ArgumentException("DutyDetail not found");

                existingDuty.Name = dto.Name;
                existingDuty.IsCompleted = dto.IsCompleted;
                //existingDuty.DutyDetails = dto.DutyDetails.Select(d => new DutyDetail
                //{
                //    DutyDetailId = d.Id,
                //    UserId = d.userId,
                //    Description = d.Description
                //}).ToList();

                var result = await _dutyRepository.UpdateDutyAsync(existingDuty);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.DutyDto
                {
                    Id = result.Id,
                    Name = dto.Name,
                    IsCompleted = dto.IsCompleted,
                    StartDate = result.StartDate,
                    DutyDetails = result.DutyDetails.Select(d => new ResponseModel.DutyDetailDto
                    {
                        DutyDetailId = d.DutyDetailId,
                        userId = d.UserId,
                        Name = existingStaff.FirstOrDefault(s => s.UserId == d.UserId)?.Fullname,
                        Description = d.Description
                    }).ToList(),
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("An error occurred while updating the duty", ex);
            }
        }
        public async Task<ResponseModel.DutyDetailDto> UpdateDutyDetailAsync(ResponseModel.UpdateDutyDetail dto, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                if (currentUserRoles.Contains("Manager"))
                {
                    var userIdsToAssign = dto.userId;

                    var assignedUsers = await _context.Users
                        .Where(u => userIdsToAssign.Equals(u.UserId))
                        .ToListAsync();

                    var anyInvalidUser = assignedUsers.Any(u => u.DepartmentId != currentUser.DepartmentId);
                    if (anyInvalidUser)
                        throw new UnauthorizedAccessException("Manager can only assign users from the same department");
                }

                else if (!currentUserRoles.Contains("Administrator"))
                {
                    throw new UnauthorizedAccessException("You do not have permission to assign users to duty");
                }

                var existingDuty = await _context.DutyDetail.FirstOrDefaultAsync(d => d.DutyDetailId == dto.Id && !d.IsDeleted);
                if (existingDuty == null)
                    throw new ArgumentException("Duty not found");

                var existingStaff = await _context.Users
                    .AsNoTracking()
                    .ToListAsync();
                if (existingStaff == null)
                    throw new ArgumentException("Staff not found");

                var existingDutyDetails = await _context.DutyDetail
                    .AsNoTracking()
                    .ToListAsync();
                if (existingDutyDetails == null)
                    throw new ArgumentException("DutyDetail not found");

                existingDuty.DutyDetailId = dto.Id;
                existingDuty.UserId = dto.userId;
                existingDuty.Description = dto.Description;

                var result = await _dutyRepository.UpdateDutyDetailAsync(existingDuty);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.DutyDetailDto
                {
                    DutyDetailId = result.DutyDetailId,
                    userId = result.UserId,
                    Name = existingStaff.FirstOrDefault(s => s.UserId == result.UserId)?.Fullname,
                    Description = result.Description
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("An error occurred while updating the duty detail", ex);
            }
        }

        public async Task<string> SoftDeleteDutyAsync(Guid Id)
        {
            var entity = await _context.Duties
                .Include(d => d.DutyDetails) 
                .FirstOrDefaultAsync(p => p.Id == Id && !p.IsDeleted);

            if (entity == null)
                throw new ArgumentException("Cannot find duty id");

            entity.IsDeleted = true;

            foreach (var detail in entity.DutyDetails)
            {
                detail.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return "Đã xóa công việc " + entity.Name;
        }
        public async Task<string> SoftDeleteDutyDetailAsync(Guid Id)
        {
            var entity = await _dutyRepository.SoftDeleteDutyDetailAsync(Id);
            if (entity == null)
                throw new ArgumentException("Cannot find duty detail id");

            return "Đã xóa chi tiết công việc" + entity.DutyDetailId;
        }
    }
}
