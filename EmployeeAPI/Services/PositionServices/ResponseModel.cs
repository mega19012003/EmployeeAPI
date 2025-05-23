namespace EmployeeAPI.Services.PositionServices
{
    public class ResponseModel
    {
        public class PositionDTO
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
            public bool IsDeleted { get; set; } = false;
        }
        public class CreatePosition
        {
            //public Guid PositionId { get; set; }
            public Guid DepartmentId { get; set; }
            public string Name { get; set; }
        }
        public class UpdatePosition
        {
            public Guid PositionId { get; set; }
            public string Name { get; set; }
        }
        public class DeletePosition
        {
            public Guid Id { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class PositionByDepartment
        {
            public Guid PositionId { get; set; }
            public string Name { get; set; }
        }
    }
}
