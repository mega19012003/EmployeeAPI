using System.ComponentModel.DataAnnotations;

namespace EmployeeAPI.Models
{
    public class AllowedIP
    {
        [Key]
        public Guid AllowedIPId { get; set; }
        public string IPAddress { get; set; }

        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
