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
            var result = await _repository.GetScheduleTime();
            return new ScheduleTime
            {
                StartTime = result.StartTime,
                EndTime = result.EndTime,
                LateThresholdMinutes = result.LateThresholdMinutes,
            };
        }

        public async Task<ScheduleTime> UpdateScheduleTimeAsync(ScheduleTime newSchedule)
        {
            using var trasaction = await _context.Database.BeginTransactionAsync();
            try {
                var existing = await _context.ScheduleTimes.FirstOrDefaultAsync();
                if (existing == null)
                {
                    newSchedule.id = Guid.NewGuid();
                    _context.ScheduleTimes.Add(newSchedule);
                }
                else
                {
                    existing.StartTime = newSchedule.StartTime;
                    existing.LateThresholdMinutes = newSchedule.LateThresholdMinutes;
                    existing.EndTime = newSchedule.EndTime;
                    _context.ScheduleTimes.Update(existing);
                }

                await _context.SaveChangesAsync();
                await trasaction.CommitAsync();

                return newSchedule;
            }
            catch
            {
                await trasaction.RollbackAsync();
                throw new ArgumentException("Invalid input data for schedule time update.");
            }
        }
    }
}
