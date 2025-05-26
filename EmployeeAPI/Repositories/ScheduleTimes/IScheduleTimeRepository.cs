using EmployeeAPI.Models;

namespace EmployeeAPI.Repositories.ScheduleTimes
{
    public interface IScheduleTimeRepository
    {
        Task<ScheduleTime> GetScheduleTime();
        Task UpdateScheduleTime(ScheduleTime scheduleTime);
    }
}
