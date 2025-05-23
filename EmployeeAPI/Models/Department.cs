namespace EmployeeAPI.Models
{
    public class Department
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool isDeleted { get; set; } = false;
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Position> Positions { get; set; } = new List<Position>();
    }
}
