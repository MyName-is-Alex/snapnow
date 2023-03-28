namespace snapnow.ErrorHandling;

public class UserResponseModel : IBaseResponse
{
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public string? Token { get; set; }
    public DateTime ExpireDate { get; set; }
}