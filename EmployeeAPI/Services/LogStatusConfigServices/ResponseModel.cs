using EmployeeAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeAPI.Services.LogStatusConfigServices
{
    public class ResponseModel
    {
        public class LogStatusDto
        {
            //[DatabaseGenerated(DatabaseGeneratedOption.None)] // 👈 Bắt buộc để EF không tự tăng Id
            public Guid Id { get; set; }
            public int enumId { get; set; }
            public string Name { get; set; } = null!;
            public double SalaryMultiplier { get; set; }
            public string? Note { get; set; }

            public Guid? CompanyId { get; set; }
            public string? CompanyName { get; set; }
            public bool IsSystemDefault { get; set; }
        }
    }
}
