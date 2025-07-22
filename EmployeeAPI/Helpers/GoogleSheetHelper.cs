using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using EmployeeAPI.Services.DutyServices;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using static EmployeeAPI.Services.AuthServices.ResponseModel;
using static EmployeeAPI.Services.DutyServices.ResponseModel;
public class GoogleSheetHelper
{
    private readonly GoogleSheetSettings _settings;
    private readonly SheetsService _sheetsService;
    private readonly string _spreadsheetId;
    private readonly AppDbContext _context;

    //public GoogleSheetHelper(IOptions<GoogleSheetSettings> settings, AppDbContext context)
    //{
    //    _settings = settings.Value;

    //    var credential = GoogleCredential.FromFile(_settings.CredentialFilePath).CreateScoped(SheetsService.Scope.Spreadsheets);

    //    _sheetsService = new SheetsService(new BaseClientService.Initializer()
    //    {
    //        HttpClientInitializer = credential,
    //        ApplicationName = _settings.ApplicationName
    //    });

    //    _spreadsheetId = _settings.SpreadsheetId;
    //    _context = context;
    //}
    public GoogleSheetHelper(IOptions<GoogleSheetSettings> settings, AppDbContext context, IWebHostEnvironment env)
    {
        _settings = settings.Value;

        // Chuyển relative path sang absolute path
        var fullPath = Path.Combine(env.ContentRootPath, _settings.CredentialFilePath);

        // Kiểm tra có file không
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Không tìm thấy file Google Credential JSON", fullPath);
        }

        var credential = GoogleCredential
            .FromFile(fullPath)
            .CreateScoped(SheetsService.Scope.Spreadsheets);

        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = _settings.ApplicationName
        });

        _spreadsheetId = _settings.SpreadsheetId;
        _context = context;
    }
    public async Task<IList<IList<object>>> ReadSheetAsync(string range)
    {
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();
        return response.Values ?? new List<IList<object>>();
    }
    public async Task AppendDutyAsync(Duty duty)
    {
        var range = "Duty!A2:H"; // Giả sử hàng 1 là header
        var values = new List<IList<object>>
        {
            new List<object>
            {
                duty.Id.ToString(),
                duty.Name,
                duty.AssignedById.ToString(),
                duty.StartDate.ToString("yyyy-MM-dd"),
                duty.EndDate.ToString("yyyy-MM-dd"),
                duty.IsCompleted.ToString(),
                duty.IsDeleted.ToString(),
                duty.CompanyId.ToString()
            }
        };

        var valueRange = new ValueRange { Values = values };

        var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, _settings.SpreadsheetId, range);
        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

        await appendRequest.ExecuteAsync();
    }

    public async Task AppendDutyDetailsAsync(List<DutyDetail> dutyDetails)
    {
        var range = "Detail!A2:F"; // Giả sử hàng 1 là header
        var values = dutyDetails.Select(detail => new List<object>
        {
            detail.DutyDetailId.ToString(),
            detail.DutyId.ToString(),
            detail.UserId.ToString(),
            detail.Description,
            detail.IsCompleted.ToString(),
            detail.IsDeleted.ToString(),
        }).ToList<IList<object>>();

        var valueRange = new ValueRange
        {
            Values = values
        };

        var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, _settings.SpreadsheetId, range);
        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

        await appendRequest.ExecuteAsync();
    }

    public async Task<ResponseModel.DutyResultDto?> GetDutyByIdAsync(Guid id)
    {
        var dutyRows = await ReadSheetAsync("Duty");
        var detailRows = await ReadSheetAsync("Detail");

        var row = dutyRows.FirstOrDefault(r => Guid.TryParse(r[0]?.ToString(), out var did) && did == id);
        if (row == null) return null;

        var dutyId = Guid.Parse(row[0]?.ToString());
        var name = row[1]?.ToString();
        var assignedById = Guid.TryParse(row[2]?.ToString(), out var assignBy) ? assignBy : Guid.Empty;
        var startDate = DateOnly.Parse(row[3]?.ToString());
        var endDate = DateOnly.Parse(row[4]?.ToString());
        var isCompleted = bool.TryParse(row[5]?.ToString(), out var comp) && comp;
        var isDeleted = bool.TryParse(row[6]?.ToString(), out var del) && del;
        var companyId = Guid.TryParse(row[7]?.ToString(), out var compId) ? compId : Guid.Empty;

        // Lọc các DutyDetail theo DutyId
        var dutyDetails = detailRows
            .Where(r => Guid.TryParse(r[1]?.ToString(), out var detailDutyId) && detailDutyId == dutyId)
            .Select(r => new
            {
                DutyDetailId = Guid.TryParse(r[0]?.ToString(), out var detailId) ? detailId : Guid.Empty,
                UserId = Guid.TryParse(r[2]?.ToString(), out var uid) ? uid : Guid.Empty,
                Description = r[3]?.ToString(),
                IsCompleted = bool.TryParse(r[4]?.ToString(), out var comp2) && comp2
            })
            .ToList();

        // Truy vấn tất cả user trong dutyDetails 1 lần
        var userIds = dutyDetails.Select(d => d.UserId).Distinct().ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.UserId))
            .ToDictionaryAsync(u => u.UserId);

        // Map lại kết quả
        var dutyDetailResults = dutyDetails.Select(d => new ResponseModel.DutyDetailResultDto
        {
            DutyDetailId = d.DutyDetailId,
            UserId = d.UserId,
            Description = d.Description,
            IsCompleted = d.IsCompleted,
            Name = users.TryGetValue(d.UserId, out var user) ? user.Fullname : null
        }).ToList();

        return new ResponseModel.DutyResultDto
        {
            Id = dutyId,
            Name = name,
            AssignedById = assignedById,
            AssignedBy = assignedById.ToString(), // bạn có thể dùng _context.Users.FindAsync để lấy Fullname nếu cần
            StartDate = startDate,
            EndDate = endDate,
            IsCompleted = isCompleted,
            CompanyId = companyId,
            CompanyName = companyId.ToString(), // tương tự, có thể dùng _context.Companies.FindAsync để lấy tên
            DutyDetails = dutyDetailResults
        };
    }
    public async Task<List<ResponseModel.DutyDetailResultDto>> GetDutyDetailsByDutyIdAsync(Guid dutyId)
    {
        var detailRows = await ReadSheetAsync("Detail");

        return detailRows
            .Where(r => Guid.TryParse(r[1]?.ToString(), out var detailDutyId) && detailDutyId == dutyId)
            .Select(r => new ResponseModel.DutyDetailResultDto
            {
                DutyDetailId = Guid.TryParse(r[0]?.ToString(), out var detailId) ? detailId : Guid.Empty,
                UserId = Guid.TryParse(r[2]?.ToString(), out var uid) ? uid : Guid.Empty,
                Description = r[3]?.ToString(),
                IsCompleted = bool.TryParse(r[4]?.ToString(), out var comp2) && comp2,
                Name = r[2]?.ToString() // hoặc lấy tên từ DB nếu cần
            }).ToList();
    }
    public async Task<List<Duty>> GetAllDutiesAsync()
    {
        var range = $"Duty!A2:H"; // 7 cột: Id, Name, StartDate, EndDate, AssignedBy, CompanyId, IsCompleted, Is Deleted
        var request = _sheetsService.Spreadsheets.Values.Get(_settings.SpreadsheetId, range);
        var response = await request.ExecuteAsync();
        var values = response.Values;

        var result = new List<Duty>();
        if (values == null || values.Count == 0)
            return result;

        foreach (var row in values)
        {
            try
            {
                var duty = new Duty
                {
                    Id = Guid.Parse(row[0]?.ToString() ?? ""),
                    Name = row.ElementAtOrDefault(1)?.ToString() ?? "",
                    AssignedById = Guid.Parse(row.ElementAtOrDefault(2)?.ToString() ?? ""),
                    StartDate = DateOnly.Parse(row.ElementAtOrDefault(3)?.ToString() ?? DateOnly.MinValue.ToString()),
                    EndDate = DateOnly.Parse(row.ElementAtOrDefault(4)?.ToString() ?? DateOnly.MinValue.ToString()),
                    IsCompleted = bool.TryParse(row.ElementAtOrDefault(5)?.ToString(), out var completed) && completed,
                    IsDeleted = bool.TryParse(row.ElementAtOrDefault(6)?.ToString(), out var isDeleted) && isDeleted,
                    CompanyId = Guid.Parse(row.ElementAtOrDefault(7)?.ToString() ?? ""),
                };
                result.Add(duty);
            }
            catch
            {
                // Bỏ qua dòng lỗi
            }
        }

        return result;
    }

    public async Task<List<DutyDetail>> GetAllDutyDetailsAsync()
    {
        var range = $"Detail!A2:F"; 
        var request = _sheetsService.Spreadsheets.Values.Get(_settings.SpreadsheetId, range);
        var response = await request.ExecuteAsync();
        var values = response.Values;

        var result = new List<DutyDetail>();
        if (values == null || values.Count == 0)
            return result;

        foreach (var row in values)
        {
            try
            {
                var detail = new DutyDetail
                {
                    DutyDetailId = Guid.Parse(row[0]?.ToString() ?? ""),
                    DutyId = Guid.Parse(row[1]?.ToString() ?? ""),
                    UserId = Guid.Parse(row[2]?.ToString() ?? ""),
                    Description = row.ElementAtOrDefault(3)?.ToString() ?? "",
                    IsCompleted = bool.TryParse(row.ElementAtOrDefault(4)?.ToString(), out var completed) && completed,
                    IsDeleted = bool.TryParse(row.ElementAtOrDefault(5)?.ToString(), out var deleted) && deleted,
                };
                result.Add(detail);
            }
            catch
            {
                // Bỏ qua dòng lỗi
            }
        }

        return result;
    }

    public async Task AddDutyDetailAsync(DutyDetail dutyDetail)
    {
        var range = $"Detail!A2"; // Thêm vào cuối sheet
        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> {
                new List<object>
                {
                    dutyDetail.DutyDetailId.ToString(),
                    dutyDetail.DutyId.ToString(),
                    dutyDetail.UserId.ToString(),
                    dutyDetail.Description,
                    dutyDetail.IsCompleted.ToString(),
                    dutyDetail.IsDeleted.ToString(),
                }
            }
        };

        var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, _settings.SpreadsheetId, range);
        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        await appendRequest.ExecuteAsync();
    }

    public async Task UpdateDutyCompletionStatusAsync(Guid dutyId)
    {
        var duties = await GetAllDutiesAsync();
        var duty = duties.FirstOrDefault(d => d.Id == dutyId);
        if (duty == null)
            return;

        // Lấy tất cả DutyDetails thuộc duty này
        var allDetails = await GetAllDutyDetailsAsync();
        var dutyDetails = allDetails.Where(d => d.DutyId == dutyId && !d.IsDeleted).ToList();

        bool isCompleted = dutyDetails.Count > 0 && dutyDetails.All(d => d.IsCompleted);

        // Cập nhật ô IsCompleted trong Duty Sheet
        var range = $"Duty!A2:H"; // Đọc toàn bộ sheet để tìm dòng cần cập nhật
        var request = _sheetsService.Spreadsheets.Values.Get(_settings.SpreadsheetId, range);
        var response = await request.ExecuteAsync();

        if (response.Values == null)
            return;

        for (int i = 0; i < response.Values.Count; i++)
        {
            var row = response.Values[i];
            if (row.Count > 0 && Guid.TryParse(row[0]?.ToString(), out var rowId) && rowId == dutyId)
            {
                // Cập nhật cột IsCompleted (ví dụ cột G = index 6 nếu A=0)
                var updateRange = $"Duty!F{(i + 2)}";
                var valueRange = new ValueRange
                {
                    Values = new List<IList<object>> { new List<object> { isCompleted.ToString() } }
                };

                var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _settings.SpreadsheetId, updateRange);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                await updateRequest.ExecuteAsync();
                break;
            }
        }
    }

    public async Task UpdateDutyRowAsync(DutyResultDto duty)
    {
        var range = "Duty!A2:H"; // Dòng dữ liệu bắt đầu từ dòng 2
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();
        var rows = response.Values;

        if (rows == null)
            throw new Exception("Không thể lấy dữ liệu từ Google Sheet.");

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Count > 0 && Guid.TryParse(row[0]?.ToString(), out var rowId) && rowId == duty.Id)
            {
                var updateRange = $"Duty!A{i + 2}:H{i + 2}";
                var updatedRow = new List<IList<object>>
            {
                new List<object>
                {
                    duty.Id.ToString(),                    // A - Id
                    duty.Name ?? "",                       // B - Name
                    duty.AssignedBy ?? "",                 // C - AssignById
                    duty.StartDate.ToString("yyyy-MM-dd"), // D - StartDate
                    duty.EndDate.ToString("yyyy-MM-dd"),   // E - EndDate
                    duty.IsCompleted.ToString().ToUpper(), // F - IsCompleted
                    duty.IsDeleted.ToString().ToUpper(),   // G - IsDeleted 
                    duty.CompanyId.ToString() ?? ""       // H - CompanyId
                }
            };

                var valueRange = new ValueRange
                {
                    Range = updateRange,
                    Values = updatedRow
                };

                var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, updateRange);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

                await updateRequest.ExecuteAsync();
                return;
            }
        }

        throw new Exception("Không tìm thấy Duty để cập nhật trong Google Sheet.");
    }

    public async Task UpdateDutyDetailRowAsync(DutyDetail dutyDetail)
    {
        var range = "Detail!A2:F"; // Bỏ qua header
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();
        var values = response.Values;

        if (values == null || values.Count == 0)
        {
            Console.WriteLine("Không tìm thấy dữ liệu trong sheet Detail.");
            return;
        }

        int rowIndex = -1;

        for (int i = 0; i < values.Count; i++)
        {
            var row = values[i];
            if (row.Count > 0 && row[0]?.ToString() == dutyDetail.DutyDetailId.ToString())
            {
                rowIndex = i + 2; // +2 vì i=0 tương ứng dòng 2
                break;
            }
        }

        if (rowIndex == -1)
        {
            Console.WriteLine($"Không tìm thấy dòng với DutyDetailId = {dutyDetail.DutyDetailId}");
            return;
        }

        var updateRange = $"Detail!A{rowIndex}:F{rowIndex}";
        var objectList = new List<object>
        {
            dutyDetail.DutyDetailId.ToString(),
            dutyDetail.DutyId.ToString(),
            dutyDetail.UserId.ToString(),
            dutyDetail.Description ?? "",
            dutyDetail.IsCompleted.ToString().ToUpper(), // TRUE/FALSE
            dutyDetail.IsDeleted.ToString().ToUpper(),   // TRUE/FALSE
        };

        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { objectList }
        };

        var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, updateRange);
        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

        var updateResponse = await updateRequest.ExecuteAsync();

        Console.WriteLine($"Đã cập nhật thành công {updateResponse.UpdatedCells} ô tại {updateRange}");
    }
}
