using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using snapnow.ErrorHandling;
using snapnow.Models;

namespace snapnow.DAOS.MssqlDbImplementation;

public class RoleDaoMssqlDatabase : IRoleDao
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleDaoMssqlDatabase(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager; 
    }
    
    public async Task<DatabaseResponseModel<IdentityResult>> Add(IdentityRole item, string temp)
    {
        IdentityResult result;
        try
        {
            result = await _roleManager.CreateAsync(item);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<IdentityResult>
            {
                Message = "Could not connect to database",
                IsSuccess = false,
                Errors = new List<string> {exception.Message}.AsEnumerable(),
                StatusCode = 500
            };
        }
        
        return new DatabaseResponseModel<IdentityResult>
        {
            Message = "Connection to database was successful.",
            IsSuccess = result.Succeeded,
            Errors = result.Errors.Select(x => x.Description),
            StatusCode = result.Succeeded ? 200 : 422,
            Result = new List<IdentityResult>{result}
        };
    }

    public async Task<DatabaseResponseModel<IdentityRole>> GetBy(string roleName)
    {
        IdentityRole defaultRole;
        try
        {
            defaultRole = await _roleManager.FindByNameAsync(roleName);
        }
        catch (Exception exception)
        {
            return new DatabaseResponseModel<IdentityRole>
            {
                Message = "Could not connect to database.",
                IsSuccess = false,
                StatusCode = 500,
                Errors = new List<string> { exception.Message }
            };
        }
        
        return new DatabaseResponseModel<IdentityRole>
        {
            Message = "Connection to database was successful.",
            IsSuccess = defaultRole != null,
            StatusCode = defaultRole != null ? 200 : 422,
            Result = new List<IdentityRole>{defaultRole}
        };
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