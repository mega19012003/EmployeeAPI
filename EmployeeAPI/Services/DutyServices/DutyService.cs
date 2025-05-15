using System.Reflection.Metadata.Ecma335;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Duties;
using EmployeeAPI.Repositories.Staffs;
using EmployeeAPI.Services.FileServices;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Services.DutyServices
{
    public class DutyService : IDutyService
    {
        private readonly IDutyRepository _dutyRepository;
        public DutyService(IDutyRepository dutyRepository)
        {
            _dutyRepository = dutyRepository;
        }

        public async Task<IEnumerable<ResponseModel.DutyDto>> GetAllAsync(int? pageSize, int? pageIndex, string? SearchTerm)
        {
            var results = await _dutyRepository.GetAllAsync(pageSize, pageIndex, SearchTerm);
            if (results == null)
            {
                return null;
            }
            return results.Select(p => new ResponseModel.DutyDto
            {
                Id = p.Id,
                Name = p.Name,
                IsCompleted = p.IsCompleted,
                DutyDetails = p.DutyDetails.Select(d => new ResponseModel.DutyDetailDto
                {
                    StaffId = d.StaffId,
                    Description = d.Description
                }).ToList()
            });
        }


        public async Task<ResponseModel.DutyDto> GetByIdAsync(Guid id)
        {
            var results = await _dutyRepository.GetByIdAsync(id);
            if (results == null)
            {
                return null;
            }
            return new ResponseModel.DutyDto
            {
                Id = id,
                Name = results.Name,
                IsCompleted = results.IsCompleted,
                DutyDetails = results.DutyDetails.Select(d => new ResponseModel.DutyDetailDto
                {
                    StaffId = d.StaffId,
                    Description = d.Description
                }).ToList()
            };
        }

        public async Task<ResponseModel.CreateDuty> AddAsync(ResponseModel.CreateDuty dto)
        {
            /*var result = await _dutyRepository.AddAsync(dto);
            if (result == null)
            {
                return null;
            }*/
            var duty = new Duty
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                DutyDetails = (List<DutyDetail>)dto.DutyDetails.Select(d => new DutyDetail
                {
                    StaffId = d.StaffId,
                    Description = d.Description
                }).ToList()
            };

            var created = await _dutyRepository.AddAsync(duty);
            //await _dutyRepository.SaveChangesAsync();
            return new ResponseModel.CreateDuty
            {
                Name = created.Name,
                StartDate = created.StartDate,
                DutyDetails = created.DutyDetails.Select(d => new ResponseModel.DutyDetailDto
                {
                    StaffId = d.StaffId,
                    Description = d.Description
                }).ToList()
            };
        }


        public async Task<ResponseModel.UpdateDuty> UpdateAsync(ResponseModel.UpdateDuty dto)
        {
            var existingDuty = await _dutyRepository.GetByIdAsync(dto.Id);
            if (existingDuty == null)
            {
                return null;
            }
            existingDuty.Name = dto.Name;
            existingDuty.IsCompleted = dto.IsCompleted;
            existingDuty.DutyDetails = dto.DutyDetails.Select(d => new DutyDetail
            {
                StaffId = d.StaffId,
                Description = d.Description
            }).ToList();

            var result = await _dutyRepository.UpdateAsync(existingDuty);
            if (result == null)
            {
                return null;
            }

            return new ResponseModel.UpdateDuty 
            {
                Id = result.Id,
                Name = dto.Name,
                IsCompleted = dto.IsCompleted,
                DutyDetails = result.DutyDetails.Select(d => new ResponseModel.UpdateDutyDetail
                {
                    Id = d.DutyDetailId,
                    StaffId = d.StaffId,
                    Description = d.Description
                }).ToList(),
            };
        }

        public async Task<ResponseModel.DeleteDuty> SoftDeleteAsync(Guid Id)
        {
            /*var existingDuty = await _dutyRepository.GetByIdAsync(Id); ///Get id ????

            if (existingDuty == null)
                return null;
            return new ResponseModel.DeleteDuty
            {
                Id = existingDuty.Id,
                Name = existingDuty.Name,
                IsDeleted = existingDuty.IsDeleted,
            };*/

            var entity = await _dutyRepository.SoftDeleteAsync(Id);
            if (entity == null) return null;

            return new ResponseModel.DeleteDuty
            {
                Id = entity.Id,
                IsDeleted = entity.IsDeleted
            };
        }


        /*public async Task<IEnumerable<Staff>> GetDutyByName(string name, int? pageSize, int? pageIndex)
        {
            var query = _dutyRepository.Where(p => p.IsActive == true && !p.IsDeleted);

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
        }*/

        
    }
}
