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
        public async Task<IEnumerable<Duty>> GetAllAsync(int? pageSize, int? pageIndex, string? SearchTerm)
        {
                if (pageSize == null || pageSize <= 0)
                {
                    pageSize = 10;
                }
                if (pageIndex == null || pageIndex <= 0)
                {
                    pageIndex = 1;
                }
                var item = _context.Duties.Include(d=>d.DutyDetails).AsQueryable();
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    item = item.Where(m => m.Name.ToLower().Equals(SearchTerm.ToLower()));
                }
                var result = await item.Skip((int)(pageSize * (pageIndex - 1))).Take((int)pageSize).Where(p => !p.IsDeleted).ToListAsync();
                return result;
        }
        public async Task<Duty> GetByIdAsync(Guid id)
        {
            var result = await _context.Duties.Include(d => d.DutyDetails).FirstOrDefaultAsync(p => p.Id == id);
            if(result == null)
            {
                throw new Exception("Duty not found.");
            }
            return result;
        }
        public async Task<Duty> AddAsync(Duty duty)
        {
                await _context.Duties.AddAsync(duty);
                await _context.SaveChangesAsync();
                return duty;
        }
        /*public async Task<Duty> UpdateAsync(PutDutyDto dto)
        {
            try
            {
                var existingDuty = await _context.Duties.FindAsync(dto.Id);
                if (existingDuty == null)
                {
                    throw new Exception("Duty not found.");
                }
                existingDuty.Name = dto.Name;
                existingDuty.IsCompleted = dto.IsCompleted;
                existingDuty.DutyDetails = dto.DutyDetails.Select(d => new DutyDetail
                {
                    StaffId = d.StaffId,
                    Description = d.Description,
                }).ToList();

                _context.Duties.Update(existingDuty);
                await _context.SaveChangesAsync();
                return existingDuty;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while updating the duty: " + ex.Message, ex);
            }
        }*/
        public async Task<Duty> UpdateAsync(Duty duty)
        {
                var existingDuty = await _context.Duties.Include(d => d.DutyDetails).FirstOrDefaultAsync(p => p.Id == duty.Id && !p.IsDeleted);
                if (existingDuty == null)
                {
                    return null;
                }

                existingDuty.Name = duty.Name;
                existingDuty.IsCompleted = duty.IsCompleted;

                /*foreach (var dutyDetailDto in dto.DutyDetails)
                {
                    var existingDutyDetail = existingDuty.DutyDetails
                        .FirstOrDefault(dd => dd.DutyDetailId == dutyDetailDto.Id);

                    if (existingDutyDetail != null)
                    {
                        existingDutyDetail.StaffId = dutyDetailDto.StaffId;
                        existingDutyDetail.Description = dutyDetailDto.Description;
                    }
                    else
                    {
                        existingDuty.DutyDetails.Add(new DutyDetail
                        {
                            DutyDetailId = dutyDetailDto.Id,  
                            StaffId = dutyDetailDto.StaffId,
                            Description = dutyDetailDto.Description
                        });
                    }
                }*/

                //_context.Duties.Update(existingDuty);
                await _context.SaveChangesAsync();

                return existingDuty;
        }

        public async Task<Duty> SoftDeleteAsync(Guid id)
        {
           var entity = await _context.Duties.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (entity == null) return null;

            entity.IsDeleted = true;
            _context.Duties.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<IEnumerable<Duty>> GetDutyByName(string name, int? pageSize, int? pageIndex)
        {
            var query = _context.Duties.Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(f => f.Name.ToLower().Contains(name.ToLower()));
            }

            if (pageSize.HasValue && pageIndex.HasValue)
            {
                int skip = (pageIndex.Value - 1) * pageSize.Value;
                query = query.Skip(skip).Take(pageSize.Value);
            }
            return await query.ToListAsync();
        }
        public async Task<Duty> GetUnfinishedDuty(string status)
        {
            return null;// await _context.Duties.FirstOrDefaultAsync(d => d.Status == status);
        }
    }
}
