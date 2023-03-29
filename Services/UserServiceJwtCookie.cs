using Microsoft.AspNetCore.Identity;
using snapnow.DAOS;
using snapnow.DTOS;
using snapnow.ErrorHandling;
using snapnow.Models;

namespace snapnow.Services;

public class UserServiceJwtCookie : IUserService
{
    private readonly IUserDao _userDao;
    private readonly RoleService _roleDao;

    public UserServiceJwtCookie(IUserDao userDao, RoleService roleDao)
    {
        _userDao = userDao;
        _roleDao = roleDao;
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
        
        
        if (phoneNumberAllreadyExists(ref userResponseModel, userModel.PhoneNumber))
            return userResponseModel;
        

        var identityUser = new ApplicationUser
        {   
            Email = userModel.Email,
            UserName = userModel.Email,
            PhoneNumber = userModel.PhoneNumber
        };
        
        var result = await _userDao.Add(identityUser, userModel.Password);
        if (userAddedToDb(ref userResponseModel, result))
        {
            var roleResult = await _roleDao.GetBy(userModel.Role);
            var defaultRole = roleResult.Result?.FirstOrDefault();
            if (defaultRole != null)
            {
                var addToRoleResult = await _userDao.AddUserToRole(identityUser, defaultRole.Name);
                if (!addToRoleResult.IsSuccess)
                    userAssignedToRole(ref userResponseModel, addToRoleResult);

                return userResponseModel;
            }
            
        }

        somethingWentWrong(ref userResponseModel, result);
        return userResponseModel;
    }

    public UserResponseModel LoginUser(LoginUserModel userModel)
    {
        throw new NotImplementedException();
    }

    public UserResponseModel Logout()
    {
        throw new NotImplementedException();
    }

    private bool phoneNumberAllreadyExists(ref UserResponseModel userResponseModel, string phoneNumber)
    {
        var isUniquePhoneNumberResponse = _userDao.CheckPhoneNumber(phoneNumber);
        if (!isUniquePhoneNumberResponse.IsSuccess)
        {
            userResponseModel.Message = "Wrong input";
            userResponseModel.IsSuccess = false;
            userResponseModel.StatusCode = isUniquePhoneNumberResponse.StatusCode;
            userResponseModel.Errors = isUniquePhoneNumberResponse.Errors;
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

    private bool userAddedToDb(ref UserResponseModel userResponseModel, DatabaseResponseModel<ApplicationUser> result)
    {
        if (result.IsSuccess)
        {
            userResponseModel.Message = "User created successfully";
            userResponseModel.IsSuccess = true;
            userResponseModel.StatusCode = result.StatusCode;
            return true;
        }

        return false;
    }

    private void somethingWentWrong(ref UserResponseModel userResponseModel, DatabaseResponseModel<ApplicationUser> result)
    {
        userResponseModel.Message = "Something went wrong.";
        userResponseModel.IsSuccess = false;
        userResponseModel.Errors = result.Errors;
        userResponseModel.StatusCode = result.StatusCode;
    }

    private void userAssignedToRole(ref UserResponseModel userResponseModel, DatabaseResponseModel<IdentityResult> addToRoleResult)
    {
        userResponseModel.Message = "User could not be assigned to a role.";
        userResponseModel.Errors = addToRoleResult.Errors;
        userResponseModel.StatusCode = 500;
    }
}