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
                StartTimeMorning = result.StartTimeMorning,
                EndTimeMorning = result.EndTimeMorning,
                StartTimeAfternoon = result.StartTimeAfternoon,
                EndTimeAfternoon = result.EndTimeAfternoon,
                LateThresholdMinutes = result.LateThresholdMinutes,
                LogAllowtime = result.LogAllowtime,
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
                    existing.StartTimeMorning = newSchedule.StartTimeMorning;
                    existing.EndTimeMorning = newSchedule.EndTimeMorning;
                    existing.LateThresholdMinutes = newSchedule.LateThresholdMinutes;
                    existing.StartTimeAfternoon = newSchedule.StartTimeAfternoon;
                    existing.EndTimeAfternoon = newSchedule.EndTimeAfternoon;
                    existing.LogAllowtime = newSchedule.LogAllowtime;
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
