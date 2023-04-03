using System.Data;
using Microsoft.AspNetCore.Identity;
using snapnow.DAOS;
using snapnow.DTOS;
using snapnow.ErrorHandling;

namespace snapnow.Services;

public class RoleService : IBaseService<IdentityRole, RoleModel, IdentityResult>
{
    private readonly IRoleDao _roleDao;
    public RoleService(IRoleDao roleDao)
    {
        _roleDao = roleDao;
    }
    
    public async Task<DatabaseResponseModel<IdentityResult>> Add(RoleModel role)
    {
        var existentRoleResponse = GetBy(role.RoleName).Result.Result;

        if (existentRoleResponse != null)
        {
            return new DatabaseResponseModel<IdentityResult>
            {
                Message = "Role already exists.",
                IsSuccess = false
            };
        }
        
        var temp = new IdentityRole
        {
            Name = role.RoleName
        };
        var addResponse = await _roleDao.Add(temp);

        addResponse.Message = addResponse.IsSuccess ? "New role was added." : "Role was not added.";
        
        return addResponse;
    }

    public async Task<DatabaseResponseModel<IdentityRole>> GetBy(string roleName)
    {
        var result = await _roleDao.GetBy(roleName);
        result.Message = result.IsSuccess ? "Role was find." : "Could not find role.";

        return result;
    }

    public DatabaseResponseModel<IdentityRole> GetAll()
    {
        throw new NotImplementedException();
    }
    public DatabaseResponseModel<IdentityRole> Delete(RoleModel role)
    {
        throw new NotImplementedException();
    }
}