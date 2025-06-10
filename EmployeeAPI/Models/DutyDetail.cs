using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EmployeeAPI.Base;
using Microsoft.EntityFrameworkCore;
namespace EmployeeAPI.Models
{
    public class DutyDetail //: BaseEntity
    {
        [Key]
        public Guid DutyDetailId { get; set; } = Guid.NewGuid();
        public Guid DutyId { get; set; }
        public Duty Duty { get; set; }
        public Guid UserId { get; set; }
        public User Users { get; set; }
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public string Description { get; set; }
        //public string note { get; set; } = string.Empty;
    }
}
