using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using snapnow.Models;

namespace snapnow.Data;

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    public DbSet<FollowingFollowed> FollowingFollowed { get; set; }
    public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FollowingFollowed>()
            .HasKey(e => new { e.FollowingId, e.FollowedId });

        modelBuilder.Entity<FollowingFollowed>()
            .HasOne(e => e.Following)
            .WithMany(e => e.Followeds)
            .HasForeignKey(e => e.FollowingId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<FollowingFollowed>()
            .HasOne(e => e.Followed)
            .WithMany(e => e.Followings)
            .HasForeignKey(e => e.FollowedId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}