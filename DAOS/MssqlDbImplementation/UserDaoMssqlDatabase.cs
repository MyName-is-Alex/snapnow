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

    public async Task<DatabaseResponseModel<IdentityResult>> Add(ApplicationUser applicationUser, string password)
    {
        IdentityResult result;
        try
        {
            result = await _userManager.CreateAsync(applicationUser, password);
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
            Result = new List<IdentityResult>{result}
        };
    }


    public async Task<DatabaseResponseModel<ApplicationUser>> GetBy(string email)
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
            IsSuccess = identityUser != null,
            Result = identityUser != null ? new List<ApplicationUser>{identityUser} : new List<ApplicationUser>()
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

    public DatabaseResponseModel<ApplicationUser> CheckPhoneNumber(string phoneNumber)
    {
        ApplicationUser? userWithSamePhoneNumber; 
        try
        {
            userWithSamePhoneNumber = _userManager.Users.SingleOrDefault(x => x.PhoneNumber == phoneNumber);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<ApplicationUser>
            {
                Message = "Database exception",
                IsSuccess = false,
                Errors = new List<string>{exception.Message},
                StatusCode = 500
            };
        }
        
        return new DatabaseResponseModel<ApplicationUser>
        {
            Message = "Connection to database was successful.",
            IsSuccess = true,
            StatusCode = userWithSamePhoneNumber == null ? 200 : 422,
            Errors = new List<string>{userWithSamePhoneNumber == null ? "" : "This phone number is already in use."},
            Result = new List<ApplicationUser>{userWithSamePhoneNumber}
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
            Result = new List<IdentityResult> { result },
            Errors = result.Errors.Select(x => x.Description),
            StatusCode = result.Succeeded ? 200 : 422
        };
    }

    public async Task<DatabaseResponseModel<IdentityResult>> CheckPassword(ApplicationUser identityUser, string password)
    {
        bool result;
        try
        {
            result = await _userManager.CheckPasswordAsync(identityUser, password);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<IdentityResult>
            {
                Message = "Database exception",
                IsSuccess = false,
                Errors = new List<string>{exception.Message},
                StatusCode = 500
            };
        }

        return new DatabaseResponseModel<IdentityResult>
        {   
            Message = "Connection to database was successful.",
            IsSuccess = result,
            StatusCode = result ? 200 : 422,
            Errors = result ? new List<string>() : new List<string>{"Wrong input"}
        };
    }
}