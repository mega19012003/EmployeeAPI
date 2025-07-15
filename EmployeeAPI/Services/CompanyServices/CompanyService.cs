using EmployeeAPI.Base;
using EmployeeAPI.Models;
using EmployeeAPI.Repositories.Companies;
using EmployeeAPI.Repositories.LogStatusConfigs;
using EmployeeAPI.Repositories.ScheduleTimes;
using EmployeeAPI.Repositories.Users;
using EmployeeAPI.Services.ImageServices;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using static EmployeeAPI.Services.CompanyServices.ResponseModel;

namespace EmployeeAPI.Services.CompanyServices
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<CompanyService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly ICloudImageService _cloudinary;
        private readonly ILogStatusConfigRepository _logStatusConfigRepository;
        private readonly IScheduleTimeRepository _scheduleTimeRepository;
        public CompanyService(ICompanyRepository companyRepository, ICloudImageService cloudinary, ILogger<CompanyService> logger, IUserRepository userRepository, AppDbContext context, ILogStatusConfigRepository logStatusConfigRepository, IScheduleTimeRepository scheduleTimeRepository)
        {
            _companyRepository = companyRepository;
            _logger = logger;
            _userRepository = userRepository;
            _context = context;
            _cloudinary = cloudinary;
            _logStatusConfigRepository = logStatusConfigRepository;
            _scheduleTimeRepository = scheduleTimeRepository;
        }

        public async Task<PagedResult<CompanyResultDto>> GetAllCompaniesAsync(string? Name, int? pageIndex, int? pagesize)
        {
            try
            {
                pageIndex ??= 1;
                pagesize ??= 10;
                var companies = await _companyRepository.GetAllCompaniesAsync();
                if (!string.IsNullOrEmpty(Name))
                {
                    companies = companies.Where(c => c.Name.ToLower().Contains(Name.ToLower()) && !c.IsDeleted);
                }

                var totalCount = companies.Count();

                var items = companies
                    .Skip((pageIndex.Value - 1) * pagesize.Value)
                    .Take(pagesize.Value)
                    .Select(c => new CompanyResultDto
                    {
                        Name = c.Name,
                        Address = c.Address,
                        LogoUrl = c.LogoUrl,
                        CompanyId = c.Id
                    });

                return new PagedResult<CompanyResultDto>
                {
                    Items = items,
                    TotalCount = companies.Count(),
                    PageIndex = pageIndex.Value,
                    PageSize = pagesize.Value
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all companies");
                throw;
            }
        }

        public async Task<CompanyResultDto> GetCompanyByIdAsync(Guid companyId, Guid currentUserId, IList<string> curretnUserRole)
        {
            var isEmployee = curretnUserRole.Contains("Employee");
            var isManager = curretnUserRole.Contains("Manager");
            var isAdmin = curretnUserRole.Contains("Administrator");

            var currentUser = await _context.Users.FindAsync(currentUserId);

            if (isManager || isEmployee || isAdmin)
            {
                companyId = (Guid)currentUser.CompanyId;
            }

            var result = await _companyRepository.GetCompanyByIdAsync(companyId);

            return new CompanyResultDto
            {
                Name = result.Name,
                Address = result.Address,
                LogoUrl = result.LogoUrl,
                CompanyId = companyId
            };
        }

        public async Task<CompanyResultDto> CreateCompanyAsync(CreateCompanyDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrEmpty(dto.Name))
                {
                    throw new ArgumentException("Tên công ty không được để trống");
                }

                string image = null;
                if (dto.LogoUrl != null)
                {
                    image = await _cloudinary.UploadImageAsync(dto.LogoUrl);
                }

                var newCompany = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Address = dto.Address,
                    LogoUrl = image,
                    IsDeleted = false
                };

                await _companyRepository.AddCompanyAsync(newCompany);
                //await _context.SaveChangesAsync();

                // Clone LogStatusConfig
                var defaultConfigs = await _logStatusConfigRepository.GetTemplateAsync();

                var clonedConfigs = defaultConfigs.Select(x => new LogStatusConfig
                {
                    Id = Guid.NewGuid(),            
                    enumId = x.enumId,              
                    Name = x.Name,
                    SalaryMultiplier = x.SalaryMultiplier,
                    Note = x.Note,
                    CompanyId = newCompany.Id,
                    //CompanyName = newCompany.Name,
                    IsSystemDefault = false
                }).ToList();

                await _context.LogStatusConfigs.AddRangeAsync(clonedConfigs);
                //await _context.SaveChangesAsync();

                // Clone Schedule
                var defaultSchedule = await _scheduleTimeRepository.GetTemplateAsync();

                if (defaultSchedule != null)
                {
                    var companySchedule = new ScheduleTime
                    {
                        id = Guid.NewGuid(),
                        StartTimeMorning = defaultSchedule.StartTimeMorning,
                        EndTimeMorning = defaultSchedule.EndTimeMorning,
                        StartTimeAfternoon = defaultSchedule.StartTimeAfternoon,
                        EndTimeAfternoon = defaultSchedule.EndTimeAfternoon,
                        LogAllowtime = defaultSchedule.LogAllowtime,
                        IsSystemDefault = false,
                        CompanyId = newCompany.Id
                    };

                    await _context.ScheduleTimes.AddAsync(companySchedule);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CompanyResultDto
                {
                    Name = newCompany.Name,
                    Address = newCompany.Address,
                    LogoUrl = newCompany.LogoUrl,
                    CompanyId = newCompany.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding company and cloning log status configs");
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<CompanyResultDto> UpdateCompanyAsync(UpdateCompanyDto dto)
        {
            using var Transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingCompany = await _companyRepository.GetCompanyByIdAsync(dto.CompanyId);
                if (existingCompany == null)
                {
                    throw new ArgumentException("Không tìm thấy công ty");
                }

                if (!string.IsNullOrEmpty(dto.Address)) existingCompany.Address = dto.Address;
                //if (!string.IsNullOrEmpty(dto.LogoUrl)) existingCompany.LogoUrl = dto.LogoUrl;
                if (!string.IsNullOrEmpty(dto.Name)) existingCompany.Name = dto.Name;

                if( dto.LogoUrl != null )
                {
                    if(!string.IsNullOrEmpty(existingCompany.LogoUrl))
                    {
                        var oldLogoId = _cloudinary.ExtractPublicId(existingCompany.LogoUrl);
                        if(!string.IsNullOrEmpty(oldLogoId))
                            await _cloudinary.DeleteImageAsync(oldLogoId);
                    }

                    var uploadLogo = await _cloudinary.UploadImageAsync(dto.LogoUrl);
                    existingCompany.LogoUrl = uploadLogo;
                }

                await _companyRepository.UpdateCompanyAsync(existingCompany);
                await Transaction.CommitAsync();
                return new CompanyResultDto
                {
                    Name = existingCompany.Name,
                    Address = existingCompany.Address,
                    LogoUrl = existingCompany.LogoUrl,
                    CompanyId = existingCompany.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating company");
                await Transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<string> DeleteCompanyAsync(Guid companyId)
        {
            using var Transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var company = await _companyRepository.GetCompanyByIdAsync(companyId);
                if (company == null)
                {
                    throw new ArgumentException("Không tìm thấy công ty");
                }
                company.IsDeleted = true;
                await _companyRepository.UpdateCompanyAsync(company);
                await Transaction.CommitAsync();

                return "Xóa công ty " + company.Name + " thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting company");
                await Transaction.RollbackAsync();
                throw;
            }
        }
    }
}
