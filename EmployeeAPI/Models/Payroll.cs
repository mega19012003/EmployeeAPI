using System.ComponentModel.DataAnnotations.Schema;
using EmployeeAPI.Base;
using EmployeeAPI.Enums;

namespace EmployeeAPI.Models
{
    public class Payroll
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User Users { get; set; }
        public double Salary { get; set; }
        public DateTime CreatedDate { get; set; } 
        public string Note { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int DaysWorked { get; set; } 
    }
}
