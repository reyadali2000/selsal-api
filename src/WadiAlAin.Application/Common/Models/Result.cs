namespace WadiAlAin.Application.Common.Models
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static Result<T> Ok(T data, string message = "تمت العملية بنجاح") =>
            new() { Success = true, Message = message, Data = data };

        public static Result<T> Fail(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}