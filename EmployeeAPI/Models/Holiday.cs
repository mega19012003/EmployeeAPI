namespace EmployeeAPI.Models
{
    public class Holiday
    {
        public Guid Id { get; set; }
        public string name { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
