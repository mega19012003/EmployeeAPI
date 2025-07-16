using EmployeeAPI.Models;

namespace EmployeeAPI.Services.CompanyServices
{
    public class ResponseModel
    {
        public class CompanyResultDto
        {
            public Guid CompanyId { get; set; }
            public string? Name { get; set; }
            public string? Address { get; set; }
            public string? LogoUrl { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class CreateCompanyDto
        {
            //public Guid CompanyId { get; set; }
            public string? Name { get; set; }
            public string? Address { get; set; }
            public IFormFile? LogoUrl { get; set; }
            //public bool IsDeleted { get; set; }
        }

        public class UpdateCompanyDto
        {
            public Guid CompanyId { get; set; }
            public string? Name { get; set; }
            public string? Address { get; set; }
            public IFormFile? LogoUrl { get; set; }
            //public bool IsDeleted { get; set; }
        }
    }
}
