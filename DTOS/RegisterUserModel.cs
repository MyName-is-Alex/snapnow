namespace snapnow.DTOS;

public class RegisterUserModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string PhoneNumber { get; set; }
    public string Role { get; set; }
}