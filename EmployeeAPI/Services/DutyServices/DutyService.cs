using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Duties;
using EmployeeAPI.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EmployeeAPI.Services.DutyServices
{
    public class DutyService : IDutyService
    {
        private readonly IDutyRepository _dutyRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<DutyService> _logger;
        public DutyService(IDutyRepository dutyRepository, IUserRepository userRepository, AppDbContext context, ILogger<DutyService> logger)
        {
            _dutyRepository = dutyRepository;
            _userRepository = userRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<ResponseModel.DutyDto>> GetAllAsync(Guid currentUserId, IList<string> currentUserRoles, string? name, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _dutyRepository.GetAllQueryable();

            if (currentUserRoles.Contains("Manager"))
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                if (currentUser.DepartmentId == null)
                    throw new Exception("Manager does not belong to any department");
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
            var duty = await _dutyRepository.GetDutyByIdAsync(id);

            if (duty == null)
                throw new ArgumentException("Cannot find duty");

            if (currentUserRoles.Contains("Manager"))
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                if (currentUser.DepartmentId == null)
                    throw new Exception("Manager does not belong to any department");

                if (duty.AssignedById != currentUserId)
                    throw new UnauthorizedAccessException("Manager can only access duties they assigned");
            }
            else if (currentUserRoles.Contains("Employee"))
            {
                var isAssignedToUser = duty.DutyDetails.Any(dd => dd.UserId == currentUserId);
                if (!isAssignedToUser)
                    throw new UnauthorizedAccessException("employee cannot access duties from other department");
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

                var userIdsToAssign = dto.DutyDetails.Select(d => d.userId).ToList();
                var assignedUsers = await _context.Users
                    .Where(u => userIdsToAssign.Contains(u.UserId))
                    .ToListAsync();

                if (assignedUsers.Any(u => u.IsDeleted || !u.IsActive))
                    throw new Exception("Cannot assign duty to deleted or inactive users");

                if (currentUserRoles.Contains("Manager"))
                {
                    if (currentUser.DepartmentId == null)
                        throw new Exception("Manager does not belong to any department");

                    if (assignedUsers.Any(u => u.DepartmentId != currentUser.DepartmentId))
                        throw new Exception("Manager can only assign users from the same department");
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
                        IsDeleted = false,
                    }).ToList()
                };

                var created = await _dutyRepository.AddAsync(duty);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await _dutyRepository.GetDutyByIdAsync(created.Id);

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
                        IsDeleted = d.IsDeleted,
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

                var duty = await _dutyRepository.GetDutyByIdAsync(DutyId);
                if (duty == null) throw new Exception("Duty not found");

                var userIdsToAssign = dto.DutyDetails.Select(d => d.userId).ToList();
                var assignedUsers = await _context.Users
                    .Where(u => userIdsToAssign.Contains(u.UserId))
                    .ToListAsync();

                if (assignedUsers.Count != userIdsToAssign.Count)
                    throw new Exception("One or more users not found");

                if (assignedUsers.Any(u => u.IsDeleted || !u.IsActive))
                    throw new Exception("User not found");

                if (currentUserRoles.Contains("Manager"))
                {
                    if (duty.AssignedById != currentUserId)
                        throw new UnauthorizedAccessException("Manager can only modify duties they assigned");

                    //if (currentUser.DepartmentId == null)
                    //    throw new Exception("Manager does not belong to any department");

                    if (assignedUsers.Any(u => u.DepartmentId != currentUser.DepartmentId))
                        throw new Exception("Manager can only assign users from the same department");
                }

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
                            DutyId = duty.Id,
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

                var existingDuty = await _dutyRepository.GetDutyByIdAsync(dto.Id);
                if (existingDuty == null)
                    throw new ArgumentException("Duty not found");

                if (currentUserRoles.Contains("Manager"))
                {
                    if (existingDuty.AssignedById != currentUserId)
                        throw new UnauthorizedAccessException("Manager can only update duties assigned by themselves");

                    //if (currentUser.DepartmentId == null)
                    //    throw new Exception("Manager does not belong to any department");
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
                        Description = d.Description
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

                var existingDutyDetail = await _dutyRepository.GetDutyDetailByIdAsync(dto.DutyDetailId);

                if (existingDutyDetail == null)
                    throw new ArgumentException("Duty detail not found");

                var userToAssign = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.userId && (!u.IsDeleted || u.IsActive));
                if (userToAssign == null)
                    throw new ArgumentException("User not found");

                if (currentUserRoles.Contains("Manager"))
                {
                    /*if (currentUser.DepartmentId == null)
                        throw new Exception("Manager does not belong to any department");*/

                    if (existingDutyDetail.Duty.AssignedById != currentUserId)
                        throw new UnauthorizedAccessException("Manager can only update duties assigned by themselves");

                    if (userToAssign.DepartmentId != currentUser.DepartmentId)
                        throw new UnauthorizedAccessException("Manager can only assign users from the same department");
                }

                existingDutyDetail.UserId = dto.userId;
                existingDutyDetail.Description = dto.Description;

                var result = await _dutyRepository.UpdateDutyDetailAsync(existingDutyDetail);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.DutyDetailDto
                {
                    DutyDetailId = result.DutyDetailId,
                    userId = result.UserId,
                    Name = result.Users.Fullname,
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                var entity = await _dutyRepository.GetDutyByIdAsync(dutyId);

                if (entity == null)
                    throw new ArgumentException("Cannot find duty " + dutyId);

                var isManager = currentUserRoles.Contains("Manager");

                if (isManager)
                {
                    if (entity.AssignedById != currentUserId)
                        throw new UnauthorizedAccessException("Manager can only delete duties they assigned");
                }

                entity.IsDeleted = true;
                foreach (var detail in entity.DutyDetails)
                {
                    detail.IsDeleted = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return "Delete duty" + entity.Name + "success";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while soft deleting the duty. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<string> SoftDeleteDutyDetailAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Cannot find current user");

                var entity = await _dutyRepository.GetDutyDetailByIdAsync(dutyDetailId);

                if (entity == null)
                    throw new ArgumentException("Cannot find duty detail " + dutyDetailId);

                var isManager = currentUserRoles.Contains("Manager");

                if (isManager)
                {
                    if (entity.Duty.AssignedById != currentUserId)
                        throw new UnauthorizedAccessException("Manager can only delete duty details of duties they assigned");
                }

                entity.IsDeleted = true;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return "Delete duty detail " + entity.DutyDetailId + " success";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while soft deleting the duty detail. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
    }
}
