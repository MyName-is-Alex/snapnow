namespace snapnow.ErrorHandling;

public class DatabaseResponseModel<T> : IBaseResponse
{
    public string Message { get; set; }
    public IEnumerable<T>? Result { get; set; } 
    public bool IsSuccess { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public int StatusCode { get; set; }
}