using Microsoft.AspNetCore.Identity;
using snapnow.ErrorHandling;

namespace snapnow.DAOS;

public interface IRoleDao : IDao<IdentityRole, int>
{
}