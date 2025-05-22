namespace EmployeeAPI.Base
{
    public class ApiResponse <T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
        public int StatusCode { get; set; }


        public static ApiResponse<T> ReturnResult(string message, T data, int statusCode)
        {
            return new ApiResponse<T>
            {
                Message = message,
                Data = data,
                StatusCode = statusCode
            };
        }
    }
}
