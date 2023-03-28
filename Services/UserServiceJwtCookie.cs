using Microsoft.AspNetCore.Identity;
using snapnow.DAOS;
using snapnow.DTOS;
using snapnow.ErrorHandling;
using snapnow.Models;

namespace snapnow.Services;

public class UserServiceJwtCookie : IUserService
{
    private readonly IUserDao _userDao;

    public UserServiceJwtCookie(IUserDao userDao)
    {
        _userDao = userDao;
    }
    
    public async Task<UserResponseModel> RegisterUser(RegisterUserModel userModel)
    {
        if (userModel == null)
        {
            throw new NullReferenceException();
        }

        if (userModel.Password != userModel.ConfirmPassword)
        {
            return new UserResponseModel
            {
                Message = "Password doesn't match.",
                IsSuccess = false
            };
        }

        var isUniquePhoneNumberResponse = _userDao.CheckPhoneNumber(userModel.PhoneNumber);
        if (!isUniquePhoneNumberResponse.IsSuccess)
        {
            return new UserResponseModel
            {
                Message = "Phone number already exists.",
                IsSuccess = false,
                Errors = new List<string>{isUniquePhoneNumberResponse.Message}
            };
        }

        var identityUser = new ApplicationUser
        {   
            Email = userModel.Email,
            PhoneNumber = userModel.PhoneNumber
        };

        var result = await _userDao.Add(identityUser, userModel.Password);
        
        if (result.IsSuccess)
        {
            return new UserResponseModel
            {
                Message = "User created successfully",
                IsSuccess = true,
                StatusCode = result.StatusCode
            };
        }

        return new UserResponseModel
        {
            Message = "Something went wrong.",
            IsSuccess = false,
            Errors = result.Errors,
            StatusCode = result.StatusCode
        };
    }

    public UserResponseModel LoginUser(LoginUserModel userModel)
    {
        throw new NotImplementedException();
    }

    public UserResponseModel Logout()
    {
        throw new NotImplementedException();
    }
}