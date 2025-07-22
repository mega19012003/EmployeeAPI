//using EmployeeAPI.Models;
//using EmployeeAPI.Services.DutyServices;
//using Microsoft.EntityFrameworkCore;

//namespace EmployeeAPI.Repositories.Duties
//{
//    public class EFDutyRepository : IDutyRepository
//    {
//        private readonly AppDbContext _context;
//        private readonly GoogleSheetHelper _googleSheetHelper;
//        public EFDutyRepository(AppDbContext context, GoogleSheetHelper googleSheetHelper)
//        {
//            _context = context;
//            _googleSheetHelper = googleSheetHelper;
//        }
//        public async Task<IEnumerable<Duty>> GetAllAsync()
//        {
//            return _context.Duties
//                .AsNoTracking()
//                .Include(d => d.DutyDetails)
//                .ThenInclude(dd => dd.Users)
//                .Where(p => !p.IsDeleted);
//        }

//        public IQueryable<Duty> GetAllQueryable()
//        {
//            return _context.Duties
//                .AsNoTracking()
//                .Include(d => d.DutyDetails/*.Where(dd => !dd.IsDeleted)*/)
//                .ThenInclude(dd => dd.Users)
//                .Where(p => !p.IsDeleted)
//                .AsQueryable();
//        }

//        public async Task<Duty> GetDutyByIdAsync(Guid id)
//        {
//            return await _context.Duties
//                .Include(p => p.AssignedBy)
//                .Include(p => p.Company)
//                .Include(p => p.DutyDetails.Where(dd => !dd.IsDeleted))
//                .ThenInclude(p => p.Users)
//                .Where(p => !p.IsDeleted)
//                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
//        }

//        public async Task<DutyDetail> GetDutyDetailByIdAsync(Guid id)
//        {
//            return await _context.DutyDetails
//                .Include(p => p.Users)
//                .Include(p => p.Duty)
//                .FirstOrDefaultAsync(p => p.DutyDetailId == id && !p.IsDeleted);
//        }


//        public async Task<Duty> AddAsync(Duty duty)
//        {
//                await _context.Duties.AddAsync(duty);
//                return duty;
//        }

//        public async Task<DutyDetail> AddDutyDetailAsync(DutyDetail detail)
//        {
//            await _context.DutyDetails.AddAsync(detail);
//            return detail;
//        }

//        public async Task UpdateDutyAsync(Duty duty)
//        {
//             _context.Duties.Update(duty);
//        }

//        public async Task UpdateDutyDetailAsync(DutyDetail detail)
//        {
//            _context.DutyDetails.Update(detail);
//        }

//        public async Task<bool> HasConflictAsync(List<Guid> userIds)
//        {
//            // Đọc toàn bộ dữ liệu từ sheet "DutyList"
//            var dutyDetails = await _googleSheetHelper.ReadSheetAsync("DutyList");

//            if (dutyDetails == null)
//                throw new Exception("Không thể đọc dữ liệu từ sheet DutyList");

//            foreach (var row in dutyDetails.Skip(1)) // Bỏ qua dòng tiêu đề
//            {
//                // Bỏ qua dòng rỗng hoặc dòng không đủ cột
//                if (row == null || row.Count < 13)
//                {
//                    Console.WriteLine($"⚠️ Dòng lỗi hoặc thiếu cột: {string.Join(" | ", row ?? new List<object>())}");
//                    continue;
//                }

//                var userIdStr = row[9]?.ToString();
//                var isDeletedStr = row[11]?.ToString();
//                var isCompletedStr = row[12]?.ToString();

//                // Parse dữ liệu từng cột
//                if (Guid.TryParse(userIdStr, out var userId) &&
//                    bool.TryParse(isDeletedStr, out var isDeleted) &&
//                    bool.TryParse(isCompletedStr, out var isCompleted))
//                {
//                    // Nếu userId nằm trong danh sách cần kiểm tra và chưa hoàn thành, chưa bị xóa → có conflict
//                    if (userIds.Contains(userId) && !isCompleted && !isDeleted)
//                    {
//                        return true;
//                    }
//                }
//                else
//                {
//                    Console.WriteLine($"⚠️ Không thể parse: UserId='{userIdStr}', IsDeleted='{isDeletedStr}', IsCompleted='{isCompletedStr}'");
//                }
//            }

//            // Không có conflict nào
//            return false;
//        }

//        public async Task<IEnumerable<Duty>> GetDutyByName()
//        {
//            return _context.Duties
//                .AsNoTracking()
//                .Include(p => p.DutyDetails)
//                .ThenInclude(p => p.Users)
//                .Where(p => !p.IsDeleted);
//        }
//    }
//}
