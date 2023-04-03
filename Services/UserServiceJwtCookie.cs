using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using snapnow.DAOS;
using snapnow.DTOS;
using snapnow.ErrorHandling;
using snapnow.Models;

namespace snapnow.Services;

public class UserServiceJwtCookie : IUserService
{
    private readonly IUserDao _userDao;
    private readonly RoleService _roleDao;
    private readonly IConfiguration _configuration;

    public UserServiceJwtCookie(IUserDao userDao, RoleService roleDao, IConfiguration configuration)
    {
        _userDao = userDao;
        _roleDao = roleDao;
        _configuration = configuration;
    }
    
    public async Task<IBaseResponse> RegisterUser(RegisterUserModel userModel)
    {
        var userResponseModel = new UserResponseModel();

        if (userModel == null)
        {
            throw new NullReferenceException();
        }

        if (passwordDoesNotMatch(ref userResponseModel, userModel.Password, userModel.ConfirmPassword))
            return userResponseModel;
        
        var databaseResponse = _userDao.CheckPhoneNumber(userModel.PhoneNumber);
        var isUniquePhoneNumber = databaseResponse.Result;
        if (!databaseResponse.IsSuccess)
            return databaseResponse;

        if (!isUniquePhoneNumber)
        {
            databaseResponse.Message = "The phone number is already in use.";
            databaseResponse.IsSuccess = false;
            return databaseResponse;
        }

        var identityUser = new ApplicationUser
        {   
            Email = userModel.Email,
            UserName = userModel.Email,
            PhoneNumber = userModel.PhoneNumber
        };
        
        var addUserResponse = await _userDao.Add(identityUser, userModel.Password);
        if (!addUserResponse.IsSuccess)
            return addUserResponse;

        if (!addUserResponse.Result.Succeeded)
        {
            addUserResponse.Message = "User could not be added to database.";
            addUserResponse.IsSuccess = false;
            return addUserResponse;
        }
        
        var roleResponse = await _roleDao.GetBy(userModel.Role);
        if (!roleResponse.IsSuccess)
            return roleResponse;
        
        var defaultUser = roleResponse.Result;
        if (defaultUser == null)
        {
            roleResponse.Message = "User was added to database. // User was not assigned to a role";
            return roleResponse;
        }
        
        var addToRoleResponse = await _userDao.AddUserToRole(identityUser, defaultUser.Name);
        if (!addToRoleResponse.IsSuccess)
        {
            addToRoleResponse.Message =
                "User was added to database. // User was not assigned to a role. // Could not connect to database.";
            return addToRoleResponse;
        }

        if (!addToRoleResponse.Result.Succeeded)
        {
            addToRoleResponse.Message =
                "User was added to database. // User was not assigned to a role. // Could not connect to database.";
            addToRoleResponse.IsSuccess = false;
            return addToRoleResponse;
        }
        
        // if everything ok, user is registered
        userResponseModel.Message = "User was registered successfully";
        userResponseModel.StatusCode = 200;
        userResponseModel.IsSuccess = true;
        return userResponseModel;
    }

    public async Task<IBaseResponse> LoginUser(LoginUserModel userModel, HttpContext context)
    {
        var databaseResponse = await _userDao.GetBy(userModel.Email);
        if (!databaseResponse.IsSuccess) 
            return databaseResponse;
        
        var user = databaseResponse.Result;

        if (user == null)
        {
            databaseResponse.Message = "Email or password is invalid.";
            databaseResponse.IsSuccess = false;
            return databaseResponse;
        }
        
        var checkPasswordResponse = await _userDao.CheckPassword(user, userModel.Password);
        if (!checkPasswordResponse.IsSuccess)
            return checkPasswordResponse;
        
        var correctPassword = checkPasswordResponse.Result;

        if (!correctPassword)
        {
            checkPasswordResponse.Message = "Email or password is invalid.";
            checkPasswordResponse.IsSuccess = false;
            return checkPasswordResponse;
        }

        var userRolesResponse = await _userDao.GetRolesForUser(user);
        if (!userRolesResponse.IsSuccess)
            return userRolesResponse;
        
        var userRoles = String.Join(",", userRolesResponse.Result);

        ClaimsIdentity identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, userModel.Email),
            new Claim(ClaimTypes.Role, userRoles)
        });
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);

        var userCanSignInResponse = await _userDao.UserConfirmedEmail(user);
        if (!userCanSignInResponse.IsSuccess)
            return userCanSignInResponse;

        if (!userCanSignInResponse.Result)
        {
            userCanSignInResponse.Message = "You must confirm your email.";
            userCanSignInResponse.IsSuccess = false;
            return userCanSignInResponse;
        }
        
        // if everything is ok the user is signed in

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, userModel.Email),
            new Claim(ClaimTypes.Role, userRoles)
        };

        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["AuthSettings:Issuer"],
            audience: _configuration["AuthSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(10),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(10),
            IsEssential = true,
            Secure = true
        };

        context.Response.Cookies.Append("Token", tokenAsString, cookieOptions);
        
        userCanSignInResponse.Message = "Cookie was set.";
        userCanSignInResponse.IsSuccess = true;
        return new UserResponseModel
        {
            Message = "Cookie was set.",
            IsSuccess = true,
            StatusCode = userCanSignInResponse.StatusCode,
            Errors = userCanSignInResponse.Errors,
            ExpireDate = token.ValidTo,
            Token = tokenAsString
        };
    }

    public async Task<IBaseResponse> Logout(HttpContext context)
    {
        context.Response.Cookies.Delete("Token");

        return new UserResponseModel
        {
            Message = "User logged out successfully.",
            IsSuccess = true,
            StatusCode = 200
        };
    }

    private bool passwordDoesNotMatch(ref UserResponseModel userResponseModel, string password1, string password2)
    {
        if (password1 != password2)
        {
            userResponseModel.Message = "Passwords does not match.";
            userResponseModel.IsSuccess = false;
            userResponseModel.StatusCode = 422;
            userResponseModel.Errors = new List<string> { "Password doesn't match." };
            return true;
        }

        return false;
    }
}