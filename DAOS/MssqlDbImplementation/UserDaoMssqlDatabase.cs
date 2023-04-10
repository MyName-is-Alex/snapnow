using Microsoft.AspNetCore.Identity;
using snapnow.ErrorHandling;
using snapnow.Models;

namespace snapnow.DAOS.MssqlDbImplementation;

public class UserDaoMssqlDatabase : IUserDao
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserDaoMssqlDatabase(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<DatabaseResponseModel<IdentityResult>> Add(ApplicationUser applicationUser, string password = "")
    {
        IdentityResult result;
        try
        {
            if (password.Length < 1)
            {
                result = await _userManager.CreateAsync(applicationUser);
            }
            else
            {
                result = await _userManager.CreateAsync(applicationUser, password);
            }
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<IdentityResult>
            {
                Message = "Database exception",
                IsSuccess = false,
                StatusCode = 500,
                Errors = new List<string> { exception.Message }
            };
        }

        return new DatabaseResponseModel<IdentityResult>
        {
            Message = "Database connection succeeded.",
            IsSuccess = true,
            Errors = result.Errors.Select(x => x.Description),
            StatusCode = result.Succeeded ? 200 : 422,
            Result = result
        };
    }


    public async Task<DatabaseResponseModel<ApplicationUser>> GetByNameIdentifier(string email)
    {
        ApplicationUser identityUser;
        try
        {
            identityUser = await _userManager.FindByEmailAsync(email);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<ApplicationUser>
            {
                Message = "Database exception",
                Errors = new List<string>{exception.Message},
                IsSuccess = false,
                StatusCode = 500
            };
        }

        return new DatabaseResponseModel<ApplicationUser>
        {
            Message = "Connection to database was successful.",
            Errors = new List<string>{identityUser == null ? "Could not find user." : ""},
            IsSuccess = true,
            Result = identityUser,
            StatusCode = identityUser != null ? 200 : 422
        };
    }

    public async Task<DatabaseResponseModel<ApplicationUser>> GetById(string userId)
    {
        ApplicationUser user;
        try
        {
            user = await _userManager.FindByIdAsync(userId);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<ApplicationUser>
            {
                Message = "Database exception.",
                IsSuccess = false,
                StatusCode = 500,
                Errors = new List<string>{exception.Message}
            };
        }

        return new DatabaseResponseModel<ApplicationUser>
        {
            Message = "Connection to database was successful.",
            IsSuccess = true,
            Result = user,
            StatusCode = user != null ? 200 : 422,
            Errors = new List<string>{user == null ? "There is no user with this ID." : ""}
        };
    }
    
    public DatabaseResponseModel<ApplicationUser> GetAll()
    {
        throw new NotImplementedException();
    }

    public DatabaseResponseModel<ApplicationUser> Delete(ApplicationUser item)
    {
        throw new NotImplementedException();
    }

    public DatabaseResponseModel<bool> CheckPhoneNumber(string phoneNumber)
    {
        bool isUniquePhoneNumber; 
        try
        {
            isUniquePhoneNumber = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == phoneNumber) == null; 
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<bool>
            {
                Message = "Database exception",
                IsSuccess = false,
                Errors = new List<string>{exception.Message},
                StatusCode = 500
            };
        }
        
        return new DatabaseResponseModel<bool>
        {
            Message = "Connection to database was successful.",
            IsSuccess = true,
            StatusCode = isUniquePhoneNumber ? 200 : 422,
            Errors = new List<string>{isUniquePhoneNumber ? "" : "This phone number is already in use."},
            Result = isUniquePhoneNumber
        };
    }

    public async Task<DatabaseResponseModel<IdentityResult>> AddUserToRole(ApplicationUser user, string roleName)
    {
        IdentityResult result;
        try
        {
            result = await _userManager.AddToRoleAsync(user, roleName);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<IdentityResult>
            {
                Message = "Database exception",
                IsSuccess = false,
                StatusCode = 500,
                Errors = new List<string>{exception.Message}
            };
        }
        
        return new DatabaseResponseModel<IdentityResult>
        {
            Message = "Connection to database succeeded.",
            IsSuccess = true,
            Result = result,
            Errors = result.Errors.Select(x => x.Description),
            StatusCode = result.Succeeded ? 200 : 422
        };
    }

    public async Task<DatabaseResponseModel<bool>> CheckPassword(ApplicationUser identityUser, string password)
    {
        bool result;
        try
        {
            result = await _userManager.CheckPasswordAsync(identityUser, password);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<bool>
            {
                Message = "Database exception",
                IsSuccess = false,
                Errors = new List<string>{exception.Message},
                StatusCode = 500
            };
        }

        return new DatabaseResponseModel<bool>
        {   
            Message = "Connection to database was successful.",
            IsSuccess = true,
            StatusCode = result ? 200 : 422,
            Errors = result ? new List<string>() : new List<string>{"Wrong input"},
            Result = result
        };
    }

    public async Task<DatabaseResponseModel<IList<string>>> GetRolesForUser(ApplicationUser user)
    {
        IList<string> result;
        try
        {
            result = await _userManager.GetRolesAsync(user);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<IList<string>>
            {
                Message = "Database exception",
                IsSuccess = false,
                Errors = new List<string>{exception.Message},
                StatusCode = 500
            };
        }

        return new DatabaseResponseModel<IList<string>>
        {   
            Message = "Connection to database was successful.",
            IsSuccess = true,
            StatusCode = 200,
            Result = result
        };
    }

    public async Task<DatabaseResponseModel<string>> GenerateConfirmationToken(ApplicationUser user)
    {
        string result;
        
        try
        {
            result = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<string>
            {
                Message = "Database exception",
                IsSuccess = false,
                Errors = new List<string>{ exception.Message },
                StatusCode = 500
            };
        }
        
        return new DatabaseResponseModel<string>
        {
            Message = "Connection to database was successful.",
            IsSuccess = true,
            Errors = new List<string>{result ?? "Could not generate email confirmation token."},
            Result = result,
            StatusCode = result != null ? 200 : 422
        };
    }

    public async Task<DatabaseResponseModel<bool>> UserConfirmedEmail(ApplicationUser user)
    {
        bool result;
        try
        {
            result = await _userManager.IsEmailConfirmedAsync(user);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<bool>
            {
                Message = "Database exception.",
                IsSuccess = false,
                Errors = new List<string> { exception.Message },
                StatusCode = 500
            };
        }

        return new DatabaseResponseModel<bool>
        {
            Message = "Connection to database was successful.",
            IsSuccess = true,
            StatusCode = result ? 200 : 403,
            Result = result
        };
    }

    public async Task<DatabaseResponseModel<IdentityResult>> ConfirmUserEmail(ApplicationUser user, string token)
    {
        IdentityResult result;

        try
        {
            result = await _userManager.ConfirmEmailAsync(user, token);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<IdentityResult>
            {
                Message = "Database exception.",
                IsSuccess = false,
                Errors = new List<string> { exception.Message },
                StatusCode = 500
            };
        }

        return new DatabaseResponseModel<IdentityResult>
        {
            Message = "Connection to database was successful.",
            IsSuccess = true,
            StatusCode = result.Succeeded ? 200 : 422,
            Result = result,
            Errors = result.Errors.Select(x => x.Description)
        };
    }

    public async Task<DatabaseResponseModel<IdentityResult>> AddUserLogin(ApplicationUser userId, UserLoginInfo loginInfo)
    {
        IdentityResult result;
        try
        {
            result = await _userManager.AddLoginAsync(userId, loginInfo);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<IdentityResult>
            {
                Message = "Database exception.",
                IsSuccess = false,
                StatusCode = 500,
                Errors = new List<string>{exception.Message}
            };
        }

        return new DatabaseResponseModel<IdentityResult>
        {
            Message = "Connection to database was successful.",
            IsSuccess = true,
            StatusCode = result.Succeeded ? 200 : 422,
            Result = result,
            Errors = result.Errors.Select(x => x.Description)
        };
    }
}