using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.ScheduleTimes
{
    public interface IScheduleTimeRepository
    {
        Task<IEnumerable<ScheduleTime>> GetAllAsync();
        Task<ScheduleTime?> GetTemplateAsync();
        IQueryable<ScheduleTime> GetAll();
        Task<ScheduleTime> GetScheduleTimeId(Guid id);
        Task UpdateScheduleTime(ScheduleTime scheduleTime);
    }
}
