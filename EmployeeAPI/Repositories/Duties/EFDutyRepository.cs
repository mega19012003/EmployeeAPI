using EmployeeAPI.Models;
using EmployeeAPI.Services.DutyServices;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Duties
{
    public class EFDutyRepository : IDutyRepository
    {
        private readonly AppDbContext _context;
        public EFDutyRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Duty>> GetAllAsync(string? SearchTerm, int? pageSize, int? pageIndex)
        {
            return _context.Duties
                .AsNoTracking()
                .Include(d => d.DutyDetails)
                .ThenInclude(dd => dd.Users)
                .AsQueryable();
        }

        public async Task<Duty> GetDutyByIdAsync(Guid id)
        {
            return await _context.Duties
                .AsNoTracking()
                .Include(p => p.DutyDetails)
                .ThenInclude(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<DutyDetail> GetDutyDetailByIdAsync(Guid id)
        {
            return await _context.DutyDetail
                .AsNoTracking()
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.DutyDetailId == id);
        }

        public async Task<Duty> AddAsync(Duty duty)
        {
                await _context.Duties.AddAsync(duty);
                await _context.SaveChangesAsync();
                return duty;
        }

        public async Task<Duty> UpdateDutyAsync(Duty duty)
        {
            var existingDuty = await _context.Duties.Include(d => d.DutyDetails).FirstOrDefaultAsync(p => p.Id == duty.Id && !p.IsDeleted);
            //if (existingDuty == null) return null;
            //existingDuty.Name = duty.Name;
            //existingDuty.IsCompleted = duty.IsCompleted;

            //await _context.SaveChangesAsync();

            return existingDuty;
        }

        public async Task<DutyDetail> UpdateDutyDetailAsync(DutyDetail duty)
        {
            var existingDuty = await _context.DutyDetail.FirstOrDefaultAsync(p => p.DutyDetailId == duty.DutyDetailId);
            //if (existingDuty == null) return null;
            //existingDuty.Name = duty.Name;
            //existingDuty.IsCompleted = duty.IsCompleted;

            //await _context.SaveChangesAsync();

            return existingDuty;
        }

        public async Task<Duty> SoftDeleteDutyAsync(Guid id)
        {
           var entity = await _context.Duties.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            //if (entity == null) return null;

            //entity.IsDeleted = true;
            //_context.Duties.Update(entity);
            //await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<DutyDetail> SoftDeleteDutyDetailAsync(Guid id)
        {
            var entity = await _context.DutyDetail.FirstOrDefaultAsync(p => p.DutyDetailId == id && !p.IsDeleted);
            //if (entity == null) return null;

            //entity.IsDeleted = true;
            //_context.Duties.Update(entity);
            //await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<Duty>> GetDutyByName(string name, int? pageSize, int? pageIndex)
        {
            return _context.Duties
                .AsNoTracking()
                .Include(p => p.DutyDetails)
                .ThenInclude(p => p.Users)
                .Where(p => !p.IsDeleted);
        }
    }
}
