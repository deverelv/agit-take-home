namespace AccessRequestHub.Common.DTOs
{
    public class ServiceResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }

        public static ServiceResult Ok() => new()
        {
            Success = true
        };

        public static ServiceResult Fail(string message) => new()
        {
            Success = false,
            ErrorMessage = message
        };
    }

    public class ServiceResult<T>
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public T? Data { get; init; }

        public static ServiceResult<T> Ok(T data) => new()
        {
            Success = true,
            Data = data
        };

        public static ServiceResult<T> Fail(string message) => new()
        {
            Success = false,
            ErrorMessage = message
        };
    }
}
