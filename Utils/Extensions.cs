using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using snapnow.Models;

namespace snapnow.Utils;

public static class Extensions
{
    public static IEnumerable<ApplicationUser> GetCompleteUsers(this UserManager<ApplicationUser> userManager)
    {
        return userManager.Users
            .Include(x => x.Followeds)
            .Include(x => x.Followings);
    }
}