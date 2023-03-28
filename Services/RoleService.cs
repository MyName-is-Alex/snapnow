using System.Data;
using Microsoft.AspNetCore.Identity;
using snapnow.DAOS;
using snapnow.DTOS;
using snapnow.ErrorHandling;

namespace snapnow.Services;

public class RoleService : IBaseService<IdentityRole, RoleModel>
{
    private readonly IRoleDao _roleDao;
    public RoleService(IRoleDao roleDao)
    {
        _roleDao = roleDao;
    }
    
    public async Task<DatabaseResponseModel<IdentityRole>> Add(RoleModel role)
    {
        var temp = new IdentityRole
        {
            Name = role.RoleName
        };
        return await _roleDao.Add(temp);
    }

    public DatabaseResponseModel<IdentityRole> GetBy(int id)
    {
        throw new NotImplementedException();
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