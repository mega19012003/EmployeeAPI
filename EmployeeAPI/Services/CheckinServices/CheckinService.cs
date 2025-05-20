using System.Runtime.CompilerServices;
using System.Transactions;
using Azure;
using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Checkins;
using EmployeeAPI.Repositories.Staffs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.CheckinServices.ResponseModel;
using static EmployeeAPI.Services.DepartmentServices.ResponseModel;

namespace EmployeeAPI.Services.CheckinServices
{
    public class CheckinService : ICheckinService
    {
        private readonly ICheckinRepository _checkinRepository;
        private readonly IStaffRepository _staffcheckinRepository;
        private readonly AppDbContext _context;

        public CheckinService(ICheckinRepository checkinRepository, IStaffRepository staffcheckinRepository, AppDbContext context)
        {
            _checkinRepository = checkinRepository;
            _staffcheckinRepository = staffcheckinRepository;
            _context = context;
        }

        /*public async Task<IEnumerable<ResponseModel.CheckinDto>> GetAllAsync(string? StaffName, int? pageIndex, int? pageSize)
        {
            if (pageSize == null || pageSize <= 0)
            {
                pageSize = 10;
            }
            if (pageIndex == null || pageIndex <= 0)
            {
                pageIndex = 1;
            }
            var result = await _checkinRepository.GetAllAsync(StaffName, pageIndex, pageSize);

            return result.Select(c => new ResponseModel.CheckinDto
            {
                CheckinId = c.Id,
                CheckinDate = c.CheckinDate,
                Status = c.Status,
                StaffId = c.StaffId,
                StaffName = c.Staff.Name,
            });
        }*/
        public async Task<PagedResult<ResponseModel.CheckinDto>> GetAllAsync(string? StaffName, int? pageIndex, int? pageSize)
        {
            pageIndex ??= 1;
            pageSize ??= 10;

            var query = _context.Checkins
                .Include(c => c.Staff)
                .Where(f => string.IsNullOrEmpty(StaffName) || f.Staff.Name.ToLower().Contains(StaffName.ToLower()))
                .Where(p => !p.IsDeleted);
            
            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .Select(c => new ResponseModel.CheckinDto
                {
                    CheckinId = c.Id,
                    CheckinDate = c.CheckinDate,
                    Status = c.Status,
                    StaffId = c.StaffId,
                    StaffName = c.Staff.Name,
                }).ToListAsync();

            return new PagedResult<ResponseModel.CheckinDto>
            {
                Items = items,
                PageIndex = pageIndex.Value,
                PageSize = pageSize.Value,
                TotalCount = totalCount
            };
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
                StaffName = c.Staff.Name,
            };
        }

        public async Task<ResponseModel.CheckinDto> CreateAsync(ResponseModel.CreateCheckin dto)
        {
            //using var transaction = await _context.Database.BeginTransactionAsync();
            /*try
            { */
            //var checkins = await _checkinRepository.GetAllAsync();
            var exists = await _checkinRepository.ExistAsync(dto.StaffId);
            if (exists)
                return null;
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
            /*await _context.SaveChangesAsync(); //nhớ xóa savechang trong repository

            await transaction.CommitAsync();*/
            var staff = await _staffcheckinRepository.GetByIdAsync(dto.StaffId);
            return new ResponseModel.CheckinDto
            {
                CheckinId = checkin.Id,
                CheckinDate = checkin.CheckinDate,
                Status = checkin.Status,
                StaffId = checkin.StaffId,
                StaffName = staff.Name,
            };
            /*}
            catch
            {
                await transaction.RollbackAsync();
                throw
            }*/
        }

        public async Task<ResponseModel.CheckinDto> UpdateAsync(ResponseModel.UpdateCheckin dto)
        {
 
            var existing = await _checkinRepository.GetByIdAsync(dto.CheckinId);
            if (existing == null) return null;

            existing.Id = dto.CheckinId;
            //existing.CheckinDate = dto.CheckinDate;
            existing.Status = dto.Status;
            //existing.StaffId = dto.StaffId;

            await _checkinRepository.UpdateAsync(existing);

            return new ResponseModel.CheckinDto
            {
                CheckinId = existing.Id,
                CheckinDate = existing.CheckinDate,
                Status = existing.Status,
                StaffId = existing.StaffId,
                StaffName = existing.Staff.Name,
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
                StaffName = p.Staff.Name,
            });
        }
    }
}
