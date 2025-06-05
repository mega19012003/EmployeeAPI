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
            return await _context.ScheduleTimes.FirstOrDefaultAsync();
        }

        public async Task UpdateScheduleTime(ScheduleTime schedule)
        {
            _context.ScheduleTimes.Update(schedule);
            await _context.SaveChangesAsync();
        }
    }
}
