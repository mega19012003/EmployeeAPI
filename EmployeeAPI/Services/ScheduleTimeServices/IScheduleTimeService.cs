using EmployeeAPI.Models;

namespace EmployeeAPI.Services.ScheduleTimeServices
{
    public interface IScheduleTimeService
    {
        Task<ScheduleTime> GetScheduleTimeAsync();
        Task UpdateScheduleTimeAsync(ScheduleTime scheduleTime);

    }
}
