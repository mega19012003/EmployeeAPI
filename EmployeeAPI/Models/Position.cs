using System.Text.Json.Serialization;

namespace EmployeeAPI.Models
{
    public class Position
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; } = false;
        [JsonIgnore]
        public ICollection<Staff> Staffs { get; set; } = new List<Staff>();
    }
}
