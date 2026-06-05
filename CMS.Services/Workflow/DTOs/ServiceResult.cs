namespace CMS.Services.Workflow.DTOs
{
    public class ServiceResult<T>
    {
        public bool   Success      { get; set; }
        public T      Data         { get; set; }
        public string ErrorCode    { get; set; }
        public string ErrorMessage { get; set; }

        public static ServiceResult<T> Ok(T data)
        {
            return new ServiceResult<T> { Success = true, Data = data };
        }

        public static ServiceResult<T> Fail(string errorCode, string message)
        {
            return new ServiceResult<T> { Success = false, ErrorCode = errorCode, ErrorMessage = message };
        }
    }

    public class ServiceResult
    {
        public bool   Success      { get; set; }
        public string ErrorCode    { get; set; }
        public string ErrorMessage { get; set; }

        public static ServiceResult Ok()
        {
            return new ServiceResult { Success = true };
        }

        public static ServiceResult Fail(string errorCode, string message)
        {
            return new ServiceResult { Success = false, ErrorCode = errorCode, ErrorMessage = message };
        }
    }
}
