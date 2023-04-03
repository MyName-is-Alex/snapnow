using Microsoft.AspNetCore.Identity;
using snapnow.DTOS;
using snapnow.ErrorHandling;
using snapnow.Models;

namespace snapnow.DAOS;

public interface IUserDao : IDao<ApplicationUser, string, IdentityResult>
{
    public DatabaseResponseModel<bool> CheckPhoneNumber(string phoneNumber);
    public Task<DatabaseResponseModel<IdentityResult>> AddUserToRole(ApplicationUser identityUser, string roleName);
    public Task<DatabaseResponseModel<bool>> CheckPassword(ApplicationUser identityUser, string password);
    public Task<DatabaseResponseModel<IList<string>>> GetRolesForUser(ApplicationUser user);
    public Task<DatabaseResponseModel<bool>> UserConfirmedEmail(ApplicationUser user);
}