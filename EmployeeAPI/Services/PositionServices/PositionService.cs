using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Positions;
using EmployeeAPI.Services.PositionServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using static EmployeeAPI.Services.PositionServices.ResponseModel;

namespace EmployeeAPI.Services.PositionServices
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _repository;

        public PositionService(IPositionRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ResponseModel.PositionDTO>> GetAllAsync()
        {
            var positions = await _repository.GetAllAsync();
            return positions.Select(p => new ResponseModel.PositionDTO
            {
                Id = p.Id,
                Name = p.Name,
                IsDeleted = p.IsDeleted
            });
        }

        public async Task<ResponseModel.PositionDTO> GetByIdAsync(Guid id)
        {
            var position = await _repository.GetByIdAsync(id);
            if (position == null) return null;

            return new ResponseModel.PositionDTO
            {
                Id = position.Id,
                Name = position.Name,
                IsDeleted = position.IsDeleted
            };
        }

        public async Task<ResponseModel.CreatePosition> AddAsync(string name)
        {
            var model = new Position
            {
                Id = Guid.NewGuid(),
                Name = name,
            };

            var entity = await _repository.AddAsync(model);
            return new ResponseModel.CreatePosition
            {
                PositionId = entity.Id,
                Name = entity.Name,
            };
        }

        public async Task<ResponseModel.UpdatePosition?> UpdateAsync(Guid id, string newName)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;

            entity.Name = newName;
            var updated = await _repository.UpdateAsync(entity);
            if (updated == null) return null; 

            return new ResponseModel.UpdatePosition
            {
                Id = updated.Id,
                Name = updated.Name,
                IsDeleted = updated.IsDeleted
            };
        }

        public async Task<string> SoftDeleteAsync(Guid id)
        {
            var entity = await _repository.SoftDeleteAsync(id);
            if (entity == null) return "Không tìm thấy vị trí";

            return "Đã xóa vị trí: " + id;
        }

        public async Task<ResponseModel.PositionDTO?> GetAllEmployee(string name)
        {
            var entity = await _repository.GetAllEmployee(name);
            if (entity == null) return null;

            return new ResponseModel.PositionDTO{
                Id = entity.Id, 
                Name = entity.Name 
            };
        }
    }
}
