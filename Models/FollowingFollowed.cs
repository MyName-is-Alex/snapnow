using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace snapnow.Models;

[Keyless]
public class FollowingFollowed
{
    public string FollowingId { get; set; }
    public string FollowedId { get; set; }

    [JsonIgnore]
    public virtual ApplicationUser Following { get; set; }
    [JsonIgnore]
    public virtual ApplicationUser Followed { get; set; }
}