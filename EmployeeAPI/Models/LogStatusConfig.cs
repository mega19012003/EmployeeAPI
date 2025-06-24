using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EmployeeAPI.Enums;

namespace EmployeeAPI.Models
{
    public class LogStatusConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // 👈 Bắt buộc để EF không tự tăng Id
        public int Id { get; set; } 

        public string Name { get; set; } = null!; 

        public double SalaryMultiplier { get; set; } 

        public string? Note { get; set; } 
    }
}
