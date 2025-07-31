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
using System.Reflection;
using static EmployeeAPI.Services.AuthServices.ResponseModel;
using static EmployeeAPI.Services.DutyServices.ResponseModel;
public class GoogleSheetHelper
{
    private readonly GoogleSheetSettings _settings;
    private readonly SheetsService _sheetsService;
    private readonly string _spreadsheetId;
    private readonly AppDbContext _context;
    private static DateTime _lastFetchTime = DateTime.MinValue;
    private static List<DutyDetail> _cachedDutyDetails;

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
        var range = "Duty!A2:K"; 
        var values = new List<IList<object>>
        {
            new List<object>
            {
                duty.Id.ToString(),
                duty.Name,
                duty.AssignedById.ToString(),
                duty.StartDate.ToString("yyyy-MM-dd"),
                duty.EndDate.ToString("yyyy-MM-dd"),
                //duty.IsCompleted.ToString(),
                duty.Status.ToString(),
                duty.IsDeleted.ToString(),
                duty.CompanyId.ToString(),
                duty.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                duty.UpdatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                duty.Note ?? ""
            }
        };

        var valueRange = new ValueRange { Values = values };

        var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, _settings.SpreadsheetId, range);
        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

        await appendRequest.ExecuteAsync();
    }
    public async Task AppendDutyDetailsAsync(List<DutyDetail> dutyDetails)
    {
        var range = "Detail!A2:L"; 
        var values = dutyDetails.Select(detail => new List<object>
        {
            detail.DutyDetailId.ToString(),
            detail.DutyId.ToString(),
            detail.UserId.ToString(),
            detail.Deadline,
            detail.Title,
            detail.Description,
            //detail.IsCompleted.ToString(),
            detail.Status.ToString(),
            detail.IsDeleted.ToString(),
            detail.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
            detail.UpdatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? null,
            detail.CompletedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? null,
            detail.Note ?? null
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
        //var isCompleted = bool.TryParse(row[5]?.ToString(), out var comp) && comp;
        var status = Enum.TryParse<DutyStatus>(row[5]?.ToString(), out var dutyStatus) ? dutyStatus : DutyStatus.NotStarted;
        var isDeleted = bool.TryParse(row[6]?.ToString(), out var del) && del;
        var companyId = Guid.TryParse(row[7]?.ToString(), out var compId) ? compId : Guid.Empty;
        var createdDate = DateTime.Parse(row[8]?.ToString() ?? DateTime.MinValue.ToString());
        var updatedDate = string.IsNullOrWhiteSpace(row.ElementAtOrDefault(9)?.ToString()) ? (DateTime?)null : DateTime.Parse(row[9].ToString());
        var note = row.ElementAtOrDefault(10)?.ToString();

        // Lọc các DutyDetail theo DutyId
        var dutyDetails = detailRows
            .Where(r => Guid.TryParse(r[1]?.ToString(), out var detailDutyId) && detailDutyId == dutyId)
            .Select(r => new
            {
                DutyDetailId = Guid.TryParse(r[0]?.ToString(), out var detailId) ? detailId : Guid.Empty,
                UserId = Guid.TryParse(r[2]?.ToString(), out var uid) ? uid : Guid.Empty,
                Deadline = DateOnly.Parse(r[3]?.ToString()),
                Title = r[4]?.ToString(),
                Description = r[5]?.ToString(),
                //IsCompleted = bool.TryParse(r[4]?.ToString(), out var comp2) && comp2
                Status = r[6]?.ToString(),
                CreatedDate = DateTime.Parse(r[8]?.ToString() ?? DateTime.MinValue.ToString()),
                UpdatedDate = string.IsNullOrWhiteSpace(r.ElementAtOrDefault(9)?.ToString()) ? (DateTime?)null : DateTime.Parse(r[9].ToString()),
                CompletedDate = string.IsNullOrWhiteSpace(r.ElementAtOrDefault(10)?.ToString()) ? (DateTime?)null : DateTime.Parse(r[10].ToString()),
                Note = r.ElementAtOrDefault(11)?.ToString()
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
            Deadline = d.Deadline,
            Title = d.Title,
            Description = d.Description,
            //IsCompleted = d.IsCompleted,
            Status = d.Status,
            Name = users.TryGetValue(d.UserId, out var user) ? user.Fullname : null,
            CreatedDate = d.CreatedDate,
            UpdatedDate = d.UpdatedDate,
            CompletedDate = d.CompletedDate,
            Note = d.Note,
        }).ToList();

        return new ResponseModel.DutyResultDto
        {
            Id = dutyId,
            Name = name,
            AssignedById = assignedById,
            AssignedBy = assignedById.ToString(), 
            StartDate = startDate,
            EndDate = endDate,
            //IsCompleted = isCompleted,
            Status = status.ToString(),
            CompanyId = companyId,
            CompanyName = companyId.ToString(), 
            DutyDetails = dutyDetailResults,
            CreatedDate = createdDate,
            UpdatedDate = updatedDate,
            Note = note
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
                Deadline = DateOnly.Parse(r[3]?.ToString()),
                Title = r[4]?.ToString(),
                Description = r[5]?.ToString(),
                //IsCompleted = bool.TryParse(r[4]?.ToString(), out var comp2) && comp2,
                Status = r[6]?.ToString(),
                CreatedDate = DateTime.Parse(r[8]?.ToString() ?? DateTime.MinValue.ToString()),
                UpdatedDate = string.IsNullOrWhiteSpace(r.ElementAtOrDefault(9)?.ToString()) ? (DateTime?)null : DateTime.Parse(r[9].ToString()),
                CompletedDate = string.IsNullOrWhiteSpace(r.ElementAtOrDefault(10)?.ToString()) ? (DateTime?)null : DateTime.Parse(r[10].ToString()),
                Note = r.ElementAtOrDefault(11)?.ToString()
            }).ToList();
    }
    public async Task<List<Duty>> GetAllDutiesAsync()
    {
        var range = $"Duty!A2:K"; 
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
                    //IsCompleted = bool.TryParse(row.ElementAtOrDefault(5)?.ToString(), out var completed) && completed,
                    Status = Enum.TryParse<DutyStatus>(row.ElementAtOrDefault(5)?.ToString() ?? "", out var status) ? status : DutyStatus.NotStarted,
                    IsDeleted = bool.TryParse(row.ElementAtOrDefault(6)?.ToString(), out var isDeleted) && isDeleted,
                    CompanyId = Guid.Parse(row.ElementAtOrDefault(7)?.ToString() ?? ""),
                    CreatedDate = DateTime.Parse(row.ElementAtOrDefault(8)?.ToString() ?? DateTime.MinValue.ToString()),
                    UpdatedDate = DateTime.TryParse(row.ElementAtOrDefault(9)?.ToString(), out var updated) ? updated : (DateTime?)null,
                    Note = row.ElementAtOrDefault(10)?.ToString() ?? ""
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
        var range = $"Detail!A2:L"; 
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
                    Deadline = DateOnly.Parse(row.ElementAtOrDefault(3)?.ToString()),
                    Title = row.ElementAtOrDefault(4)?.ToString() ?? "",
                    Description = row.ElementAtOrDefault(5)?.ToString() ?? "",
                    //IsCompleted = bool.TryParse(row.ElementAtOrDefault(4)?.ToString(), out var completed) && completed,
                    Status = Enum.TryParse<DutyStatus>(row.ElementAtOrDefault(6)?.ToString() ?? "", out var status) ? status : DutyStatus.NotStarted,
                    IsDeleted = bool.TryParse(row.ElementAtOrDefault(7)?.ToString(), out var deleted) && deleted,
                    CreatedDate = DateTime.Parse(row.ElementAtOrDefault(8)?.ToString() ?? DateTime.MinValue.ToString()),
                    UpdatedDate = DateTime.TryParse(row.ElementAtOrDefault(9)?.ToString(), out var updated) ? updated : (DateTime?)null,
                    CompletedDate = DateTime.TryParse(row.ElementAtOrDefault(10)?.ToString(), out var completed) ? completed : (DateTime?)null,
                    Note = row.ElementAtOrDefault(11)?.ToString() ?? ""
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

    public async Task<List<DutyDetail>> GetAllDutyDetailsWithDutiesCachedAsync()
    {
        // Nếu chưa cache hoặc đã quá 5 phút thì gọi lại
        if (_cachedDutyDetails == null || (DateTime.Now - _lastFetchTime).TotalMinutes > 5)
        {
            var dutyDetails = await GetAllDutyDetailsAsync();    
            var duties = await GetAllDutiesAsync();               

            var dutyDict = duties.ToDictionary(d => d.Id, d => d);

            foreach (var detail in dutyDetails)
            {
                if (dutyDict.TryGetValue(detail.DutyId, out var duty))
                {
                    detail.Duty = duty;
                }
            }

            _cachedDutyDetails = dutyDetails;
            _lastFetchTime = DateTime.Now;
        }

        return _cachedDutyDetails;
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
                    dutyDetail.Deadline.ToString("yyyy-MM-dd"),
                    dutyDetail.Title ?? "",
                    dutyDetail.Description,
                   // dutyDetail.IsCompleted.ToString(),
                    dutyDetail.Status.ToString(),
                    dutyDetail.IsDeleted.ToString(),
                    dutyDetail.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    dutyDetail.UpdatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    dutyDetail.CompletedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    dutyDetail.Note ?? ""
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

        var allDetails = await GetAllDutyDetailsAsync();
        var dutyDetails = allDetails.Where(d => d.DutyId == dutyId && !d.IsDeleted).ToList();

        //bool isCompleted = dutyDetails.Count > 0 && dutyDetails.All(d => d.IsCompleted);
        string status = dutyDetails.Count > 0 && dutyDetails.All(d => d.Status == DutyStatus.Completed) ? DutyStatus.Completed.ToString() : DutyStatus.InProgress.ToString();

        var range = $"Duty!A2:K"; 
        var request = _sheetsService.Spreadsheets.Values.Get(_settings.SpreadsheetId, range);
        var response = await request.ExecuteAsync();

        if (response.Values == null)
            return;

        for (int i = 0; i < response.Values.Count; i++)
        {
            var row = response.Values[i];
            if (row.Count > 0 && Guid.TryParse(row[0]?.ToString(), out var rowId) && rowId == dutyId)
            {
                var updateRange = $"Duty!F{(i + 2)}";
                var valueRange = new ValueRange
                {
                    //Values = new List<IList<object>> { new List<object> { isCompleted.ToString() } }
                    Values = new List<IList<object>> { new List<object> { status } }
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
        var range = "Duty!A2:K"; 
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
                var updateRange = $"Duty!A{i + 2}:K{i + 2}";
                var updatedRow = new List<IList<object>>
            {
                new List<object>
                {
                    duty.Id.ToString(),                    // A - Id
                    duty.Name ?? "",                       // B - Name
                    duty.AssignedBy ?? "",                 // C - AssignById
                    duty.StartDate.ToString("yyyy-MM-dd"), // D - StartDate
                    duty.EndDate.ToString("yyyy-MM-dd"),   // E - EndDate
                    //duty.IsCompleted.ToString().ToUpper(), // F - IsCompleted
                    duty.Status.ToString(),               // F - Status
                    duty.IsDeleted.ToString().ToUpper(),   // G - IsDeleted 
                    duty.CompanyId.ToString() ?? "",       // H - CompanyId
                    duty.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"), // I - CreatedDate
                    duty.UpdatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "", // J - UpdatedDate
                    duty.Note ?? ""                        // K - Note
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
        var range = "Detail!A2:L";
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
                rowIndex = i + 2; 
                break;
            }
        }

        if (rowIndex == -1)
        {
            throw new ArgumentException($"Không tìm thấy dòng với DutyDetailId = {dutyDetail.DutyDetailId}");
            //return;
        }

        var updateRange = $"Detail!A{rowIndex}:L{rowIndex}";
        var objectList = new List<object>
        {
            dutyDetail.DutyDetailId.ToString(),
            dutyDetail.DutyId.ToString(),
            dutyDetail.UserId.ToString(),
            dutyDetail.Deadline.ToString("yyyy-MM-dd"),
            dutyDetail.Title ?? "",
            dutyDetail.Description ?? "",
            // dutyDetail.IsCompleted.ToString().ToUpper(), 
            dutyDetail.Status.ToString(),
            dutyDetail.IsDeleted.ToString().ToUpper(),   
            dutyDetail.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
            dutyDetail.UpdatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
            dutyDetail.CompletedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
            dutyDetail.Note ?? ""
        };

        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { objectList }
        };

        var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, updateRange);
        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

        var updateResponse = await updateRequest.ExecuteAsync();
    }
}
