namespace EmployeeAPI.Base
{
    public class ApiResponse <T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
        public int StatusCode { get; set; }
    }
}
