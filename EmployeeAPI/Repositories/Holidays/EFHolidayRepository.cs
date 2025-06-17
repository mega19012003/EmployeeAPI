using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Holidays
{
    public class EFHolidayRepository : IHolidayRepository
    {
        private readonly AppDbContext _context;

        public EFHolidayRepository(AppDbContext context)
        {
            _context = context;
        }
        public IQueryable<Holiday> GetAll()
        {
            return _context.Holidays.Where(h => !h.IsDeleted).AsQueryable();
        }
        public async Task<Holiday> GetByIdAsync(Guid id)
        {
            return await _context.Holidays
                .Where(h => h.Id == id && !h.IsDeleted)
                .FirstOrDefaultAsync();
        }
        public async Task<Holiday> CreateAsync(Holiday holiday)
        {
            _context.Holidays.Add(holiday);
            return holiday;
        }
        public async Task<Holiday> UpdateAsync(Holiday holiday)
        {
            _context.Holidays.Update(holiday);
            return holiday;
        }
        public async Task<Holiday> SoftDeleteAsync(Holiday holiday)
        {
            _context.Holidays.Update(holiday);
            return holiday;
        }
        public async Task<IEnumerable<Holiday>> GetAllAsync()
        {
            return await _context.Holidays.Where(h => !h.IsDeleted).ToListAsync();
        }
        public async Task<bool> IsHolidayAsync(DateTime utcNow)
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnDate = TimeZoneInfo.ConvertTimeFromUtc(utcNow, vnTimeZone);
            var targetDay = vnDate.Day;
            var targetMonth = vnDate.Month;

            return await _context.Holidays
                .Where(h => !h.IsDeleted)
                .AnyAsync(h =>
                    (h.startDate.Month < h.endDate.Month || h.startDate.Month == h.endDate.Month) &&
                    (
                        (h.startDate.Month == h.endDate.Month && h.startDate.Month == targetMonth && targetDay >= h.startDate.Day && targetDay <= h.endDate.Day)
                        ||
                        (h.startDate.Month < h.endDate.Month &&
                         (
                             (targetMonth == h.startDate.Month && targetDay >= h.startDate.Day) ||
                             (targetMonth == h.endDate.Month && targetDay <= h.endDate.Day) ||
                             (targetMonth > h.startDate.Month && targetMonth < h.endDate.Month)
                         )
                        )
                    )
                );
        }

    }
}
