using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;

using EmployeeAPI.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.Design;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using static EmployeeAPI.Services.DutyServices.ResponseModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EmployeeAPI.Services.DutyServices
{
    public class DutyService : IDutyService
    {
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<DutyService> _logger;
        private readonly GoogleSheetHelper _googleSheetHelper;
        private readonly IMemoryCache _cache;
        public DutyService(IMemoryCache cache, IUserRepository userRepository, AppDbContext context, ILogger<DutyService> logger, GoogleSheetHelper googleSheetHelper)
        {
            _userRepository = userRepository;
            _context = context;
            _logger = logger;
            _googleSheetHelper = googleSheetHelper;
            _cache = cache;
        }

        public async Task<PagedResult<DutyResultDto>> GetAllAsync(Guid currentUserId, IList<string> currentUserRoles, string? name, Guid? companyId, int? Day, int? Month, int? Year, string? filterStatus, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            //var dutyRows = await _googleSheetHelper.ReadSheetAsync("Duty!A2:K");
            //var detailRows = await _googleSheetHelper.ReadSheetAsync("Detail!A2:F");

            var dutyRows = await _cache.GetOrCreateAsync("CachedDutyRows", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30); // Cache 1 phút
                return await _googleSheetHelper.ReadSheetAsync("Duty!A2:K");
            });

            var detailRows = await _cache.GetOrCreateAsync("CachedDetailRows", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30); // Cache 1 phút
                return await _googleSheetHelper.ReadSheetAsync("Detail!A2:L");
            });

            var users = await _context.Users.ToListAsync();
            var companies = await _context.Companies.ToListAsync();
            var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);

            var duties = dutyRows
                .Where(row => !string.IsNullOrWhiteSpace(row[0]?.ToString()))
                .Select(row => new
                {
                    Id = Guid.Parse(row[0].ToString()),
                    Name = row[1].ToString(),
                    AssignedById = Guid.Parse(row[2].ToString()),
                    StartDate = DateOnly.Parse(row[3].ToString()),
                    EndDate = DateOnly.Parse(row[4].ToString()),
                    //IsCompleted = bool.Parse(row[5].ToString()),
                    Status = row[5].ToString(),
                    IsDeleted = bool.Parse(row[6].ToString()),
                    CompanyId = string.IsNullOrEmpty(row[7].ToString()) ? (Guid?)null : Guid.Parse(row[7].ToString()),
                    CreatedDate = DateTime.Parse(row[8].ToString()),
                    UpdatedDate = string.IsNullOrWhiteSpace(row.ElementAtOrDefault(9)?.ToString()) ? (DateTime?)null : DateTime.Parse(row[9].ToString()),
                    Note = row.ElementAtOrDefault(10)?.ToString()
                })
                .Where(d => !d.IsDeleted)
                .ToList();

            var dutyDetails = detailRows
                .Where(row => !string.IsNullOrWhiteSpace(row[0]?.ToString()))
                .Select(row => new
                {
                    DutyDetailId = Guid.Parse(row[0].ToString()),
                    DutyId = Guid.Parse(row[1].ToString()),
                    UserId = Guid.Parse(row[2].ToString()),
                    Deadline = DateOnly.Parse(row[3].ToString()),
                    Title = row.ElementAtOrDefault(4)?.ToString() ?? "",
                    Description = row.ElementAtOrDefault(5)?.ToString() ?? "",
                    Status = row.ElementAtOrDefault(6)?.ToString(),
                    IsDeleted = bool.TryParse(row.ElementAtOrDefault(7)?.ToString(), out var deleted) && deleted,
                    CreatedDate = DateTime.Parse(row[8].ToString()),
                    UpdatedDate = string.IsNullOrWhiteSpace(row.ElementAtOrDefault(9)?.ToString()) ? (DateTime?)null : DateTime.Parse(row[9].ToString()),
                    CompletedDate = string.IsNullOrWhiteSpace(row.ElementAtOrDefault(10)?.ToString()) ? (DateTime?)null : DateTime.Parse(row[10].ToString()),
                    Note = row.ElementAtOrDefault(11)?.ToString()
                })
                .Where(dd => !dd.IsDeleted)
                .ToList();

            // Join Duty + DutyDetail
            var grouped = duties
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.AssignedById,
                    d.StartDate,
                    d.EndDate,
                    d.Status,
                    d.CompanyId,
                    d.CreatedDate,
                    d.UpdatedDate,
                    d.Note,
                    DutyDetails = dutyDetails
                        .Where(dd => dd.DutyId == d.Id)
                        .ToList()
                }).ToList();

            var validStatuses = new[] { "Pending", "InProgress", "Completed" };

            if (!string.IsNullOrWhiteSpace(filterStatus))
            {
                var trimmedStatus = filterStatus.Trim();
                if (validStatuses.Contains(trimmedStatus, StringComparer.OrdinalIgnoreCase))
                {
                    grouped = grouped
                        .Where(d => string.Equals(d.Status, trimmedStatus, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }

            if (currentUserRoles.Contains("SystemAdmin"))
            {
                if (companyId.HasValue)
                    grouped = grouped.Where(d => d.CompanyId == companyId).ToList();
            }
            else if (currentUserRoles.Contains("Administrator"))
            {
                if (currentUser?.CompanyId == null)
                    throw new ArgumentException("Administrator chưa có công ty.");
                grouped = grouped.Where(d => d.CompanyId == currentUser.CompanyId).ToList();
            }
            else if (currentUserRoles.Contains("Manager"))
            {
                grouped = grouped.Where(d => d.AssignedById == currentUserId).ToList();
            }
            else if (currentUserRoles.Contains("Employee"))
            {
                grouped = grouped.Where(d => d.DutyDetails.Any(dd => dd.UserId == currentUserId)).ToList();
            }

            var now = DateTime.Now;
            Year ??= now.Year;

            if (Year.HasValue)
                grouped = grouped.Where(d => d.StartDate.Year == Year.Value).ToList();
            if (Month.HasValue)
                grouped = grouped.Where(d => d.StartDate.Month == Month.Value).ToList();
            if (Day.HasValue)
                grouped = grouped.Where(d => d.StartDate.Day == Day.Value).ToList();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var lowered = name.ToLower();
                grouped = grouped.Where(d => d.Name.ToLower().Contains(lowered)).ToList();
            }

            var totalCount = grouped.Count;

            var pagedItems = grouped
                .OrderByDescending(d => d.StartDate)
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(d => new DutyResultDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    CreatedDate = d.CreatedDate,
                    UpdatedDate = d.UpdatedDate ?? null,
                    Note = d.Note ?? null,
                    //IsCompleted = d.IsCompleted,
                    Status = d.Status,
                    AssignedBy = users.FirstOrDefault(u => u.UserId == d.AssignedById)?.Fullname ?? "",
                    AssignImageUrl = users.FirstOrDefault(u => u.UserId == d.AssignedById)?.ImageUrl ?? "",
                    CompanyName = companies.FirstOrDefault(c => c.Id == d.CompanyId)?.Name ?? "",
                    DutyDetails = d.DutyDetails.Select(dd => new DutyDetailResultDto
                    {
                        DutyDetailId = dd.DutyDetailId,
                        UserId = dd.UserId,
                        Title = dd.Title,
                        Description = dd.Description,
                        Deadline = dd.Deadline,
                        Name = users.FirstOrDefault(u => u.UserId == dd.UserId)?.Fullname ?? "",
                        UserImageUrl = users.FirstOrDefault(u => u.UserId == dd.UserId)?.ImageUrl ?? "",
                        CreatedDate = dd.CreatedDate,
                        UpdatedDate = dd.UpdatedDate ?? null,
                        CompletedDate = dd.CompletedDate ?? null,
                        Note = dd.Note ?? "",
                        //IsCompleted = dd.IsCompleted
                        Status = dd.Status
                    }).ToList()
                }).ToList();

            return new PagedResult<DutyResultDto>
            {
                Items = pagedItems,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
        }
        public async Task<ResponseModel.DutyResultDto> GetDutyByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            var duty = await _googleSheetHelper.GetDutyByIdAsync(id); 
            if (duty.CompanyId == null || duty.CompanyId == Guid.Empty)
                throw new Exception("Thiếu CompanyId từ duty, kiểm tra dữ liệu Google Sheet.");
            var currentUser = await _context.Users.FindAsync(currentUserId); 
            if (duty.CompanyId != currentUser.CompanyId)
                throw new UnauthorizedAccessException("Không cùng công ty");


            if (currentUserRoles.Contains("Administrator"))
            {
                if (currentUser?.CompanyId == null)
                    throw new ArgumentException("Administrator chưa có công ty.");

                if (duty.CompanyId != currentUser.CompanyId)
                    throw new UnauthorizedAccessException("Administrator chỉ có thể xem công việc trong công ty của mình.");
            }
            else if (currentUserRoles.Contains("Manager"))
            {
                if (currentUser.DepartmentId == null)
                    throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                if (duty.AssignedById != currentUserId)
                    throw new UnauthorizedAccessException("Manager chỉ có thể truy cập công việc do mình tạo ra");
            }

            var dutyDetails = await _googleSheetHelper.GetDutyDetailsByDutyIdAsync(id);

            if (currentUserRoles.Contains("Employee"))
            {
                var isAssignedToUser = dutyDetails.Any(dd => dd.UserId == currentUserId);
                if (!isAssignedToUser)
                    throw new UnauthorizedAccessException("Nhân viên không thể truy cập công việc của người khác");
            }
            var assignedUser = await _context.Users.FindAsync(duty.AssignedById);
            var dutyResult = new ResponseModel.DutyResultDto
            {
                Id = duty.Id,
                Name = duty.Name,
                StartDate = duty.StartDate,
                EndDate = duty.EndDate,
                //IsCompleted = duty.IsCompleted,
                Status = duty.Status,
                AssignedBy = (await _context.Users.FindAsync(duty.AssignedById))?.Fullname,
                AssignImageUrl = assignedUser?.ImageUrl,
                CompanyName = (await _context.Companies.FindAsync(duty.CompanyId))?.Name,
                Note = duty.Note ?? null,
                CreatedDate = duty.CreatedDate,
                UpdatedDate = duty.UpdatedDate ?? null,
                DutyDetails = new List<ResponseModel.DutyDetailResultDto>()
            };

            foreach (var detail in dutyDetails)
            {
                var user = await _context.Users.FindAsync(detail.UserId);
                dutyResult.DutyDetails.Add(new ResponseModel.DutyDetailResultDto
                {
                    DutyDetailId = detail.DutyDetailId,
                    UserId = detail.UserId,
                    Title = detail.Title,
                    Description = detail.Description,
                    Deadline = detail.Deadline,
                    Name = user?.Fullname,
                    UserImageUrl = user?.ImageUrl,
                    //IsCompleted = detail.IsCompleted
                    Status = detail.Status,
                    CreatedDate = detail.CreatedDate,
                    UpdatedDate = detail.UpdatedDate,
                    CompletedDate = detail.CompletedDate,
                    Note = detail.Note,
                });
            }

            return dutyResult;
        }
        public async Task<ResponseModel.DutyDetailResultDto> GetDutyDetailByIdAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles)
        {
            var detailRows = await _googleSheetHelper.ReadSheetAsync("Detail");

            foreach (var row in detailRows.Skip(1)) // Bỏ dòng tiêu đề
            {
                if (row.Count < 6) continue;
                if (!Guid.TryParse(row[0]?.ToString(), out var detailId)) continue;

                if (detailId != dutyDetailId) continue;

                var dutyId = Guid.TryParse(row[1]?.ToString(), out var parsedDutyId) ? parsedDutyId : Guid.Empty;
                var userId = Guid.TryParse(row[2]?.ToString(), out var uId) ? uId : Guid.Empty;
                var deadline = DateOnly.TryParse(row[3]?.ToString(), out var parsedDeadline) ? parsedDeadline : DateOnly.MinValue;
                var title = row[4]?.ToString() ?? "";
                var description = row[5]?.ToString() ?? "";
                //var isCompleted = bool.TryParse(row[4]?.ToString(), out var comp) && comp;
                var status = row[6]?.ToString() ?? "";
                var isDeleted = bool.TryParse(row[7]?.ToString(), out var del) && del;
                var createdDate = DateTime.TryParse(row[8]?.ToString(), out var created) ? created : DateTime.MinValue;
                var updatedDate = string.IsNullOrWhiteSpace(row.ElementAtOrDefault(9)?.ToString()) ? (DateTime?)null : DateTime.Parse(row[9].ToString());
                var completedDate = string.IsNullOrWhiteSpace(row.ElementAtOrDefault(10)?.ToString()) ? (DateTime?)null : DateTime.Parse(row[10].ToString());
                var note = row.ElementAtOrDefault(11)?.ToString();

                if (isDeleted)
                    throw new ArgumentException("Công việc chi tiết này đã bị xóa");

                // Lấy thông tin Duty từ DutyList (để lấy AssignedById & CompanyId)
                var dutyRows = await _googleSheetHelper.ReadSheetAsync("Duty");
                var matchingDutyRow = dutyRows.Skip(1).FirstOrDefault(r =>
                    r.Count >= 8 &&
                    Guid.TryParse(r[0]?.ToString(), out var id) &&
                    id == dutyId
                );

                if (matchingDutyRow == null)
                    throw new ArgumentException("Không tìm thấy nhiệm vụ cha (Duty) tương ứng");

                var assignedById = Guid.TryParse(matchingDutyRow[2]?.ToString(), out var assignBy) ? assignBy : Guid.Empty;
                var dutyCompanyId = Guid.TryParse(matchingDutyRow[7]?.ToString(), out var companyId) ? companyId : Guid.Empty;

                var isAdmin = currentUserRoles.Contains("Admin");
                var isManager = currentUserRoles.Contains("Manager");
                var isEmployee = currentUserRoles.Contains("Employee");

                if (isAdmin)
                {
                    var currentUser = await _context.Users.FindAsync(currentUserId);
                    if (currentUser?.CompanyId == null)
                        throw new ArgumentException("Administrator chưa có công ty.");
                    if (dutyCompanyId != currentUser.CompanyId)
                        throw new UnauthorizedAccessException("Administrator chỉ có thể xem công việc trong công ty của mình.");
                }
                else if (isManager)
                {
                    bool isAssignedByMe = assignedById == currentUserId;
                    bool isSelf = userId == currentUserId;

                    if (!isAssignedByMe && !isSelf)
                        throw new UnauthorizedAccessException("Manager chỉ có thể truy cập công việc do mình tạo ra hoặc công việc của bản thân");
                }
                else if (isEmployee)
                {
                    if (userId != currentUserId)
                        throw new UnauthorizedAccessException("Nhân viên chỉ có thể truy cập công việc của bản thân");
                }

                var user = await _context.Users.FindAsync(userId);

                return new ResponseModel.DutyDetailResultDto
                {
                    DutyDetailId = dutyDetailId,
                    UserId = userId,
                    Title = title,
                    Description = description,
                    Deadline = deadline,
                    Name = user?.Fullname ?? "",
                    UserImageUrl = user?.ImageUrl ?? "",
                    //IsCompleted = isCompleted
                    Status = status,
                    CreatedDate = createdDate,
                    UpdatedDate = updatedDate,
                    CompletedDate = completedDate,
                    Note = note
                };
            }
            throw new ArgumentException("Không tìm thấy công việc chi tiết này");
        }

        public async Task<ResponseModel.DutyResultDto> AddDutyAsync(ResponseModel.CreateDutyDto dto, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                var userIdsToAssign = dto.DutyDetails.Select(d => d.userId).ToList();

                var assignedUsers = await _context.Users
                    .Where(u => userIdsToAssign.Contains(u.UserId))
                    .ToListAsync();

                // Đọc dữ liệu DutyDetail từ Google Sheet để kiểm tra conflict
                var allDetailRows = await _googleSheetHelper.ReadSheetAsync("Detail");
                var unfinishedConflicts = allDetailRows
                .Where(r =>
                    Guid.TryParse(r[1]?.ToString(), out var dutyIdFromSheet) &&
                    Guid.TryParse(r[2]?.ToString(), out var uid) &&  userIdsToAssign.Contains(uid) &&
                    Enum.TryParse<Enums.DutyStatus>(r[6]?.ToString(), out var status) && status != Enums.DutyStatus.Completed &&
                    bool.TryParse(r[7]?.ToString(), out var isDeleted) && !isDeleted
                )
                .Select(r => Guid.Parse(r[2].ToString()))
                .Distinct()
                .ToList();

                if (unfinishedConflicts.Any())
                {
                    throw new InvalidOperationException("Một hoặc nhiều nhân viên đang có công việc khác chưa hoàn thành.");
                }

                if (assignedUsers.Any(u => u.IsDeleted || !u.IsActive))
                    throw new ArgumentException("Không tìm thấy người dùng hoặc người dùng đã bị vô hiệu hóa");

                if (currentUserRoles.Contains("Administrator"))
                {
                    if (currentUser.CompanyId == null)
                        throw new ArgumentException("Admin chưa có công ty. Vui lòng liên hệ System Admin để cập nhật công ty");

                    if (assignedUsers.Any(u => u.Role != RoleType.Employee))
                        throw new ArgumentException("Chỉ nhân viên được phép gán vào 1 công việc");

                    if (assignedUsers.Any(u => u.CompanyId != currentUser.CompanyId))
                        throw new ArgumentException("Admin chỉ được chọn nhân viên cùng công ty để thực hiện công việc");

                }

                if (currentUserRoles.Contains("Manager"))
                {
                    if (currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                    if (assignedUsers.Any(u => u.Role != RoleType.Employee))
                        throw new ArgumentException("Chỉ nhân viên được phép gán vào 1 công việc");

                    if (assignedUsers.Any(u => u.DepartmentId != currentUser.DepartmentId))
                        throw new ArgumentException("Manager chỉ được chọn nhân viên cùng phòng ban để thực hiện công việc");

                }

                if (dto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
                    throw new ArgumentException("Ngày bắt đầu không được trước ngày hiện tại");

                if (dto.StartDate > dto.EndDate)
                    throw new ArgumentException("Ngày bắt đầu không được sau ngày kết thúc");

                var duty = new Duty
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    AssignedById = currentUserId,
                    CompanyId = (Guid)currentUser.CompanyId!,
                    //IsCompleted = false,
                    Status = Enums.DutyStatus.Pending,
                    IsDeleted = false,
                    CreatedDate = vnNow,
                    UpdatedDate = null,
                    Note = null
                };

                var dutyDetails = new List<DutyDetail>();
                foreach (var detailDto in dto.DutyDetails)
                {
                    if (detailDto.Deadline != null)
                    {
                        if (detailDto.Deadline < duty.StartDate)
                            throw new ArgumentException("Deadline không được trước ngày bắt đầu của công việc.");

                        if (detailDto.Deadline > duty.EndDate)
                            throw new ArgumentException("Deadline không được sau ngày kết thúc của công việc.");
                    }

                    var newDetail = new DutyDetail
                    {
                        DutyDetailId = Guid.NewGuid(),
                        UserId = detailDto.userId,
                        DutyId = duty.Id,
                        Title = detailDto.Title,
                        Description = detailDto.Description,
                        Deadline = detailDto.Deadline,
                        Status = Enums.DutyStatus.Pending,
                        IsDeleted = false,
                        CreatedDate = vnNow,
                        UpdatedDate = null,
                        CompletedDate = null,
                        Note = null
                    };
                    dutyDetails.Add(newDetail);
                    //await _googleSheetHelper.AddDutyDetailAsync(newDetail);
                }

                duty.DutyDetails = dutyDetails;

                // Ghi vào Google Sheets
                await _googleSheetHelper.AppendDutyAsync(duty);
                await _googleSheetHelper.AppendDutyDetailsAsync(dutyDetails);

                return new ResponseModel.DutyResultDto
                {
                    Id = duty.Id,
                    Name = duty.Name,
                    StartDate = duty.StartDate,
                    EndDate = duty.EndDate,
                    CreatedDate = duty.CreatedDate,
                    UpdatedDate = duty.UpdatedDate,
                    Note = duty.Note,
                    //IsCompleted = duty.IsCompleted,
                    Status = duty.Status.ToString(),
                    AssignedById = currentUser.UserId,
                    AssignedBy = currentUser.Fullname,
                    AssignImageUrl = currentUser.ImageUrl ?? "",
                    CompanyId = duty.CompanyId?? Guid.Empty,
                    CompanyName = currentUser.Company?.Name ?? duty.CompanyId.ToString() ?? "",
                    DutyDetails = dutyDetails.Select(d => new ResponseModel.DutyDetailResultDto
                    {
                        DutyDetailId = d.DutyDetailId,
                        UserId = d.UserId,
                        Description = d.Description,
                        Title = d.Title,
                        Deadline = d.Deadline,
                        CreatedDate = d.CreatedDate,
                        UpdatedDate = d.UpdatedDate,
                        CompletedDate = d.CompletedDate,
                        Note = d.Note,
                        //IsCompleted = d.IsCompleted,
                        Status = d.Status.ToString(),
                        Name = assignedUsers.FirstOrDefault(u => u.UserId == d.UserId)?.Fullname ?? "",
                        UserImageUrl = assignedUsers.FirstOrDefault(u => u.UserId == d.UserId)?.ImageUrl ?? ""
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm công việc vào Google Sheet: {Message}", ex.Message);
                throw;
            }
        }
        public async Task<ResponseModel.DutyResultDto> AddDutyDetailAsync(ResponseModel.GetDutyDto dto, Guid dutyId, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                var duty = await _googleSheetHelper.GetDutyByIdAsync(dutyId);
                if (duty == null)
                    throw new Exception("Không tìm thấy công việc");

                var userIdsToAssign = dto.DutyDetails.Select(d => d.userId).ToList();
                var assignedUsers = await _context.Users
                    .Where(u => userIdsToAssign.Contains(u.UserId))
                    .ToListAsync();

                if (assignedUsers.Any(u => u.IsDeleted || !u.IsActive))
                    throw new ArgumentException("Không tìm thấy người dùng hợp lệ");


                var allDutyDetails = await _googleSheetHelper.GetAllDutyDetailsAsync();
                var unfinishedConflicts = allDutyDetails
                .Where(d =>
                    userIdsToAssign.Contains(d.UserId) &&
                    !d.IsDeleted &&
                    d.Status != Enums.DutyStatus.Completed &&
                    d.DutyId != dutyId 
                )
                .Select(d => d.UserId)
                .Distinct()
                .ToList();

                if (unfinishedConflicts.Any())
                {
                    throw new InvalidOperationException("Một hoặc nhiều nhân viên đang tham gia công việc khác chưa hoàn thành.");
                }

                if(duty.Status == Enums.DutyStatus.Completed.ToString())
                    throw new InvalidOperationException("Không thể thêm chi tiết vào công việc đã hoàn thành");

                if (currentUserRoles.Contains("Administrator"))
                {
                    if (duty.CompanyId != currentUser.CompanyId)
                        throw new UnauthorizedAccessException("Admin chỉ thao tác trong công ty của mình");

                    if (assignedUsers.Any(u => u.Role != RoleType.Employee))
                        throw new Exception("Chỉ nhân viên được phép gán vào công việc");

                    if (assignedUsers.Any(u => u.CompanyId != currentUser.CompanyId))
                        throw new ArgumentException("Admin chỉ gán nhân viên cùng công ty");

                    //if (conflict != null)
                    //    throw new InvalidOperationException("Một hoặc nhiều nhân viên đang có công việc chưa hoàn thành");
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    if (duty.AssignedById != currentUserId)
                        throw new UnauthorizedAccessException("Manager chỉ sửa công việc do họ tạo");

                    if (assignedUsers.Any(u => u.Role != RoleType.Employee))
                        throw new Exception("Chỉ nhân viên được phép gán vào công việc");

                    if (assignedUsers.Any(u => u.DepartmentId != currentUser.DepartmentId))
                        throw new Exception("Manager chỉ gán nhân viên cùng phòng ban");

                    //if (conflict != null)
                    //    throw new InvalidOperationException("Một hoặc nhiều nhân viên đang có công việc chưa hoàn thành");
                }

                // Thêm DutyDetail vào Google Sheets
                foreach (var detailDto in dto.DutyDetails)
                {
                    if (detailDto.Deadline < duty.StartDate || detailDto.Deadline > duty.EndDate)
                    {
                        throw new ArgumentException("Deadline không được nằm ngoài khoảng thời gian của công việc.");
                    }
                    var newDetail = new DutyDetail
                    {
                        DutyDetailId = Guid.NewGuid(),
                        UserId = detailDto.userId,
                        DutyId = dutyId,
                        Description = detailDto.Description,
                        Deadline = detailDto.Deadline,
                        // IsCompleted = false,
                        Status = Enums.DutyStatus.Pending,
                        IsDeleted = false,
                        Title = detailDto.Title,
                        CreatedDate = vnNow,
                        UpdatedDate = null,
                        CompletedDate = null,
                        Note = null,
                    };
                    await _googleSheetHelper.AddDutyDetailAsync(newDetail);
                }
                await _googleSheetHelper.UpdateDutyCompletionStatusAsync(dutyId);

                var updatedDuty = await _googleSheetHelper.GetDutyByIdAsync(dutyId);

                return new ResponseModel.DutyResultDto
                {
                    Id = updatedDuty.Id,
                    Name = updatedDuty.Name,
                    StartDate = updatedDuty.StartDate,
                    EndDate = updatedDuty.EndDate,
                    CreatedDate = updatedDuty.CreatedDate,
                    UpdatedDate = updatedDuty.UpdatedDate,
                    Note = updatedDuty.Note,
                    AssignedBy = currentUser.Fullname,
                    CompanyName = (await _context.Companies.FindAsync(updatedDuty.CompanyId))?.Name ?? "",
                    DutyDetails = updatedDuty.DutyDetails.Select(d => new ResponseModel.DutyDetailResultDto 
                    {
                        DutyDetailId = d.DutyDetailId,
                        UserId = d.UserId,
                        Description = d.Description,
                        Deadline = d.Deadline,
                        CreatedDate = d.CreatedDate,
                        UpdatedDate = d.UpdatedDate,
                        CompletedDate = d.CompletedDate,
                        Title = d.Title,
                        Note = d.Note,
                        //IsCompleted = d.IsCompleted,
                        Status = d.Status.ToString(),
                        Name = assignedUsers.FirstOrDefault(u => u.UserId == d.UserId)?.Fullname ?? "",
                        UserImageUrl = assignedUsers.FirstOrDefault(u => u.UserId == d.UserId)?.ImageUrl ?? "",
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm duty detail: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ResponseModel.DutyResultDto> UpdateDutyAsync(ResponseModel.UpdateDutyDto dto, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                var existingDuty = await _googleSheetHelper.GetDutyByIdAsync(dto.Id);
                if (existingDuty == null)
                    throw new ArgumentException("Không tìm thấy công việc");

                if (existingDuty.IsDeleted )
                    throw new ArgumentException("Công việc đã bị xóa");

                if (currentUserRoles.Contains("Administrator"))
                {
                    if (existingDuty.CompanyId != currentUser.CompanyId)
                        throw new ArgumentException("Admin chỉ có thể chỉnh sửa công việc trong công ty của mình");
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    if (existingDuty.AssignedById != currentUserId)
                        throw new ArgumentException("Manager chỉ có thể chỉnh sửa công việc do họ tạo ra");
                }

                //////////////
                if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate > dto.EndDate)
                    throw new ArgumentException("Ngày bắt đầu không được để sau ngày kết thúc");

                if (existingDuty.Status == Enums.DutyStatus.Pending.ToString())
                {
                    if (!string.IsNullOrEmpty(dto.Name)) existingDuty.Name = dto.Name;

                    if (dto.StartDate.HasValue)
                    {
                        if (dto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
                            throw new ArgumentException("Ngày bắt đầu không được trước ngày hiện tại");
                        existingDuty.StartDate = dto.StartDate.Value;
                    }

                    if (dto.EndDate.HasValue)
                    {
                        if (dto.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
                            throw new ArgumentException("Ngày kết thúc không được trước ngày hiện tại");
                        existingDuty.EndDate = dto.EndDate.Value;
                    }
                }
                else if (existingDuty.Status == Enums.DutyStatus.InProgress.ToString())
                {
                    if (dto.Name != existingDuty.Name || dto.StartDate != existingDuty.StartDate)
                        throw new ArgumentException("Không thể chỉnh sửa tên hoặc ngày bắt đầu khi công việc đang thực hiện");

                    if (dto.EndDate.HasValue)
                    {
                        if (dto.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
                            throw new ArgumentException("Ngày kết thúc không được trước ngày hiện tại");
                        existingDuty.EndDate = dto.EndDate.Value;
                    }
                }
                ///////////////

                //if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate > dto.EndDate)
                //    throw new ArgumentException("Ngày bắt đầu không được để sau ngày kết thúc");

                //if (!string.IsNullOrEmpty(dto.Name)) existingDuty.Name = dto.Name;
                //if (dto.StartDate.HasValue)
                //{
                //    if(dto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
                //        throw new ArgumentException("Ngày bắt đầu không được trước ngày hiện tại");
                //    existingDuty.StartDate = dto.StartDate.Value;
                //}
                //if (dto.EndDate.HasValue)
                //{
                //    if (dto.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
                //        throw new ArgumentException("Ngày kết thúc không được trước ngày hiện tại");
                //        existingDuty.EndDate = dto.EndDate.Value;
                //}

                existingDuty.Note = dto.Note + " (Được cập nhật bởi " + currentUser.Fullname + ")";
                existingDuty.UpdatedDate = vnNow;

                //if (dto.Status.HasValue) existingDuty.Status = dto.Status.Value.ToString();

                await _googleSheetHelper.UpdateDutyRowAsync(existingDuty);

                var allDutyDetails = await _googleSheetHelper.GetAllDutyDetailsAsync();
                var details = allDutyDetails.Where(d => d.DutyId == dto.Id).ToList();

                var userIds = details.Select(d => d.UserId).Distinct().ToList();
                var users = await _context.Users.Where(u => userIds.Contains(u.UserId)).ToDictionaryAsync(u => u.UserId, u => new { u.Fullname, u.ImageUrl });

                var dutyDetailResults = details.Select(d => new ResponseModel.DutyDetailResultDto
                {
                    DutyDetailId = d.DutyDetailId,
                    UserId = d.UserId,
                    Name = users.GetValueOrDefault(d.UserId)?.Fullname ?? "",
                    UserImageUrl = users.GetValueOrDefault(d.UserId)?.ImageUrl ?? "",
                    Description = d.Description,
                    Deadline = d.Deadline,
                    CreatedDate = d.CreatedDate,
                    UpdatedDate = d.UpdatedDate,
                    //IsCompleted = d.IsCompleted
                    Status = d.Status.ToString()
                }).ToList();

                return new ResponseModel.DutyResultDto
                {
                    Id = existingDuty.Id,
                    Name = existingDuty.Name,
                    StartDate = existingDuty.StartDate,
                    EndDate = existingDuty.EndDate,
                    CreatedDate = existingDuty.CreatedDate,
                    UpdatedDate = existingDuty.UpdatedDate,
                    Note = existingDuty.Note,
                    //IsCompleted = existingDuty.IsCompleted,
                    Status = existingDuty.Status.ToString(),
                    AssignedBy = (await _context.Users.FindAsync(existingDuty.AssignedById))?.Fullname ?? "",
                    AssignImageUrl = (await _context.Users.FindAsync(existingDuty.AssignedById))?.ImageUrl ?? "",
                    DutyDetails = dutyDetailResults
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật Duty. Message: {Message}", ex.Message);
                throw;
            }
        }
        public async Task<ResponseModel.DutyDetailResultDto> UpdateDutyDetailAsync(ResponseModel.UpdateDutyDetailDto dto, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                var allDutyDetails = await _googleSheetHelper.GetAllDutyDetailsAsync();
                var existingDutyDetail = allDutyDetails.FirstOrDefault(d => d.DutyDetailId == dto.DutyDetailId && !d.IsDeleted);
                if (existingDutyDetail == null)
                    throw new ArgumentException("Không tìm thấy chi tiết công việc");

                var allDuties = await _googleSheetHelper.GetAllDutiesAsync();
                var relatedDuty = allDuties.FirstOrDefault(d => d.Id == existingDutyDetail.DutyId);
                if (relatedDuty == null)
                    throw new ArgumentException("Không tìm thấy công việc liên kết với chi tiết này");

                if (relatedDuty.IsDeleted)
                    throw new ArgumentException("Công việc chính đã bị xóa. Không thể cập nhật được chi tiết công việc");
                if (existingDutyDetail.Status == Enums.DutyStatus.Completed)
                    throw new ArgumentException("Công việc chi tiết đã hoàn thành, không thể cập nhật");

                User userToAssign = null;

                var isAdmin = currentUserRoles.Contains("Admin");
                var isManager = currentUserRoles.Contains("Manager");
                var isEmployee = currentUserRoles.Contains("Employee");

                if ((isAdmin || isManager) && dto.userId.HasValue)
                {
                    if (existingDutyDetail.Status != Enums.DutyStatus.Pending && !isAdmin)
                        throw new InvalidOperationException("Chỉ được gán người khi công việc ở trạng thái Pending");

                    userToAssign = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.userId.Value && (!u.IsDeleted || u.IsActive));
                    if (userToAssign == null)
                        throw new ArgumentException("Không tìm thấy người dùng");
                    if (userToAssign.Role != RoleType.Employee)
                        throw new ArgumentException("Chỉ nhân viên được phép gán vào công việc");

                    if (isAdmin && userToAssign.CompanyId != currentUser.CompanyId)
                        throw new UnauthorizedAccessException("Admin chỉ được gán nhân viên cùng công ty");
                    if (isManager && userToAssign.DepartmentId != currentUser.DepartmentId)
                        throw new UnauthorizedAccessException("Manager chỉ được gán nhân viên cùng phòng ban");

                    existingDutyDetail.UserId = dto.userId.Value;
                }

                if (isAdmin || (isManager && (existingDutyDetail.Status == Enums.DutyStatus.Pending || existingDutyDetail.Status == Enums.DutyStatus.InProgress)))
                {
                    if (!string.IsNullOrWhiteSpace(dto.Title)) existingDutyDetail.Title = dto.Title;
                    if (!string.IsNullOrWhiteSpace(dto.Description)) existingDutyDetail.Description = dto.Description;
                    if (dto.Deadline.HasValue)
                    {
                        if (dto.Deadline.Value < relatedDuty.StartDate)
                            throw new ArgumentException("Deadline không được trước ngày bắt đầu");
                        if (dto.Deadline.Value > relatedDuty.EndDate)
                            throw new ArgumentException("Deadline không được sau ngày kết thúc");
                        existingDutyDetail.Deadline = dto.Deadline.Value;
                    }
                }

                if (dto.Status.HasValue)
                {
                    if (existingDutyDetail.Status == Enums.DutyStatus.Completed && !isAdmin)
                        throw new InvalidOperationException("Không thể cập nhật trạng thái khi công việc đã hoàn thành");

                    if (isEmployee)
                    {
                        var oldStatus = (int)existingDutyDetail.Status;
                        var newStatus = (int)dto.Status.Value;

                        if (newStatus < oldStatus || newStatus > (int)Enums.DutyStatus.Completed)
                            throw new InvalidOperationException("Không được nhảy cóc hoặc lùi trạng thái");

                        if (oldStatus == (int)Enums.DutyStatus.Pending && newStatus != (int)Enums.DutyStatus.InProgress)
                            throw new InvalidOperationException("Từ Pending chỉ được chuyển sang InProgress");

                        if (oldStatus == (int)Enums.DutyStatus.InProgress && newStatus != (int)Enums.DutyStatus.Completed)
                            throw new InvalidOperationException("Từ InProgress chỉ được chuyển sang Completed");
                    }

                    existingDutyDetail.Status = dto.Status.Value;
                }

                existingDutyDetail.Note = dto.Note + " (Được cập nhật bởi " + currentUser.Fullname + ")";
                existingDutyDetail.UpdatedDate = vnNow;

                await _googleSheetHelper.UpdateDutyDetailRowAsync(existingDutyDetail);
                await _googleSheetHelper.UpdateDutyCompletionStatusAsync(existingDutyDetail.DutyId);

                var userToShow = userToAssign ?? await _context.Users.FirstOrDefaultAsync(u => u.UserId == existingDutyDetail.UserId);

                return new ResponseModel.DutyDetailResultDto
                {
                    DutyDetailId = existingDutyDetail.DutyDetailId,
                    UserId = existingDutyDetail.UserId,
                    Name = userToShow?.Fullname ?? "",
                    UserImageUrl = userToShow?.ImageUrl ?? "",
                    Title = existingDutyDetail.Title,
                    Description = existingDutyDetail.Description,
                    Deadline = existingDutyDetail.Deadline,
                    Status = existingDutyDetail.Status.ToString(),
                    CreatedDate = existingDutyDetail.CreatedDate,
                    UpdatedDate = existingDutyDetail.UpdatedDate,
                    CompletedDate = existingDutyDetail.CompletedDate,
                    Note = existingDutyDetail.Note
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật duty detail: {Message}", ex.Message);
                throw;
            }
        }


        public async Task<string> SoftDeleteDutyAsync(Guid dutyId, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                // Lấy danh sách Duty từ Google Sheets
                var allDuties = await _googleSheetHelper.GetAllDutiesAsync();
                var duty = allDuties.FirstOrDefault(d => d.Id == dutyId && !d.IsDeleted);
                if (duty == null)
                    throw new ArgumentException("Không thể tìm thấy công việc này " + dutyId);

                // Phân quyền
                if (currentUserRoles.Contains("Administrator"))
                {
                    if (currentUser.CompanyId != duty.CompanyId)
                        throw new UnauthorizedAccessException("Admin chỉ có thể xóa công việc trong công ty của mình");
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    if (duty.AssignedById != currentUserId)
                        throw new UnauthorizedAccessException("Manager chỉ có thể xóa công việc do mình tạo ra");

                    if (duty.Status == DutyStatus.InProgress || duty.Status == DutyStatus.Completed)
                        throw new InvalidOperationException("Manager không thể xóa công việc đã bắt đầu hoặc đã hoàn thành");
                }

                // Đánh dấu Duty là đã xóa mềm
                duty.IsDeleted = true;
                duty.UpdatedDate = vnNow;
                duty.Note = "Đã xóa mềm bởi " + currentUser.Fullname;

                await _googleSheetHelper.UpdateDutyRowAsync(new DutyResultDto
                {
                    Id = duty.Id,
                    Name = duty.Name,
                    StartDate = duty.StartDate,
                    EndDate = duty.EndDate,
                    CreatedDate = duty.CreatedDate,
                    UpdatedDate = duty.UpdatedDate,
                    AssignedBy = duty.AssignedById.ToString(),
                    CompanyId = duty.CompanyId ?? Guid.Empty,
                    //IsCompleted = duty.IsCompleted,
                    Status = duty.Status.ToString(),
                    IsDeleted = duty.IsDeleted,
                    Note = duty.Note
                });

                // Xóa mềm các DutyDetail liên quan
                var allDutyDetails = await _googleSheetHelper.GetAllDutyDetailsAsync();
                var relatedDetails = allDutyDetails.Where(d => d.DutyId == dutyId && !d.IsDeleted).ToList();

                foreach (var detail in relatedDetails)
                {
                    detail.IsDeleted = true;
                    await _googleSheetHelper.UpdateDutyDetailRowAsync(detail);
                }

                return "Xóa mềm công việc " + duty.Name;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa mềm Duty từ Google Sheets. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<string> SoftDeleteDutyDetailAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                // Lấy toàn bộ chi tiết công việc từ Google Sheets
                var allDutyDetails = await _googleSheetHelper.GetAllDutyDetailsAsync();

                var dutyDetail = allDutyDetails.FirstOrDefault(d => d.DutyDetailId == dutyDetailId && !d.IsDeleted);

                if (dutyDetail == null)
                    throw new ArgumentException("Không thể tìm thấy chi tiết công việc " + dutyDetailId);

                // Lấy duty tương ứng để kiểm tra quyền
                var allDuties = await _googleSheetHelper.GetAllDutiesAsync();
                var duty = allDuties.FirstOrDefault(d => d.Id == dutyDetail.DutyId && !d.IsDeleted);

                if (duty == null)
                    throw new ArgumentException("Không thể tìm thấy công việc tương ứng với chi tiết");

                var isManager = currentUserRoles.Contains("Manager");
                var isAdmin = currentUserRoles.Contains("Administrator");

                if (isAdmin)
                {
                    if (currentUser.CompanyId != duty.CompanyId)
                        throw new UnauthorizedAccessException("Admin chỉ được xóa chi tiết công việc trong công ty của mình");
                }
                else if (isManager)
                {
                    if (duty.AssignedById != currentUserId)
                        throw new UnauthorizedAccessException("Manager chỉ có thể xóa chi tiết công việc do mình tạo ra");

                    if (duty.Status == DutyStatus.InProgress || duty.Status == DutyStatus.Completed)
                        throw new InvalidOperationException("Manager không thể xóa chi tiết của công việc đã bắt đầu hoặc đã hoàn thành");
                }

                dutyDetail.IsDeleted = true;
                dutyDetail.UpdatedDate = vnNow;
                dutyDetail.Note = "Đã xóa mềm bởi " + currentUser.Fullname;

                await _googleSheetHelper.UpdateDutyDetailRowAsync(dutyDetail);

                await _googleSheetHelper.UpdateDutyCompletionStatusAsync(duty.Id);

                return "Đã xóa mềm chi tiết công việc " + dutyDetail.Title;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa mềm DutyDetail từ Google Sheets. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        //public async Task<string> MarkDutyDetailAsCompletedAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles)
        //{
        //    try
        //    {
        //        var allDetails = await _googleSheetHelper.GetAllDutyDetailsAsync();
        //        var existingDutyDetail = allDetails.FirstOrDefault(d => d.DutyDetailId == dutyDetailId && !d.IsDeleted);
        //        if (existingDutyDetail == null)
        //            throw new ArgumentException("Không tìm thấy chi tiết công việc");

        //        //if (existingDutyDetail.IsCompleted)
        //        if(existingDutyDetail.Status == Enums.DutyStatus.Completed)
        //            throw new ArgumentException("Công việc đã được đánh dấu hoàn thành trước đó");

        //        var allDuties = await _googleSheetHelper.GetAllDutiesAsync();
        //        var relatedDuty = allDuties.FirstOrDefault(d => d.Id == existingDutyDetail.DutyId && !d.IsDeleted);
        //        if (relatedDuty == null)
        //            throw new ArgumentException("Không tìm thấy công việc liên kết với chi tiết này");

        //        existingDutyDetail.Duty = relatedDuty;

        //        var now = DateTime.UtcNow;

        //        if (existingDutyDetail.Duty.EndDate < DateOnly.FromDateTime(now))
        //            throw new ArgumentException("Bạn đã quá trễ để hoàn thành công việc");

        //        var isEmployee = currentUserRoles.Contains("Employee");
        //        var isManager = currentUserRoles.Contains("Manager");
        //        var isAdmin = currentUserRoles.Contains("Administrator");

        //        if (isEmployee && existingDutyDetail.UserId != currentUserId)
        //            throw new UnauthorizedAccessException("Bạn chỉ có thể hoàn thành công việc của bản thân");

        //        if (isManager || isAdmin)
        //        {

        //            var currentUser = await _userRepository.GetUserInfoAsync(currentUserId);

        //            if (isManager)
        //            {
        //                if (existingDutyDetail.Duty.AssignedById != currentUserId)
        //                    throw new UnauthorizedAccessException("Manager chỉ được chỉnh sửa công việc do họ tạo");

        //                var assignedUser = await _userRepository.GetUserInfoAsync(existingDutyDetail.UserId);
        //                if (assignedUser.DepartmentId != currentUser.DepartmentId)
        //                    throw new UnauthorizedAccessException("Manager chỉ được thao tác với người cùng phòng ban");
        //            }

        //            if (isAdmin)
        //            {
        //                var assignedUser = await _userRepository.GetUserInfoAsync(existingDutyDetail.UserId);
        //                if (assignedUser.CompanyId != currentUser.CompanyId)
        //                    throw new UnauthorizedAccessException("Admin chỉ thao tác với người cùng công ty");
        //            }
        //        }

        //        //existingDutyDetail.IsCompleted = true;
        //        existingDutyDetail.Status = Enums.DutyStatus.Completed;
        //        await _googleSheetHelper.UpdateDutyDetailRowAsync(existingDutyDetail);
        //        await _googleSheetHelper.UpdateDutyCompletionStatusAsync(existingDutyDetail.DutyId);

        //        return "Đã hoàn thành công việc: " + existingDutyDetail.Description;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lỗi khi đánh dấu hoàn thành: {Message}", ex.Message);
        //        throw;
        //    }
        //}

    }
}
