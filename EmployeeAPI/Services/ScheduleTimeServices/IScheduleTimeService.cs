using EmployeeAPI.Base;
using EmployeeAPI.Models;
using static EmployeeAPI.Services.ScheduleTimeServices.ResponseModel;

namespace EmployeeAPI.Services.ScheduleTimeServices
{
    public interface IScheduleTimeService
    {
        Task<PagedResult<ResponseModel.ScheduleDto>> GetAllAsync(Guid? companyId, int? pageIndex, int? pageSize, Guid currentUserId, IList<string> currentUserRoles);
        Task<ScheduleDto?> GetScheduleTimeByIdAsync(Guid id, Guid currentUserId, IList<string> currentUserRoles);
        Task<ScheduleDto> UpdateScheduleTimeAsync(ScheduleTime scheduleTime, Guid currentUserID, IList<string> currentUserRoles);

    }
}
