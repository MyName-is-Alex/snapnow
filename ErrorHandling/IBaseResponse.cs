namespace snapnow.ErrorHandling;

public interface IBaseResponse
{
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public int StatusCode { get; set; }
}