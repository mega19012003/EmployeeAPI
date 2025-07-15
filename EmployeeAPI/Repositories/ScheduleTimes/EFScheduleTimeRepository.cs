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

        public async Task<IEnumerable<ScheduleTime>> GetAllAsync()
        {
            return _context.ScheduleTimes
                .AsNoTracking()
                .Include(c => c.Company);
        }

        public async Task<ScheduleTime?> GetTemplateAsync()
        {
            return await _context.ScheduleTimes.FirstOrDefaultAsync(x => x.IsSystemDefault);
        }

        public IQueryable<ScheduleTime> GetAll()
        {
            return _context.ScheduleTimes
                .AsNoTracking()
                .Include(c => c.Company);
        }

        public async Task<ScheduleTime?> GetScheduleTime(Guid id)
        {
            return await _context.ScheduleTimes.FirstOrDefaultAsync(p => p.id == id);
        }

        public async Task UpdateScheduleTime(ScheduleTime schedule)
        {
            _context.ScheduleTimes.Update(schedule);
            await _context.SaveChangesAsync();
        }
    }
}
