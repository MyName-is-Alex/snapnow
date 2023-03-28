using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using snapnow.ErrorHandling;

namespace snapnow.DAOS.MssqlDbImplementation;

public class RoleDaoMssqlDatabase : IRoleDao
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleDaoMssqlDatabase(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager; 
    }
    
    public async Task<DatabaseResponseModel<IdentityRole>> Add(IdentityRole item)
    {
        try
        {
            if (_roleManager.Roles.Select(x => x.Name).Contains(item.Name))
            {
                return new DatabaseResponseModel<IdentityRole>
                {
                    Message = "This role already exists.",
                    IsSuccess = false
                };
            }
            
            var result = await _roleManager.CreateAsync(item);
            if (result.Succeeded)
            {
                return new DatabaseResponseModel<IdentityRole>
                {
                    Message = "Role has been added.",
                    IsSuccess = true
                };
            }

            return new DatabaseResponseModel<IdentityRole>
            {
                Message = "Role was not created.",
                IsSuccess = false,
                Errors = result.Errors.Select(x => x.Description)
            };
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<IdentityRole>
            {
                Message = "Error when trying to connect to database.",
                IsSuccess = false,
                Errors = new List<string> {exception.Message}.AsEnumerable(),
                StatusCode = 500
            };
        }
    }

public DatabaseResponseModel<IdentityRole> GetBy(int id)
    {
        throw new NotImplementedException();
    }

    public DatabaseResponseModel<IdentityRole> GetAll()
    {
        throw new NotImplementedException();
    }

    public DatabaseResponseModel<IdentityRole> Delete(IdentityRole item)
    {
        throw new NotImplementedException();
    }
}