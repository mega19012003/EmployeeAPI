using System.Text.Json.Serialization;

namespace EmployeeAPI.Models
{
    public class Position
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; } = false;
        [JsonIgnore]
        public ICollection<User> Users { get; set; } = new List<User>();
        public Department Department { get; set; }
        public Guid? DepartmentId { get; set; }
    }
}
