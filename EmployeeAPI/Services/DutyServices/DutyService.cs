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

            var query = _context.Duties.Include(d => d.DutyDetails).ThenInclude(dd => dd.Users).Where(d => !d.IsDeleted);

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
            else if (currentUserRoles.Contains("Employee"))
            {
                query = query.Where(d => d.DutyDetails.Any(dd => dd.UserId == currentUserId));
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
                    IsDeleted = d.IsDeleted,
                    StartDate = d.StartDate,
                    AssignedById = d.AssignedById,
                    AssignedBy = d.AssignedBy.Fullname,
                    DutyDetails = d.DutyDetails.Where(dd => !dd.IsDeleted).Select(dd => new ResponseModel.DutyDetailDto
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
                _logger.LogError(ex, "Error occurred while adding duty. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<ResponseModel.DutyDto> AddDutyDetailAsync(ResponseModel.GetDutyDto dto, Guid DutyId, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null) throw new ArgumentException("Cannot find current user");

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
                    if (anyDeletedUser) throw new Exception("Cannot assign duty to deleted users");
                }
                else
                {
                    throw new UnauthorizedAccessException("You do not have permission to assign users to duty");
                }

                // Lấy duty hiện có từ database
                var duty = await _context.Duties
                    .Include(d => d.DutyDetails)
                    .FirstOrDefaultAsync(d => d.Id == DutyId);

                if (duty == null)
                    throw new Exception("Duty not found");

                // Thêm DutyDetail mới, tránh thêm trùng UserId
                var existingUserIds = duty.DutyDetails.Select(dd => dd.UserId).ToHashSet();
                foreach (var detailDto in dto.DutyDetails)
                {
                    if (!existingUserIds.Contains(detailDto.userId))
                    {
                        _context.DutyDetail.Add(new DutyDetail
                        {
                            UserId = detailDto.userId,
                            Description = detailDto.Description,
                            DutyId = duty.Id
                        });
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await _context.Duties.Include(d => d.AssignedBy).Include(d => d.DutyDetails).ThenInclude(dd => dd.Users).FirstOrDefaultAsync(d => d.Id == duty.Id);

                if (result == null) throw new Exception("Cannot load result info after creation");

                return new ResponseModel.DutyDto
                {
                    Id = result.Id,
                    Name = result.Name ?? null,
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
                _logger.LogError(ex, "Error occurred while deleting duty. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
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

                var existingDuty = await _context.Duties.Include(d => d.DutyDetails).ThenInclude(dd => dd.Users).Include(d => d.AssignedBy).FirstOrDefaultAsync(d => d.Id == dto.Id);

                if (existingDuty == null)
                    throw new ArgumentException("Duty not found");

                if (currentUserRoles.Contains("Administrator"))
                {
               
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    // Manager: chỉ được sửa nếu chính họ là người được assign
                    var isManagerAssigned = existingDuty.AssignedById.Equals(currentUserId);
                    if (!isManagerAssigned)
                        throw new UnauthorizedAccessException("Manager can only update duties assigned to themselves");
                }
                else
                {
                    throw new UnauthorizedAccessException("You do not have permission to update this duty");
                }

                existingDuty.Name = dto.Name;
                existingDuty.IsCompleted = dto.IsCompleted;

                await _dutyRepository.UpdateDutyAsync(existingDuty);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.DutyDto
                {
                    Id = existingDuty.Id,
                    AssignedById = existingDuty.AssignedById,
                    Name = existingDuty.Name,
                    IsCompleted = existingDuty.IsCompleted,
                    StartDate = existingDuty.StartDate,
                    AssignedBy = existingDuty.AssignedBy?.Fullname,
                    DutyDetails = existingDuty.DutyDetails.Select(d => new ResponseModel.DutyDetailDto
                    {
                        DutyDetailId = d.DutyDetailId,
                        userId = d.UserId,
                        Name = d.Users?.Fullname,
                        Description = d.Description ?? null
                    }).ToList(),
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while updating the duty. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
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

                var existingDutyDetail = await _context.DutyDetail.Include(dd => dd.Users).Where(dd => !dd.Users.IsDeleted).FirstOrDefaultAsync(d => d.DutyDetailId == dto.DutyDetailId && !d.IsDeleted);
                if (existingDutyDetail == null)
                    throw new ArgumentException("Duty detail not found");

                if (currentUserRoles.Contains("Administrator"))
                {
                    // Do nothing, allow update
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    var userIdsToAssign = dto.userId;

                    var userToAssign = await _context.Users.FirstOrDefaultAsync(p => p.UserId == dto.userId);
                    if (userToAssign == null)
                        throw new ArgumentException("Cannot assign this employee");

                    if(userToAssign.DepartmentId != currentUser.DepartmentId)
                        throw new UnauthorizedAccessException("Manager can only assign users from the same department");
                }
                else
                {
                    throw new UnauthorizedAccessException("You do not have permission to assign users to duty");
                }

                existingDutyDetail.DutyDetailId = dto.DutyDetailId;
                existingDutyDetail.UserId = dto.userId;
                existingDutyDetail.Description = dto.Description;

                var result = await _dutyRepository.UpdateDutyDetailAsync(existingDutyDetail);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.DutyDetailDto
                {
                    DutyDetailId = result.DutyDetailId,
                    userId = result.UserId,
                    Name = result.Users?.Fullname,
                    Description = result.Description
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while updating the duty detail. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<string> SoftDeleteDutyAsync(Guid dutyId, Guid currentUserId, IList<string> currentUserRoles)
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
            if (currentUser == null)
                throw new ArgumentException("Cannot find current user");

            var entity = await _context.Duties.Include(d => d.DutyDetails).ThenInclude(dd => dd.Users).FirstOrDefaultAsync(d => d.Id == dutyId && !d.IsDeleted);

            if (entity == null)
                throw new ArgumentException("Cannot find duty with id " + dutyId);

            if (currentUserRoles.Contains("Manager"))
            {
                foreach (var detail in entity.DutyDetails)
                {
                    if (detail.Users != null && detail.Users.DepartmentId != currentUser.DepartmentId)
                    {
                        throw new UnauthorizedAccessException("Manager cannot delete duty from other department");
                    }
                }
            }

            entity.IsDeleted = true;
            foreach (var detail in entity.DutyDetails)
            {
                detail.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return $"Delete duty '{entity.Name}' success";
        }

        public async Task<string> SoftDeleteDutyDetailAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles)
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
            if (currentUser == null)
                throw new ArgumentException("Cannot find current user");

            var entity = await _context.DutyDetail.Include(dd => dd.Users).FirstOrDefaultAsync(dd => dd.DutyDetailId == dutyDetailId && !dd.IsDeleted);

            if (entity == null)
                throw new ArgumentException("Cannot find duty detail with id " + dutyDetailId);

            if (currentUserRoles.Contains("Manager"))
            {
                if (entity.Users != null && entity.Users.DepartmentId != currentUser.DepartmentId)
                {
                    throw new UnauthorizedAccessException("Manager cannot delete duty detail from other department");
                }
            }

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();

            return $"Delete duty detail'{entity.DutyDetailId}' success";
        }
    }
}
