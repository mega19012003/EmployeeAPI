using EmployeeAPI.Models;
using EmployeeAPI.Repositories.ScheduleTimes;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Services.ScheduleTimeServices
{
    public class ScheduleTimeService : IScheduleTimeService
    {
        private readonly IScheduleTimeRepository _repository;
        private readonly AppDbContext _context;

        public ScheduleTimeService(IScheduleTimeRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }
        public async Task<ScheduleTime?> GetScheduleTimeAsync()
        {
            return await _repository.GetScheduleTime();
        }

        public async Task UpdateScheduleTimeAsync(ScheduleTime schedule)
        {
            var existing = await _repository.GetScheduleTime();
            if (existing == null)
            {
                _context.ScheduleTimes.Add(schedule);
            }
            else
            {
                existing.StartTime = schedule.StartTime;
                existing.LateThresholdMinutes = schedule.LateThresholdMinutes;
                existing.EndTime = schedule.EndTime;
                _context.ScheduleTimes.Update(existing);
            }

            await _context.SaveChangesAsync();
        }
    }
}
