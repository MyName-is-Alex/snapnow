using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using snapnow.DTOS;
using snapnow.ErrorHandling;
using snapnow.Models;

namespace snapnow.Services;

public interface IUserService
{
    public Task<IBaseResponse> RegisterUser(RegisterUserModel userModel, IUrlHelper urlHelper);
    public Task<IBaseResponse> LoginUser(LoginUserModel userModel);
    public IBaseResponse Logout();
    public Task<DatabaseResponseModel<ApplicationUser>> GetUserById(string userId);
    public Task<IBaseResponse> ConfirmUserEmail(string user, string token);
    public Task<IBaseResponse> GoogleAuthentication(string accessToken);
}