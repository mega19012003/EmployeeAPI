using System.Reflection.Metadata.Ecma335;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Duties;
using EmployeeAPI.Services.FileServices;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Services.DutyServices
{
    public class DutyService : IDutyService
    {
        private readonly IDutyRepository _dutyRepository;
        private readonly AppDbContext _context;
        public DutyService(IDutyRepository dutyRepository, AppDbContext context)
        {
            _dutyRepository = dutyRepository;
            _context = context;
        }

        public async Task<PagedResult<ResponseModel.DutyDto>> GetAllAsync(string? name, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _context.Duties
                .Where(f => string.IsNullOrEmpty(name) || f.Name.ToLower().Contains(name.ToLower()))
                .Where(p => !p.IsDeleted);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(f => new ResponseModel.DutyDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    IsCompleted = f.IsCompleted,
                    StartDate = f.StartDate,
                    DutyDetails = f.DutyDetails.Select(d => new ResponseModel.DutyDetailDto
                    {
                        DutyDetailId = d.DutyDetailId,
                        userId = d.UserId,
                        Name = d.Users.Fullname,
                        Description = d.Description
                    }).ToList()
                })
                .ToListAsync();

            return new PagedResult<ResponseModel.DutyDto>
            {
                TotalCount = totalCount,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                Items = items
            };
        }

        public async Task<ResponseModel.DutyDto> GetByIdAsync(Guid id)
        {
            var results = await _dutyRepository.GetByIdAsync(id);
            if (results == null)
                throw new ArgumentException("Cannot find duty id");

            return new ResponseModel.DutyDto
            {
                Id = id,
                Name = results.Name,
                IsCompleted = results.IsCompleted,
                StartDate = results.StartDate,
                DutyDetails = results.DutyDetails.Select(d => new ResponseModel.DutyDetailDto
                {
                    DutyDetailId = d.DutyDetailId,
                    userId = d.UserId,
                    Description = d.Description,
                    Name = d.Users.Fullname ?? "ko tồn tại",
                }).ToList()
            };
        }

        public async Task<ResponseModel.CreateDuty> AddAsync(ResponseModel.CreateDuty dto)
        {
            var duty = new Duty
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                StartDate = DateTime.Now,
                DutyDetails = (List<DutyDetail>)dto.DutyDetails.Select(d => new DutyDetail
                {
                    UserId = d.userId,
                    Description = d.Description
                }).ToList()
            };

            var created = await _dutyRepository.AddAsync(duty);
            return new ResponseModel.CreateDuty
            {
                Name = created.Name,
                StartDate = created.StartDate,
                DutyDetails = created.DutyDetails.Select(d => new ResponseModel.CreateDutyDetail
                {
                    userId = d.UserId,
                    Description = d.Description
                }).ToList()
            };
        }

        public async Task<ResponseModel.DutyDto> UpdateAsync(ResponseModel.UpdateDuty dto)
        {
            var existingDuty = await _dutyRepository.GetByIdAsync(dto.Id);
            if (existingDuty == null)
                throw new ArgumentException("Duty not found");

            var existingStaff = await _context.Users
                .Where(s => dto.DutyDetails.Any(d => d.userId == s.UserId))
                .AsNoTracking()
                .ToListAsync();
            if (existingStaff == null)
                throw new ArgumentException("Staff not found");

            var existingDutyDetails = await _context.DutyDetail
                .Where(d => dto.DutyDetails.Any(dd => dd.Id == d.DutyDetailId))
                .AsNoTracking()
                .ToListAsync();
            if (existingDutyDetails == null)
                throw new ArgumentException("DutyDetail not found");

            existingDuty.Name = dto.Name;
            existingDuty.IsCompleted = dto.IsCompleted;
            existingDuty.DutyDetails = dto.DutyDetails.Select(d => new DutyDetail
            {
                DutyDetailId = d.Id,
                UserId = d.userId,
                Description = d.Description
            }).ToList();

            var result = await _dutyRepository.UpdateAsync(existingDuty);
            if (result == null)
            {
                return null;
            }

            return new ResponseModel.DutyDto
            {
                Id = result.Id,
                Name = dto.Name,
                IsCompleted = dto.IsCompleted,
                StartDate = result.StartDate,
                DutyDetails = result.DutyDetails.Select(d => new ResponseModel.DutyDetailDto
                {
                    DutyDetailId = d.DutyDetailId,
                    userId = d.UserId,
                    Description = d.Description
                }).ToList(),
            };
        }

        public async Task<string> SoftDeleteAsync(Guid Id)
        {
            var entity = await _dutyRepository.SoftDeleteAsync(Id);
            if (entity == null)
                throw new ArgumentException("Cannot find duty id");

            return "Đã xóa công việc" + entity.Name;
        }
    }
}
