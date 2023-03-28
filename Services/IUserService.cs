using snapnow.DTOS;
using snapnow.ErrorHandling;

namespace snapnow.Services;

public interface IUserService
{
    public Task<UserResponseModel> RegisterUser(RegisterUserModel userModel);
    public UserResponseModel LoginUser(LoginUserModel userModel);
    public UserResponseModel Logout();
}