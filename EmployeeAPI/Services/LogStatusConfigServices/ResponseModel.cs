using EmployeeAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeAPI.Services.LogStatusConfigServices
{
    public class ResponseModel
    {
        public class LogStatusDto
        {
            public Guid Id { get; set; }
            public int enumId { get; set; }
            public string Name { get; set; } = null!;
            //public double SalaryMultiplier { get; set; }
            public string? Note { get; set; }
            public string? CompanyName { get; set; }
        }

        public class UpdateLogStatusDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = null!;
            //public double SalaryMultiplier { get; set; }
            public string? Note { get; set; }
        }
    }
}
