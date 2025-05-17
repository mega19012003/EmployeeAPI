using System.Runtime.CompilerServices;
using Azure;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.Staffs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;

namespace EmployeeAPI.Services.CheckinServices
{
    public class CheckinService : ICheckinService
    {
        private readonly ICheckinRepository _checkinRepository;
        private readonly IStaffRepository _staffcheckinRepository;

        public CheckinService(ICheckinRepository checkinRepository, IStaffRepository staffcheckinRepository)
        {
            _checkinRepository = checkinRepository;
            _staffcheckinRepository = staffcheckinRepository;
        }

        public async Task<IEnumerable<ResponseModel.CheckinDto>> GetAllAsync()
        {
            var checkins = await _checkinRepository.GetAllAsync();
            return checkins.Select(c => new ResponseModel.CheckinDto
            {
                CheckinId = c.Id,
                CheckinDate = c.CheckinDate,
                Status = c.Status,
                StaffId = c.StaffId,
            });
        }

        public async Task<ResponseModel.CheckinDto> GetByIdAsync(Guid id)
        {
            var c = await _checkinRepository.GetByIdAsync(id);
            if (c == null) return null;

            return new ResponseModel.CheckinDto
            {
                CheckinDate = c.CheckinDate,
                Status = c.Status,
                StaffId = c.StaffId,
            };
        }

        public async Task<ResponseModel.CheckinDto> CreateAsync(ResponseModel.CreateCheckin dto)
        {
            var checkins = await _checkinRepository.GetAllAsync();
            var exists = checkins.Any(c => c.StaffId == dto.StaffId && c.CheckinDate.Date == dto.CheckinDate.Date);
            /*var exists = await _checkinRepository.ExistsAsync(c =>
    c.StaffId == dto.StaffId && EF.Functions.DateDiffDay(c.CheckinDate, dto.CheckinDate) == 0);*/

            if (exists)
                return null;

            var existStaff = await _staffcheckinRepository.GetByIdAsync(dto.StaffId);
            if (existStaff == null)
                return null;

            var checkin = new Checkin
            {
                Id = Guid.NewGuid(),
                CheckinDate = dto.CheckinDate,
                Status = dto.Status,
                StaffId = dto.StaffId,
            };

            await _checkinRepository.CreateAsync(checkin);
            var staff = await _staffcheckinRepository.GetByIdAsync(dto.StaffId);
            return new ResponseModel.CheckinDto
            {
                CheckinId = checkin.Id,
                CheckinDate = checkin.CheckinDate,
                Status = checkin.Status,
                StaffId = checkin.StaffId,
            };
        }

        public async Task<ResponseModel.CheckinDto> UpdateAsync(ResponseModel.UpdateCheckin dto)
        {
            var existing = await _checkinRepository.GetByIdAsync(dto.Id);
            if (existing == null) return null;

            existing.CheckinDate = dto.CheckinDate;
            existing.Status = dto.Status;
            existing.StaffId = dto.StaffId;

            await _checkinRepository.UpdateAsync(existing);

            return new ResponseModel.CheckinDto
            {
                CheckinDate = existing.CheckinDate,
                Status = existing.Status,
                StaffId = existing.StaffId,
            };
        }

        public async Task<string> DeleteAsync(Guid id)
        {
            var deleted = await _checkinRepository.SoftDeleteAsync(id);
            if (deleted == null) return null;

            return "Đã xóa checkin: " + id;
        }

        public async Task<IEnumerable<CheckinDto>> GetCheckinByStaffAsync(Guid staffId)
        {
            var checkins = await _checkinRepository.GetCheckinByStaffAsync(staffId);
            return checkins.Select(p => new CheckinDto
            {
                CheckinId = p.Id,
                CheckinDate = p.CheckinDate,
                Status = p.Status,
                StaffId = p.StaffId,
            });
        }
    }
}
