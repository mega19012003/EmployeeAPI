namespace EmployeeAPI.Models
{
    public class Department
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool isDeleted { get; set; } = false;
        public ICollection<Staff> Staffs { get; set; } = new List<Staff>();
    }
}
