using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace snapnow.Models;

public class ApplicationUser : IdentityUser
{
    public virtual ICollection<FollowingFollowed> Followings { get; set; }
    public virtual ICollection<FollowingFollowed> Followeds { get; set; }
}