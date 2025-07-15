using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EmployeeAPI.Enums;

namespace EmployeeAPI.Models
{
    public class LogStatusConfig
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)] // 👈 Bắt buộc để EF không tự tăng Id
        public Guid Id { get; set; } 
        public int enumId { get; set; } 
        public string Name { get; set; } = null!; 

        public double SalaryMultiplier { get; set; } 

        public string? Note { get; set; }

        public Guid? CompanyId { get; set; }
        public bool IsSystemDefault { get; set; } // true=template, false=instance của công ty
        public Company Company { get; set; }
    }
}
