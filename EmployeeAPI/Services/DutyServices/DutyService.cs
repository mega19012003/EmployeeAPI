using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;
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

        public async Task<PagedResult<ResponseModel.DutyResultDto>> GetAllAsync(Guid currentUserId, IList<string> currentUserRoles, string? name, int? pageIndex, int? pageSize)
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
                .Select(d => new ResponseModel.DutyResultDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    IsCompleted = d.IsCompleted,
                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    AssignedBy = d.AssignedBy.Fullname,
                    DutyDetails = d.DutyDetails.Where(dd => !dd.IsDeleted).Select(dd => new ResponseModel.DutyDetailResultDto
                    {
                        DutyDetailId = dd.DutyDetailId,
                        UserId = dd.UserId,
                        Description = dd.Description,
                        Name = dd.Users.Fullname,
                        IsCompleted = dd.IsCompleted
                    }).ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<ResponseModel.DutyResultDto>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }
        public async Task<ResponseModel.DutyResultDto> GetDutyByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
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

            return new ResponseModel.DutyResultDto
            {
                Id = duty.Id,
                Name = duty.Name,
                IsCompleted = duty.IsCompleted,
                StartDate = duty.StartDate,
                EndDate = duty.EndDate,
                AssignedBy = duty.AssignedBy?.Fullname,
                DutyDetails = duty.DutyDetails.Select(d => new ResponseModel.DutyDetailResultDto
                {
                    DutyDetailId = d.DutyDetailId,
                    UserId = d.UserId,
                    Description = d.Description,
                    Name = d.Users?.Fullname,
                    IsCompleted = d.IsCompleted
                }).ToList()
            };
        }
        public async Task<ResponseModel.DutyDetailResultDto> GetDutyDetailByIdAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles)
        {
            var dutyDetail = await _dutyRepository.GetDutyDetailByIdAsync(dutyDetailId);

            if (dutyDetail == null)
                throw new ArgumentException("Cannot find duty detail");

            var isAdmin = currentUserRoles.Contains("Admin");
            var isManager = currentUserRoles.Contains("Manager");
            var isEmployee = currentUserRoles.Contains("Employee");

            if (isManager)
            {
                bool isAssignedByMe = dutyDetail.Duty?.AssignedById == currentUserId;
                bool isSelf = dutyDetail.UserId == currentUserId;

                if (!isAssignedByMe && !isSelf)
                    throw new UnauthorizedAccessException("Manager can only access duty details they assigned or are assigned to");
            }
            else if (isEmployee)
            {
                if (dutyDetail.UserId != currentUserId)
                    throw new UnauthorizedAccessException("Employee can only access their own duty details");
            }

            return new ResponseModel.DutyDetailResultDto
            {
                DutyDetailId = dutyDetail.DutyDetailId,
                UserId = dutyDetail.UserId,
                Description = dutyDetail.Description,
                Name = dutyDetail.Users?.Fullname,
                IsCompleted = dutyDetail.IsCompleted
            };
        }
        
        public async Task<ResponseModel.DutyResultDto> AddDutyAsync(ResponseModel.CreateDutyDto dto, Guid currentUserId, IList<string> currentUserRoles)
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

                var conflict = await _context.DutyDetails
                    .Where(dd => userIdsToAssign.Contains(dd.UserId) && !dd.IsDeleted && !dd.IsCompleted)
                    .Select(dd => dd.UserId)
                    .FirstOrDefaultAsync();

                if (assignedUsers.Any(u => u.IsDeleted || !u.IsActive))
                    throw new Exception("User not found");

                if (currentUserRoles.Contains("Manager"))
                {
                    if (currentUser.DepartmentId == null)
                        throw new Exception("Manager does not belong to any department");

                    if (assignedUsers.Any(u => u.Role != RoleType.Employee))
                        throw new Exception("Only employees can be assigned to a duty");

                    if (assignedUsers.Any(u => u.DepartmentId != currentUser.DepartmentId))
                        throw new Exception("Manager can only assign employee from the same department");

                    if (conflict != Guid.Empty)
                        throw new InvalidOperationException("One or more employees are already assigned to an uncompleted duty.");
                }

                if (dto.StartDate.Date < DateTime.UtcNow.Date)
                    throw new ArgumentException("Start date cannot be earlier than today");

                if(dto.StartDate > dto.EndDate)
                    throw new ArgumentException("Start date cannot be later than end date");

                var duty = new Duty
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    AssignedById = currentUserId,
                    DutyDetails = dto.DutyDetails.Select(d => new DutyDetail
                    {
                        UserId = d.userId,
                        Description = d.Description,
                        IsDeleted = false,
                    }).ToList()
                };

                await _dutyRepository.AddAsync(duty);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.DutyResultDto
                {
                    Id = duty.Id,
                    Name = duty.Name,
                    IsCompleted = duty.IsCompleted,
                    StartDate = duty.StartDate,
                    AssignedBy = duty.AssignedBy?.Fullname,
                    DutyDetails = duty.DutyDetails.Select(d => new ResponseModel.DutyDetailResultDto
                    {
                        DutyDetailId = d.DutyDetailId,
                        UserId = d.UserId,
                        Description = d.Description,
                        Name = d.Users?.Fullname,
                        IsCompleted = d.IsCompleted

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
        public async Task<ResponseModel.DutyResultDto> AddDutyDetailAsync(ResponseModel.GetDutyDto dto, Guid DutyId, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null) throw new ArgumentException("Cannot find current user");

                var duty = await _dutyRepository.GetDutyByIdAsync(DutyId);
                if (duty == null) throw new Exception("Duty not found");

                //var userIdsToAssign = dto.DutyDetails.Select(d => d.userId).ToList();
                var userIdsToAssign = dto.DutyDetails.Select(d => d.userId).ToList();
                var assignedUsers = await _context.Users
                    .Where(u => userIdsToAssign.Contains(u.UserId))
                    .ToListAsync();

                var conflict = await _context.DutyDetails
                   .Where(dd => userIdsToAssign.Contains(dd.UserId) && !dd.IsDeleted && !dd.IsCompleted)
                   .Select(dd => dd.UserId)
                   .FirstOrDefaultAsync();

                if (assignedUsers.Any(u => u.IsDeleted || !u.IsActive))
                    throw new Exception("User not found");

                if (currentUserRoles.Contains("Manager"))
                {
                    if (duty.AssignedById != currentUserId)
                        throw new UnauthorizedAccessException("Manager can only modify duties they assigned");

                    if (assignedUsers.Any(u => u.Role != RoleType.Employee))
                        throw new Exception("Only employees can be assigned to a duty");

                    if (assignedUsers.Any(u => u.DepartmentId != currentUser.DepartmentId))
                        throw new Exception("Manager can only assign employee from the same department");

                    if (conflict != Guid.Empty)
                        throw new InvalidOperationException("One or more employees are already assigned to an uncompleted duty.");
                }

                var existingUserIds = duty.DutyDetails.Select(dd => dd.UserId).ToHashSet();
                foreach (var detailDto in dto.DutyDetails)
                {
                    /*if (!existingUserIds.Contains(detailDto.userId))
                    {*/
                        var newDetail = new DutyDetail
                        {
                            UserId = detailDto.userId,
                            Description = detailDto.Description,
                            DutyId = duty.Id,
                        };
                        await _dutyRepository.AddDutyDetailAsync(newDetail);
                    }
                    /*else {
                        throw new Exception("This user is already assigned to this duty");
                        }
                    }*/

                await _context.SaveChangesAsync();
                await UpdateDutyCompletionStatusAsync(duty.Id);
                await transaction.CommitAsync();

                var result = await _dutyRepository.GetDutyByIdAsync(duty.Id);

                return new ResponseModel.DutyResultDto
                {
                    Id = result.Id,
                    Name = result.Name ?? null,
                    StartDate = result.StartDate,
                    AssignedBy = result.AssignedBy?.Fullname,
                    DutyDetails = result.DutyDetails.Select(d => new ResponseModel.DutyDetailResultDto
                    {
                        DutyDetailId = d.DutyDetailId,
                        UserId = d.UserId,
                        Description = d.Description,
                        Name = d.Users?.Fullname,
                        IsCompleted = d.IsCompleted
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

        
        public async Task<ResponseModel.DutyResultDto> UpdateDutyAsync(ResponseModel.UpdateDutyDto dto, Guid currentUserId, IList<string> currentUserRoles)
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
                }

                //if (dto.StartDate.Date < DateTime.UtcNow.Date)
                //    throw new ArgumentException("Start date cannot be earlier than today");
                if (dto.StartDate > dto.EndDate)
                    throw new ArgumentException("Start date cannot be earlier than end date");

                existingDuty.Name = dto.Name;
                existingDuty.StartDate = dto.StartDate;
                existingDuty.EndDate = dto.EndDate;
                //existingDuty.IsCompleted = dto.IsCompleted;

                await _dutyRepository.UpdateDutyAsync(existingDuty);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.DutyResultDto
                {
                    Id = existingDuty.Id,
                    Name = existingDuty.Name,
                    IsCompleted = existingDuty.IsCompleted,
                    StartDate = existingDuty.StartDate,
                    EndDate = existingDuty.EndDate,
                    AssignedBy = existingDuty.AssignedBy?.Fullname,
                    DutyDetails = existingDuty.DutyDetails.Select(d => new ResponseModel.DutyDetailResultDto
                    {
                        DutyDetailId = d.DutyDetailId,
                        UserId = d.UserId,
                        Name = d.Users?.Fullname,
                        Description = d.Description,
                        IsCompleted = d.IsCompleted
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
        public async Task<ResponseModel.DutyDetailResultDto> UpdateDutyDetailAsync(ResponseModel.UpdateDutyDetailDto dto, Guid currentUserId, IList<string> currentUserRoles)
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

                await _dutyRepository.UpdateDutyDetailAsync(existingDutyDetail);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel.DutyDetailResultDto
                {
                    DutyDetailId = existingDutyDetail.DutyDetailId,
                    Name = existingDutyDetail.Users.Fullname,
                    Description = existingDutyDetail.Description,
                    IsCompleted = existingDutyDetail.IsCompleted
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while updating the duty detail. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<string> MarkDutyDetailAsCompletedAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var detail = await _dutyRepository.GetDutyDetailByIdAsync(dutyDetailId);
                if (detail == null)
                    throw new ArgumentException("Duty detail not found");

                var isEmployee = currentUserRoles.Contains("Employee");
                var isManager = currentUserRoles.Contains("Manager");
                var isAdmin = currentUserRoles.Contains("Administrator");

                if (isEmployee)
                {
                    if (detail.Duty.EndDate < DateTime.UtcNow.Date)
                    {
                        throw new ArgumentException("Your're too late to complete your task");
                    }
                    else
                    {
                        if (detail.UserId != currentUserId)
                            throw new UnauthorizedAccessException("Employee can only complete their own duty details.");
                    }
                }

                if (isManager)
                {
                    var currentUser = await _context.Users.FindAsync(currentUserId);
                    if (detail.Duty.AssignedById != currentUserId)
                        throw new UnauthorizedAccessException("Manager can only modify duties they assigned.");

                    if (detail.Users.DepartmentId != currentUser.DepartmentId)
                        throw new UnauthorizedAccessException("Manager can only complete duty details of users in the same department.");
                }

                detail.IsCompleted = true;
                await _dutyRepository.UpdateDutyDetailAsync(detail);
                await _context.SaveChangesAsync();
                await UpdateDutyCompletionStatusAsync(detail.DutyId);
                await transaction.CommitAsync();

                return "Duty detail " + detail.Description + " completed";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while marking duty detail as completed. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        private async Task UpdateDutyCompletionStatusAsync(Guid dutyId)
        {
            var duty = await _dutyRepository.GetDutyByIdAsync(dutyId);
            if (duty == null)
                throw new ArgumentException("Duty not found");

            bool allCompleted = duty.DutyDetails
                .Where(d => !d.IsDeleted)
                .All(d => d.IsCompleted);

            if (duty.IsCompleted != allCompleted)
            {
                duty.IsCompleted = allCompleted;
                await _dutyRepository.UpdateDutyAsync(duty);
                await _context.SaveChangesAsync();
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

                await _dutyRepository.UpdateDutyAsync(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return "Delete duty " + entity.Name + " success";
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
                await _dutyRepository.UpdateDutyDetailAsync(entity);
                await _context.SaveChangesAsync();
                await UpdateDutyCompletionStatusAsync(entity.DutyId);
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
