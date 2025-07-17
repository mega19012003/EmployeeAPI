using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.ScheduleTimes;
using EmployeeAPI.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.ScheduleTimeServices.ResponseModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EmployeeAPI.Services.ScheduleTimeServices
{
    public class ScheduleTimeService : IScheduleTimeService
    {
        private readonly IScheduleTimeRepository _scheduleRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<IScheduleTimeService> _logger;
        private readonly IUserRepository _userRepository;

        public ScheduleTimeService(IScheduleTimeRepository repository, AppDbContext context, ILogger<IScheduleTimeService> logger, IUserRepository userRepository)
        {
            _scheduleRepository = repository;
            _context = context;
            _userRepository = userRepository;
            _logger = logger;
        }


        public async Task<PagedResult<ResponseModel.ScheduleDto>> GetAllAsync(Guid? companyId, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles)
        {
            try
            {
                pageIndex ??= 1;
                pageSize ??= 10;

                var query = _scheduleRepository.GetAll();

                if (!currentUserRoles.Contains("SystemAdmin"))
                {
                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                    if (currentUser?.CompanyId == null)
                        throw new ArgumentException("Người dùng chưa có công ty.");

                    query = query.Where(s => s.CompanyId == currentUser.CompanyId);
                }
                else if (companyId.HasValue)
                {
                    query = query.Where(p => p.CompanyId == companyId);
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .Select(c => new ResponseModel.ScheduleDto
                    {
                        id = c.id,
                        StartTimeMorning = c.StartTimeMorning,
                        EndTimeMorning = c.EndTimeMorning,
                        LogAllowtime = c.LogAllowtime,
                        StartTimeAfternoon = c.StartTimeAfternoon,
                        EndTimeAfternoon = c.EndTimeAfternoon,
                        CompanyName = c.Company.Name,
                    }).ToListAsync();

                return new PagedResult<ResponseModel.ScheduleDto>
                {
                    Items = items,
                    PageIndex = pageIndex.Value,
                    PageSize = pageSize.Value,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving schedule. Message: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public async Task<ScheduleDto?> GetScheduleTimeByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles)
        {
            var result = await _scheduleRepository.GetScheduleTimeId(id);
            if (result == null)
                throw new ArgumentException("Không tìm thấy lịch làm việc.");

            if (!currentUserRoles.Contains("SystemAdmin"))
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (currentUser?.CompanyId == null)
                    throw new ArgumentException("Người dùng chưa có công ty.");

                if (result.CompanyId != currentUser.CompanyId)
                    throw new ArgumentException("Không có quyền truy cập vào thời gian làm việc của công ty khác.");
                id = (Guid)currentUser.CompanyId;
            }

            return new ScheduleDto
            {
                id = result.id,
                StartTimeMorning = result.StartTimeMorning,
                EndTimeMorning = result.EndTimeMorning,
                StartTimeAfternoon = result.StartTimeAfternoon,
                EndTimeAfternoon = result.EndTimeAfternoon,
                LogAllowtime = result.LogAllowtime,
                CompanyName = result.Company?.Name
            };
        }


        public async Task<ScheduleDto> UpdateScheduleTimeAsync(UpdateSchedule newSchedule, Guid currentUserID, IList<string> currentUserRoles)
        {
            using var trasaction = await _context.Database.BeginTransactionAsync();
            try {
                var existing = await _scheduleRepository.GetScheduleTimeId(newSchedule.id);
                if (existing == null)
                    throw new ArgumentException("Không tìm thấy thời gian làm việc");

                var currentUser = await _userRepository.GetActiveUserIdAsync(currentUserID);
                if (currentUser?.CompanyId == null) throw new ArgumentException("Người dùng chưa có công ty. Vui lòng liên hệ admin để cập nhật");
                if (currentUser?.CompanyId != existing.CompanyId) throw new ArgumentException("Chỉ được phép cập nhật thời gian làm việc của công ty");
                
                if (newSchedule.StartTimeMorning > newSchedule.EndTimeMorning || newSchedule.StartTimeMorning > newSchedule.StartTimeAfternoon || newSchedule.StartTimeMorning > newSchedule.EndTimeAfternoon)
                    throw new ArgumentException("Giờ bắt đầu buổi sáng không được lớn hơn giờ kết thúc buổi sáng, giờ bắt đầu/kết thúc buổi chiều");

                if (newSchedule.EndTimeMorning > newSchedule.StartTimeAfternoon || newSchedule.EndTimeMorning > newSchedule.EndTimeAfternoon)
                    throw new ArgumentException("Giờ kết thúc buổi sáng không được lớn hơn giờ bắt đầu/kết thúc buổi chiều");

                if (newSchedule.StartTimeAfternoon > newSchedule.EndTimeAfternoon)
                    throw new ArgumentException("Giờ bắt đầu buổi chiều không được lớn hơn giờ kết thúc buổi chiều");

                existing.StartTimeMorning = newSchedule.StartTimeMorning;
                existing.EndTimeMorning = newSchedule.EndTimeMorning;
                //existing.LateThresholdMinutes = newSchedule.LateThresholdMinutes;
                existing.StartTimeAfternoon = newSchedule.StartTimeAfternoon;
                existing.EndTimeAfternoon = newSchedule.EndTimeAfternoon;
                existing.LogAllowtime = newSchedule.LogAllowtime;
                _context.ScheduleTimes.Update(existing);


                await _context.SaveChangesAsync();
                await trasaction.CommitAsync();

                return new ResponseModel.ScheduleDto
                {
                    id = existing.id,
                    StartTimeMorning = existing.StartTimeMorning,
                    EndTimeMorning = existing.EndTimeMorning,
                    StartTimeAfternoon = existing.StartTimeAfternoon,
                    EndTimeAfternoon= existing.EndTimeAfternoon,
                    LogAllowtime = existing.LogAllowtime,
                    CompanyName = existing.Company.Name
                };
            }
            catch (Exception ex)
            {
                await trasaction.RollbackAsync();
                throw new ArgumentException(ex.Message);
            }
        }
    }
}
