using EmployeeAPI.Base;
using EmployeeAPI.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace EmployeeAPI.Models
{
    public class DutyDetail 
    {
        [Key]
        public Guid DutyDetailId { get; set; }
        public Guid DutyId { get; set; }
        public Duty Duty { get; set; }
        public Guid UserId { get; set; }
        public User Users { get; set; }
        public string Title { get; set; }
        public DateOnly Deadline { get; set; }
        public DutyStatus Status { get; set; } = DutyStatus.NotStarted;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Note { get; set; }
        public string Description { get; set; }
    }
}
