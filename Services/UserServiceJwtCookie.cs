using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure;
using Azure.Core;
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
    
    public async Task<UserResponseModel> RegisterUser(RegisterUserModel userModel)
    {
        var userResponseModel = new UserResponseModel();

        if (userModel == null)
        {
            throw new NullReferenceException();
        }

        if (passwordDoesNotMatch(ref userResponseModel, userModel.Password, userModel.ConfirmPassword))
            return userResponseModel;
        
        
        if (phoneNumberAlreadyExists(ref userResponseModel, userModel.PhoneNumber))
            return userResponseModel;
        

        var identityUser = new ApplicationUser
        {   
            Email = userModel.Email,
            UserName = userModel.Email,
            PhoneNumber = userModel.PhoneNumber
        };
        
        var result = await _userDao.Add(identityUser, userModel.Password);
        if (!userAddedToDb(ref userResponseModel, result))
            return userResponseModel;

        var roleResponse = await _roleDao.GetBy(userModel.Role);
        var defaultUser = roleResponse.Result?.FirstOrDefault();
        if (defaultUser == null)
        {
            userResponseModel.Message += " // User was not assigned to a role";
            return userResponseModel;
        }
        
        var addToRoleResponse = await _userDao.AddUserToRole(identityUser, defaultUser.Name);
        userAssignedToRole(ref userResponseModel, addToRoleResponse);
        return userResponseModel;
    }

    public async Task<UserResponseModel> LoginUser(LoginUserModel userModel, HttpContext context)
    {
        var userResponse = await _userDao.GetBy(userModel.Email);
        var user = userResponse.Result?.FirstOrDefault();
        if (!userResponse.IsSuccess)
        {
            return new UserResponseModel
            {
                Message = userResponse.Message,
                IsSuccess = userResponse.IsSuccess,
                Errors = userResponse.Errors
            };
        }
        
        var checkPasswordResponse = await _userDao.CheckPassword(user, userModel.Password);
        var correctPassword = checkPasswordResponse.IsSuccess;
        if (!correctPassword)
        {
            return new UserResponseModel
            {
                Message = checkPasswordResponse.Message,
                IsSuccess = correctPassword,
                Errors = checkPasswordResponse.Errors
            };
        }

        var claims = new[]
        {
            new Claim("Email", userModel.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
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
        
        // set cookie
        context.Response.Cookies.Append("Token", tokenAsString, cookieOptions);

        /*bool isCookieSet = context.Request.Cookies.Keys.Contains("Token");*/
        return new UserResponseModel
        {
            Message = "Cookie was set.",
            IsSuccess = true,
            StatusCode = 200,
            ExpireDate = token.ValidTo,
            Token = tokenAsString
        };
    }

    public async Task<UserResponseModel> Logout(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        context.Response.Cookies.Delete("Token");

        return new UserResponseModel
        {
            Message = "User logged out successfully.",
            IsSuccess = true,
            StatusCode = 200
        };
    }

    private bool phoneNumberAlreadyExists(ref UserResponseModel userResponseModel, string phoneNumber)
    {
        var databaseResponse = _userDao.CheckPhoneNumber(phoneNumber);
        var isUniquePhoneNumber = databaseResponse.Result?.FirstOrDefault() == null;
        if (!databaseResponse.IsSuccess)
        {
            userResponseModel.Message = databaseResponse.Message;
            userResponseModel.IsSuccess = databaseResponse.IsSuccess;
            userResponseModel.StatusCode = databaseResponse.StatusCode;
            userResponseModel.Errors = databaseResponse.Errors;
            return true;
        }

        if (!isUniquePhoneNumber)
        {
            userResponseModel.Message = "Phone number is already in use.";
            userResponseModel.IsSuccess = isUniquePhoneNumber;
            userResponseModel.StatusCode = databaseResponse.StatusCode;
            userResponseModel.Errors = databaseResponse.Errors;
            return true;
        }

        return false;
    }

    private bool passwordDoesNotMatch(ref UserResponseModel userResponseModel, string password1, string password2)
    {
        if (password1 != password2)
        {
            userResponseModel.Message = "Wrong input";
            userResponseModel.IsSuccess = false;
            userResponseModel.StatusCode = 422;
            userResponseModel.Errors = new List<string> { "Password doesn't match." };
            return true;
        }

        return false;
    }

    private bool userAddedToDb(ref UserResponseModel userResponseModel, DatabaseResponseModel<IdentityResult> databaseResponse)
    {
        if (!databaseResponse.IsSuccess)
        {
            userResponseModel.Message = databaseResponse.Message;
            userResponseModel.IsSuccess = databaseResponse.IsSuccess;
            userResponseModel.StatusCode = databaseResponse.StatusCode;
            return false;
        }

        var userAddedToDb = databaseResponse.Result?.FirstOrDefault();
        userResponseModel.Message = userAddedToDb.Succeeded ? "User added to db." : "User was not added to db.";
        userResponseModel.IsSuccess = userAddedToDb.Succeeded;
        userResponseModel.Errors = databaseResponse.Errors;
        userResponseModel.StatusCode = databaseResponse.StatusCode;

        return userAddedToDb.Succeeded;
    }

    private void userAssignedToRole(ref UserResponseModel userResponseModel, DatabaseResponseModel<IdentityResult> addToRoleResponse)
    {
        if (!addToRoleResponse.IsSuccess)
        {
            userResponseModel.Message = addToRoleResponse.Message;
            userResponseModel.IsSuccess = addToRoleResponse.IsSuccess;
            userResponseModel.Errors = addToRoleResponse.Errors;
            userResponseModel.StatusCode = addToRoleResponse.StatusCode;
        }
    
        var addToRoleResult = addToRoleResponse.Result?.FirstOrDefault();
        if (addToRoleResult != null)
        {
            userResponseModel.Message += addToRoleResult.Succeeded
                ? "User was assigned to a role"
                : "User was not assigned to a role";
        }
        userResponseModel.Errors = addToRoleResponse.Errors;
        userResponseModel.StatusCode = addToRoleResponse.StatusCode;
    }
}