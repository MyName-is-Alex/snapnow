using snapnow.DTOS;
using snapnow.ErrorHandling;

namespace snapnow.Services;

public interface IUserService
{
    public Task<UserResponseModel> RegisterUser(RegisterUserModel userModel);
    public Task<UserResponseModel> LoginUser(LoginUserModel userModel, HttpContext context);
    public UserResponseModel Logout();
}