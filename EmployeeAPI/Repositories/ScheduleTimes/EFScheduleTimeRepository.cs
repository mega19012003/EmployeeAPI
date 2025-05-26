using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.ScheduleTimes
{
    public class EFScheduleTimeRepository : IScheduleTimeRepository
    {
        private readonly AppDbContext _context;

        public EFScheduleTimeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ScheduleTime?> GetScheduleTime()
        {
            // Fix: Ensure the method is awaited on a queryable task, not a direct entity.
            return await _context.ScheduleTimes.FirstOrDefaultAsync();
        }

        public async Task UpdateScheduleTime(ScheduleTime schedule)
        {
            // Fix: Corrected the usage of _context to update the entity.
            _context.ScheduleTimes.Update(schedule);
            await _context.SaveChangesAsync();
        }
    }
}
