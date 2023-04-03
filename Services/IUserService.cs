using snapnow.DTOS;
using snapnow.ErrorHandling;

namespace snapnow.Services;

public interface IUserService
{
    public Task<IBaseResponse> RegisterUser(RegisterUserModel userModel);
    public Task<IBaseResponse> LoginUser(LoginUserModel userModel, HttpContext context);
    public Task<IBaseResponse> Logout(HttpContext context);
}