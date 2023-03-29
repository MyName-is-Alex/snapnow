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

    public async Task<DatabaseResponseModel<ApplicationUser>> Add(ApplicationUser applicationUser, string password)
    {
        IdentityResult result;
        try
        {
            result = await _userManager.CreateAsync(applicationUser, password);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<ApplicationUser>
            {
                Message = "Cannot connect to database.",
                IsSuccess = false,
                StatusCode = 500,
                Errors = new List<string> { exception.Message }
            };
        }

        return new DatabaseResponseModel<ApplicationUser>
        {
            Message = "Database connection succeeded.",
            IsSuccess = result.Succeeded,
            Errors = result.Errors.Select(x => x.Description),
            StatusCode = result.Succeeded ? 200 : 422
        };
    }


    public async Task<DatabaseResponseModel<ApplicationUser>> GetBy(string email)
    {
        throw new NotImplementedException();
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
        bool isUniquePhoneNumber;
        try
        {
            isUniquePhoneNumber = !_userManager.Users.Select(x => x.PhoneNumber).Contains(phoneNumber);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<ApplicationUser>
            {
                Message = "Could not connect to database",
                IsSuccess = false,
                Errors = new List<string>{exception.Message},
                StatusCode = 500
            };
        }
        
        return new DatabaseResponseModel<ApplicationUser>
        {
            Message = "Connection to database was successful.",
            IsSuccess = isUniquePhoneNumber,
            StatusCode = isUniquePhoneNumber ? 200 : 422,
            Errors = new List<string>{isUniquePhoneNumber ? "" : "This phone number is already in use."}
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
                Message = "Could not connect to database",
                IsSuccess = false,
                StatusCode = 500,
                Errors = new List<string>{exception.Message}
            };
        }
        
        return new DatabaseResponseModel<IdentityResult>
        {
            Message = "Connection to database succeeded.",
            IsSuccess = result.Succeeded,
            Result = new List<IdentityResult> { result },
            Errors = result.Errors.Select(x => x.Description),
            StatusCode = result.Succeeded ? 200 : 422
        };
    }
}