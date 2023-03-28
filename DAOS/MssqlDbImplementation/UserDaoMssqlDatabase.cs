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
        try
        {
            var result = await _userManager.CreateAsync(applicationUser, password);
            return new DatabaseResponseModel<ApplicationUser>
            {
                Message = "Database connection succeeded.",
                IsSuccess = result.Succeeded,
                Errors = result.Errors.Select(x => x.Description),
                StatusCode = Int32.Parse(result.Errors.First().Code)
            };
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<ApplicationUser>
            {
                Message = "Cannot connect to database.",
                IsSuccess = false,
                StatusCode = 500,
                Errors = new List<string>{exception.Message}
            };
        }
    }

    public DatabaseResponseModel<ApplicationUser> GetBy(string email)
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
        try
        {
            bool isUniquePhoneNumber = !_userManager.Users.Select(x => x.PhoneNumber).Contains(phoneNumber);
            return new DatabaseResponseModel<ApplicationUser>
            {
                Message = "IsSuccess property represents the existence of the phone number in the database.",
                IsSuccess = isUniquePhoneNumber
            };
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<ApplicationUser>
            {
                Message = "Could not connect to database",
                IsSuccess = false,
                Errors = new List<string>{exception.Message}
            };
        }
    }
}