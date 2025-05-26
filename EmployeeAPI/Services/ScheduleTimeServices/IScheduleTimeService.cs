using EmployeeAPI.Models;

namespace EmployeeAPI.Services.ScheduleTimeServices
{
    public interface IScheduleTimeService
    {
        Task<ScheduleTime> GetScheduleTimeAsync();
        Task<ScheduleTime> UpdateScheduleTimeAsync(ScheduleTime scheduleTime);

    }
}
