using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using EmployeeAPI.Models;

using EmployeeAPI.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.Design;
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

        public async Task<PagedResult<DutyResultDto>> GetAllAsync(Guid currentUserId, IList<string> currentUserRoles, string? name, Guid? companyId, int? Day, int? Month, int? Year, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            //var dutyRows = await _googleSheetHelper.ReadSheetAsync("Duty!A2:H");
            //var detailRows = await _googleSheetHelper.ReadSheetAsync("Detail!A2:F");

            var dutyRows = await _cache.GetOrCreateAsync("CachedDutyRows", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5); // Cache 5 phút
                return await _googleSheetHelper.ReadSheetAsync("Duty!A2:H");
            });

            var detailRows = await _cache.GetOrCreateAsync("CachedDetailRows", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5); // Cache 5 phút
                return await _googleSheetHelper.ReadSheetAsync("Detail!A2:F");
            });

            var users = await _context.Users.ToListAsync();
            var companies = await _context.Companies.ToListAsync();
            var currentUser = await _context.Users.FindAsync(currentUserId);

            var duties = dutyRows
                .Where(row => !string.IsNullOrWhiteSpace(row[0]?.ToString()))
                .Select(row => new
                {
                    Id = Guid.Parse(row[0].ToString()),
                    Name = row[1].ToString(),
                    AssignedById = Guid.Parse(row[2].ToString()),
                    StartDate = DateOnly.Parse(row[3].ToString()),
                    EndDate = DateOnly.Parse(row[4].ToString()),
                    IsCompleted = bool.Parse(row[5].ToString()),
                    IsDeleted = bool.Parse(row[6].ToString()),
                    CompanyId = string.IsNullOrEmpty(row[7].ToString()) ? (Guid?)null : Guid.Parse(row[7].ToString())
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
                    Description = row[3].ToString(),
                    IsCompleted = bool.Parse(row[4].ToString()),
                    IsDeleted = bool.TryParse(row[5]?.ToString(), out var deleted) && deleted
                })
                .Where(dd => !dd.IsDeleted) // ❗ Lọc bỏ detail bị xóa
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
                    d.IsCompleted,
                    d.CompanyId,
                    DutyDetails = dutyDetails
                        .Where(dd => dd.DutyId == d.Id)
                        .ToList()
                }).ToList();

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
                    IsCompleted = d.IsCompleted,
                    AssignedBy = users.FirstOrDefault(u => u.UserId == d.AssignedById)?.Fullname ?? "",
                    AssignImageUrl = users.FirstOrDefault(u => u.UserId == d.AssignedById)?.ImageUrl ?? "",
                    CompanyName = companies.FirstOrDefault(c => c.Id == d.CompanyId)?.Name ?? "",
                    DutyDetails = d.DutyDetails.Select(dd => new DutyDetailResultDto
                    {
                        DutyDetailId = dd.DutyDetailId,
                        UserId = dd.UserId,
                        Description = dd.Description,
                        Name = users.FirstOrDefault(u => u.UserId == dd.UserId)?.Fullname ?? "",
                        UserImageUrl = users.FirstOrDefault(u => u.UserId == dd.UserId)?.ImageUrl ?? "",
                        IsCompleted = dd.IsCompleted
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
            // Lấy từ Google Sheets thay vì DB
            var duty = await _googleSheetHelper.GetDutyByIdAsync(id); // trả về DutyDto
            if (duty.CompanyId == null || duty.CompanyId == Guid.Empty)
                throw new Exception("Thiếu CompanyId từ duty, kiểm tra dữ liệu Google Sheet.");
            var currentUser = await _context.Users.FindAsync(currentUserId); // vẫn lấy từ DB để xác định công ty/phòng ban
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

            // Lấy danh sách duty details từ Sheet
            var dutyDetails = await _googleSheetHelper.GetDutyDetailsByDutyIdAsync(id);

            if (currentUserRoles.Contains("Employee"))
            {
                var isAssignedToUser = dutyDetails.Any(dd => dd.UserId == currentUserId);
                if (!isAssignedToUser)
                    throw new UnauthorizedAccessException("Nhân viên không thể truy cập công việc của người khác");
            }

            // Mapping dữ liệu trả về
            var dutyResult = new ResponseModel.DutyResultDto
            {
                Id = duty.Id,
                Name = duty.Name,
                StartDate = duty.StartDate,
                EndDate = duty.EndDate,
                IsCompleted = duty.IsCompleted,
                AssignedBy = (await _context.Users.FindAsync(duty.AssignedById))?.Fullname,
                CompanyName = (await _context.Companies.FindAsync(duty.CompanyId))?.Name,
                DutyDetails = new List<ResponseModel.DutyDetailResultDto>()
            };

            foreach (var detail in dutyDetails)
            {
                var user = await _context.Users.FindAsync(detail.UserId);
                dutyResult.DutyDetails.Add(new ResponseModel.DutyDetailResultDto
                {
                    DutyDetailId = detail.DutyDetailId,
                    UserId = detail.UserId,
                    Description = detail.Description,
                    Name = user?.Fullname,
                    IsCompleted = detail.IsCompleted
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
                var description = row[3]?.ToString() ?? "";
                var isCompleted = bool.TryParse(row[4]?.ToString(), out var comp) && comp;
                var isDeleted = bool.TryParse(row[5]?.ToString(), out var del) && del;

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

                var assignedById = Guid.TryParse(matchingDutyRow[4]?.ToString(), out var assignBy) ? assignBy : Guid.Empty;
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

                // Lấy fullname từ userId
                var user = await _context.Users.FindAsync(userId);

                return new ResponseModel.DutyDetailResultDto
                {
                    DutyDetailId = dutyDetailId,
                    UserId = userId,
                    Description = description,
                    Name = user?.Fullname ?? "(Không tìm thấy tên)",
                    IsCompleted = isCompleted
                };
            }

            throw new ArgumentException("Không tìm thấy công việc chi tiết này");
        }

        public async Task<ResponseModel.DutyResultDto> AddDutyAsync(ResponseModel.CreateDutyDto dto, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                // --- 1. Kiểm tra người dùng hiện tại
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                var userIdsToAssign = dto.DutyDetails.Select(d => d.userId).ToList();

                // --- 2. Lấy thông tin người dùng được gán nhiệm vụ
                var assignedUsers = await _context.Users
                    .Where(u => userIdsToAssign.Contains(u.UserId))
                    .ToListAsync();

                // --- 3. Đọc dữ liệu DutyDetail từ Google Sheet để kiểm tra conflict
                var allDetailRows = await _googleSheetHelper.ReadSheetAsync("Detail");
                //var conflict = allDetailRows
                //    .Where(r =>
                //        Guid.TryParse(r[2]?.ToString(), out var uid) &&
                //        userIdsToAssign.Contains(uid) &&
                //        bool.TryParse(r[5]?.ToString(), out var isDeleted) && !isDeleted &&
                //        bool.TryParse(r[4]?.ToString(), out var isCompleted) && !isCompleted
                //    )
                //    .Select(r => Guid.Parse(r[2].ToString()))
                //    .FirstOrDefault();

                // --- 4. Kiểm tra trạng thái người dùng
                if (assignedUsers.Any(u => u.IsDeleted || !u.IsActive))
                    throw new ArgumentException("Không tìm thấy người dùng hoặc người dùng đã bị vô hiệu hóa");

                // --- 5. Logic phân quyền theo role
                if (currentUserRoles.Contains("Administrator"))
                {
                    if (currentUser.CompanyId == null)
                        throw new ArgumentException("Admin chưa có công ty. Vui lòng liên hệ System Admin để cập nhật công ty");

                    if (assignedUsers.Any(u => u.Role != RoleType.Employee))
                        throw new ArgumentException("Chỉ nhân viên được phép gán vào 1 công việc");

                    if (assignedUsers.Any(u => u.CompanyId != currentUser.CompanyId))
                        throw new ArgumentException("Admin chỉ được chọn nhân viên cùng công ty để thực hiện công việc");

                    //if (conflict != Guid.Empty)
                    //    throw new InvalidOperationException("Một hoặc nhiều nhân viên đang được gán cho công việc khác chưa hoàn thành");
                }

                if (currentUserRoles.Contains("Manager"))
                {
                    if (currentUser.DepartmentId == null)
                        throw new ArgumentException("Manager chưa có phòng ban. Vui lòng liên hệ Admin để cập nhật phòng ban");

                    if (assignedUsers.Any(u => u.Role != RoleType.Employee))
                        throw new ArgumentException("Chỉ nhân viên được phép gán vào 1 công việc");

                    if (assignedUsers.Any(u => u.DepartmentId != currentUser.DepartmentId))
                        throw new ArgumentException("Manager chỉ được chọn nhân viên cùng phòng ban để thực hiện công việc");

                    //if (conflict != Guid.Empty)
                    //    throw new InvalidOperationException("Một hoặc nhiều nhân viên đang được gán cho công việc khác chưa hoàn thành");
                }

                // --- 6. Kiểm tra ngày tháng hợp lệ
                if (dto.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
                    throw new ArgumentException("Ngày bắt đầu không được trước ngày hiện tại");

                if (dto.StartDate > dto.EndDate)
                    throw new ArgumentException("Ngày bắt đầu không được sau ngày kết thúc");

                // --- 7. Tạo đối tượng Duty
                var duty = new Duty
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    AssignedById = currentUserId,
                    CompanyId = (Guid)currentUser.CompanyId!,
                    IsCompleted = false,
                    IsDeleted = false
                };

                // --- 8. Tạo danh sách DutyDetails
                var dutyDetails = dto.DutyDetails.Select(d => new DutyDetail
                {
                    DutyDetailId = Guid.NewGuid(),
                    DutyId = duty.Id,
                    UserId = d.userId,
                    Description = d.Description,
                    IsDeleted = false,
                    IsCompleted = false
                }).ToList();

                duty.DutyDetails = dutyDetails;

                // --- 9. Ghi vào Google Sheets
                await _googleSheetHelper.AppendDutyAsync(duty);
                await _googleSheetHelper.AppendDutyDetailsAsync(dutyDetails);

                // --- 10. Trả kết quả
                return new ResponseModel.DutyResultDto
                {
                    Id = duty.Id,
                    Name = duty.Name,
                    StartDate = duty.StartDate,
                    EndDate = duty.EndDate,
                    IsCompleted = duty.IsCompleted,
                    AssignedById = currentUser.UserId,
                    AssignedBy = currentUser.Fullname,
                    CompanyId = duty.CompanyId?? Guid.Empty,
                    CompanyName = currentUser.Company?.Name ?? duty.CompanyId.ToString(),
                    DutyDetails = dutyDetails.Select(d => new ResponseModel.DutyDetailResultDto
                    {
                        DutyDetailId = d.DutyDetailId,
                        UserId = d.UserId,
                        Description = d.Description,
                        IsCompleted = d.IsCompleted,
                        Name = assignedUsers.FirstOrDefault(u => u.UserId == d.UserId)?.Fullname
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
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                // Lấy Duty từ Google Sheets
                var duty = await _googleSheetHelper.GetDutyByIdAsync(dutyId);
                if (duty == null)
                    throw new Exception("Không tìm thấy công việc");

                var userIdsToAssign = dto.DutyDetails.Select(d => d.userId).ToList();
                var assignedUsers = await _context.Users
                    .Where(u => userIdsToAssign.Contains(u.UserId))
                    .ToListAsync();

                if (assignedUsers.Any(u => u.IsDeleted || !u.IsActive))
                    throw new ArgumentException("Không tìm thấy người dùng hợp lệ");

                // Lấy tất cả DutyDetails từ Google Sheets
                var allDutyDetails = await _googleSheetHelper.GetAllDutyDetailsAsync();
                //var conflict = allDutyDetails
                //    .FirstOrDefault(dd =>
                //        userIdsToAssign.Contains(dd.UserId) &&
                //        !dd.IsDeleted &&
                //        !dd.IsCompleted);

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
                    var newDetail = new DutyDetail
                    {
                        DutyDetailId = Guid.NewGuid(),
                        UserId = detailDto.userId,
                        DutyId = dutyId,
                        Description = detailDto.Description,
                        IsCompleted = false,
                        IsDeleted = false
                    };

                    await _googleSheetHelper.AddDutyDetailAsync(newDetail);
                }

                // Cập nhật trạng thái hoàn thành của công việc nếu cần
                await _googleSheetHelper.UpdateDutyCompletionStatusAsync(dutyId);

                // Lấy lại duty từ sheet (đã có DutyDetail mới)
                var updatedDuty = await _googleSheetHelper.GetDutyByIdAsync(dutyId);

                return new ResponseModel.DutyResultDto
                {
                    Id = updatedDuty.Id,
                    Name = updatedDuty.Name,
                    StartDate = updatedDuty.StartDate,
                    EndDate = updatedDuty.EndDate,
                    AssignedBy = currentUser.Fullname,
                    CompanyName = (await _context.Companies.FindAsync(updatedDuty.CompanyId))?.Name ?? "",
                    DutyDetails = updatedDuty.DutyDetails.Select(d => new ResponseModel.DutyDetailResultDto
                    {
                        DutyDetailId = d.DutyDetailId,
                        UserId = d.UserId,
                        Description = d.Description,
                        IsCompleted = d.IsCompleted,
                        Name = assignedUsers.FirstOrDefault(u => u.UserId == d.UserId)?.Fullname
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
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                // Lấy duty từ Google Sheets
                var existingDuty = await _googleSheetHelper.GetDutyByIdAsync(dto.Id);
                if (existingDuty == null)
                    throw new ArgumentException("Không tìm thấy công việc");

                // Phân quyền
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

                if (dto.StartDate > dto.EndDate)
                    throw new ArgumentException("Ngày bắt đầu không được để sau ngày kết thúc");

                // Cập nhật thông tin
                existingDuty.Name = dto.Name;
                existingDuty.StartDate = dto.StartDate;
                existingDuty.EndDate = dto.EndDate;

                // Cập nhật lên Google Sheet
                await _googleSheetHelper.UpdateDutyRowAsync(existingDuty);

                // Lấy lại danh sách duty detail từ Google Sheet
                var allDutyDetails = await _googleSheetHelper.GetAllDutyDetailsAsync();
                var details = allDutyDetails.Where(d => d.DutyId == dto.Id).ToList();

                // Load thông tin user cho từng detail
                var userIds = details.Select(d => d.UserId).Distinct().ToList();
                var users = await _context.Users.Where(u => userIds.Contains(u.UserId)).ToDictionaryAsync(u => u.UserId, u => u.Fullname);

                var dutyDetailResults = details.Select(d => new ResponseModel.DutyDetailResultDto
                {
                    DutyDetailId = d.DutyDetailId,
                    UserId = d.UserId,
                    Name = users.GetValueOrDefault(d.UserId),
                    Description = d.Description,
                    IsCompleted = d.IsCompleted
                }).ToList();

                // Trả về
                return new ResponseModel.DutyResultDto
                {
                    Id = existingDuty.Id,
                    Name = existingDuty.Name,
                    StartDate = existingDuty.StartDate,
                    EndDate = existingDuty.EndDate,
                    IsCompleted = existingDuty.IsCompleted,
                    AssignedBy = (await _context.Users.FindAsync(existingDuty.AssignedById))?.Fullname,
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
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    throw new ArgumentException("Không thể tìm thấy user hiện tại");

                // Lấy danh sách tất cả DutyDetail từ Google Sheet
                var allDutyDetails = await _googleSheetHelper.GetAllDutyDetailsAsync();

                var existingDutyDetail = allDutyDetails.FirstOrDefault(d => d.DutyDetailId == dto.DutyDetailId && !d.IsDeleted);
                if (existingDutyDetail == null)
                    throw new ArgumentException("Không tìm thấy chi tiết công việc");

                //// Lấy danh sách tất cả Duties từ Google Sheet
                var allDuties = await _googleSheetHelper.GetAllDutiesAsync();
                var relatedDuty = allDuties.FirstOrDefault(d => d.Id == existingDutyDetail.DutyId);
                if (relatedDuty == null)
                    throw new ArgumentException("Không tìm thấy công việc liên kết với chi tiết này");

                var userToAssign = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.userId && (!u.IsDeleted || u.IsActive));
                if (userToAssign == null)
                    throw new ArgumentException("Không tìm thấy người dùng");
                if (userToAssign.Role != RoleType.Employee)
                    throw new ArgumentException("Chỉ nhân viên được phép gán vào 1 công việc");

                if (currentUserRoles.Contains("Admin"))
                {
                    if (userToAssign.CompanyId != currentUser.CompanyId)
                        throw new UnauthorizedAccessException("Admin chỉ được chọn nhân viên cùng công ty để thực hiện công việc");
                }
                else if (currentUserRoles.Contains("Manager"))
                {
                    if (relatedDuty.AssignedById != currentUserId)
                        throw new UnauthorizedAccessException("Manager chỉ có thể chỉnh sửa công việc do họ tạo ra");

                    if (userToAssign.DepartmentId != currentUser.DepartmentId)
                        throw new UnauthorizedAccessException("Manager chỉ được chọn nhân viên cùng phòng ban để thực hiện công việc");
                }

                // Cập nhật nội dung
                existingDutyDetail.UserId = dto.userId;
                existingDutyDetail.Description = dto.Description;

                // Ghi lại lên Google Sheets
                await _googleSheetHelper.UpdateDutyDetailRowAsync(existingDutyDetail);

                return new ResponseModel.DutyDetailResultDto
                {
                    DutyDetailId = existingDutyDetail.DutyDetailId,
                    UserId = existingDutyDetail.UserId,
                    Name = userToAssign.Fullname,
                    Description = existingDutyDetail.Description,
                    IsCompleted = existingDutyDetail.IsCompleted
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the duty detail in Google Sheets. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public async Task<string> MarkDutyDetailAsCompletedAsync(Guid dutyDetailId, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                var allDetails = await _googleSheetHelper.GetAllDutyDetailsAsync();
                var existingDutyDetail = allDetails.FirstOrDefault(d => d.DutyDetailId == dutyDetailId && !d.IsDeleted);
                if (existingDutyDetail == null)
                    throw new ArgumentException("Không tìm thấy chi tiết công việc");

                if (existingDutyDetail.IsCompleted)
                    throw new ArgumentException("Công việc đã được đánh dấu hoàn thành trước đó");

                var allDuties = await _googleSheetHelper.GetAllDutiesAsync();
                var relatedDuty = allDuties.FirstOrDefault(d => d.Id == existingDutyDetail.DutyId && !d.IsDeleted);
                if (relatedDuty == null)
                    throw new ArgumentException("Không tìm thấy công việc liên kết với chi tiết này");

                existingDutyDetail.Duty = relatedDuty;

                var now = DateTime.UtcNow;

                if (existingDutyDetail.Duty.EndDate < DateOnly.FromDateTime(now))
                    throw new ArgumentException("Bạn đã quá trễ để hoàn thành công việc");

                var isEmployee = currentUserRoles.Contains("Employee");
                var isManager = currentUserRoles.Contains("Manager");
                var isAdmin = currentUserRoles.Contains("Administrator");

                if (isEmployee && existingDutyDetail.UserId != currentUserId)
                    throw new UnauthorizedAccessException("Bạn chỉ có thể hoàn thành công việc của bản thân");

                if (isManager || isAdmin)
                {

                    var currentUser = await _userRepository.GetUserInfoAsync(currentUserId);

                    if (isManager)
                    {
                        if (existingDutyDetail.Duty.AssignedById != currentUserId)
                            throw new UnauthorizedAccessException("Manager chỉ được chỉnh sửa công việc do họ tạo");

                        var assignedUser = await _userRepository.GetUserInfoAsync(existingDutyDetail.UserId);
                        if (assignedUser.DepartmentId != currentUser.DepartmentId)
                            throw new UnauthorizedAccessException("Manager chỉ được thao tác với người cùng phòng ban");
                    }

                    if (isAdmin)
                    {
                        var assignedUser = await _userRepository.GetUserInfoAsync(existingDutyDetail.UserId);
                        if (assignedUser.CompanyId != currentUser.CompanyId)
                            throw new UnauthorizedAccessException("Admin chỉ thao tác với người cùng công ty");
                    }
                }

                existingDutyDetail.IsCompleted = true;
                await _googleSheetHelper.UpdateDutyDetailRowAsync(existingDutyDetail);

                // (Optional) Cập nhật lại trạng thái Duty nếu tất cả detail đã xong
                await _googleSheetHelper.UpdateDutyCompletionStatusAsync(existingDutyDetail.DutyId);

                return "Đã hoàn thành công việc: " + existingDutyDetail.Description;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu hoàn thành: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<string> SoftDeleteDutyAsync(Guid dutyId, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
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
                }
                else
                {
                    throw new UnauthorizedAccessException("Chỉ Admin hoặc Manager được phép xóa công việc");
                }

                // Đánh dấu Duty là đã xóa mềm
                duty.IsDeleted = true;
                await _googleSheetHelper.UpdateDutyRowAsync(new DutyResultDto
                {
                    Id = duty.Id,
                    Name = duty.Name,
                    StartDate = duty.StartDate,
                    EndDate = duty.EndDate,
                    AssignedBy = duty.AssignedById.ToString(),
                    CompanyId = duty.CompanyId ?? Guid.Empty,
                    IsCompleted = duty.IsCompleted,
                    IsDeleted = duty.IsDeleted
                });

                // Xóa mềm các DutyDetail liên quan
                var allDutyDetails = await _googleSheetHelper.GetAllDutyDetailsAsync();
                var relatedDetails = allDutyDetails.Where(d => d.DutyId == dutyId && !d.IsDeleted).ToList();

                foreach (var detail in relatedDetails)
                {
                    detail.IsDeleted = true;
                    await _googleSheetHelper.UpdateDutyDetailRowAsync(detail);
                }

                return "Xóa mềm công việc \"" + duty.Name + "\" thành công";
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
                }
                else
                {
                    throw new UnauthorizedAccessException("Chỉ Admin hoặc Manager được phép xóa chi tiết công việc");
                }

                // Đánh dấu là đã xóa mềm
                dutyDetail.IsDeleted = true;
                await _googleSheetHelper.UpdateDutyDetailRowAsync(dutyDetail);

                // Cập nhật trạng thái hoàn thành của Duty nếu cần
                await _googleSheetHelper.UpdateDutyCompletionStatusAsync(duty.Id);

                return "Đã xóa mềm chi tiết công việc " + dutyDetail.DutyDetailId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa mềm DutyDetail từ Google Sheets. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

    }
}
