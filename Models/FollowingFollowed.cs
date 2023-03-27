using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace snapnow.Models;

[Keyless]
public class FollowingFollowed
{
    public string FollowingId { get; set; }
    public string FollowedId { get; set; }

    public virtual ApplicationUser Following { get; set; }
    public virtual ApplicationUser Followed { get; set; }
}