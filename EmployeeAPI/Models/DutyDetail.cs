using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
namespace EmployeeAPI.Models
{
    public class DutyDetail
    {
        [Key]
        public Guid DutyDetailId { get; set; } = Guid.NewGuid();
        public Guid StaffId { get; set; }
        public Guid DutyId { get; set; }
        public Staff Staff { get; set; }
        public Duty Duty { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Description { get; set; }
    }
}
