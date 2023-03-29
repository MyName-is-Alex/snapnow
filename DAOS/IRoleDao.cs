using Microsoft.AspNetCore.Identity;
using snapnow.ErrorHandling;
using snapnow.Models;

namespace snapnow.DAOS;

public interface IRoleDao : IDao<IdentityRole, string>
{
}