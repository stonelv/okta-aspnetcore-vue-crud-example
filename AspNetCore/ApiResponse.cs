using System.Collections.Generic;

namespace AspNetCore
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        
        public T Data { get; set; }
        
        public List<string> Errors { get; set; }
        
        public string Message { get; set; }
        
        public ApiResponse()
        {
            Errors = new List<string>();
        }
        
        public static ApiResponse<T> Ok(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }
        
        public static ApiResponse<T> Fail(List<string> errors, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = errors,
                Message = message
            };
        }
        
        public static ApiResponse<T> Fail(string error, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = new List<string> { error },
                Message = message
            };
        }
    }
    
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Ok(string message = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }
        
        public new static ApiResponse Fail(List<string> errors, string message = null)
        {
            return new ApiResponse
            {
                Success = false,
                Errors = errors,
                Message = message
            };
        }
        
        public new static ApiResponse Fail(string error, string message = null)
        {
            return new ApiResponse
            {
                Success = false,
                Errors = new List<string> { error },
                Message = message
            };
        }
    }
}
