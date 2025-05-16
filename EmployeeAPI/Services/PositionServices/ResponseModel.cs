namespace EmployeeAPI.Services.PositionServices
{
    public class ResponseModel
    {
        public class PositionDTO
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public bool IsDeleted { get; set; } = false;
        }
        public class CreateAndUpdatePosition
        {
            public Guid PositionId { get; set; }
            public string Name { get; set; }
        }
        public class DeletePosition
        {
            public Guid Id { get; set; }
            public bool IsDeleted { get; set; }
        }
    }
}
