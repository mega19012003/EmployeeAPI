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

        public async Task<string> SoftDeleteAsync(Guid Id)
        {
            var entity = await _dutyRepository.SoftDeleteAsync(Id);
            if (entity == null) return null;

            return "Đã xóa công việc" + Id;
        }
    }
}
