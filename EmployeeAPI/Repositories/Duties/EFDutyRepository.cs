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
        public async Task<IEnumerable<Duty>> GetAllAsync()
        {
            return _context.Duties
                .AsNoTracking()
                .Include(d => d.DutyDetails)
                .ThenInclude(dd => dd.Users)
                .Where(p => !p.IsDeleted);
        }

        public IQueryable<Duty> GetAllQueryable()
        {
            return _context.Duties
                .AsNoTracking()
                .Include(d => d.DutyDetails/*.Where(dd => !dd.IsDeleted)*/)
                .ThenInclude(dd => dd.Users)
                .Where(p => !p.IsDeleted)
                .AsQueryable();
        }

        public async Task<Duty> GetDutyByIdAsync(Guid id)
        {
            return await _context.Duties
                .Include(p => p.AssignedBy)
                .Include(p => p.DutyDetails.Where(dd => !dd.IsDeleted))
                .ThenInclude(p => p.Users)
                .Where(p => !p.IsDeleted)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<DutyDetail> GetDutyDetailByIdAsync(Guid id)
        {
            return await _context.DutyDetails
                .Include(p => p.Users)
                .Include(p => p.Duty)
                .FirstOrDefaultAsync(p => p.DutyDetailId == id && !p.IsDeleted);
        }


        public async Task<Duty> AddAsync(Duty duty)
        {
                await _context.Duties.AddAsync(duty);
                return duty;
        }

        public async Task<DutyDetail> AddDutyDetailAsync(DutyDetail detail)
        {
            await _context.DutyDetails.AddAsync(detail);
            return detail;
        }

        public async Task UpdateDutyAsync(Duty duty)
        {
             _context.Duties.Update(duty);
        }

        public async Task UpdateDutyDetailAsync(DutyDetail detail)
        {
            _context.DutyDetails.Update(detail);
        }

        //public async Task<Duty> SoftDeleteDutyAsync(Guid id)
        //{
        //    return await _context.Duties
        //        .Include(p => p.AssignedBy)
        //         .Include(p => p.DutyDetails)
        //         .ThenInclude(p => p.Users)
        //         .Where(p => !p.IsDeleted)
        //         .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        //}

        //public async Task<DutyDetail> SoftDeleteDutyDetailAsync(Guid id)
        //{
        //    return await _context.DutyDetail
        //        .Include(p => p.Users)
        //        .Include(p => p.Duty)
        //        .FirstOrDefaultAsync(p => p.DutyDetailId == id && !p.IsDeleted);
        //}

        public async Task<IEnumerable<Duty>> GetDutyByName()
        {
            return _context.Duties
                .AsNoTracking()
                .Include(p => p.DutyDetails)
                .ThenInclude(p => p.Users)
                .Where(p => !p.IsDeleted);
        }
    }
}
