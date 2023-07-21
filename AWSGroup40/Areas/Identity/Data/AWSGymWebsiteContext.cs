using AWSGymWebsite.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AWSGymWebsite.Data;

public class AWSGymWebsiteContext : IdentityDbContext<AWSGymWebsiteUser>
{
    public AWSGymWebsiteContext(DbContextOptions<AWSGymWebsiteContext> options)
        : base(options)
    {
    }
    public DbSet<AWSGymWebsite.Models.GymPage> gympage { get; set; }
    public DbSet<AWSGymWebsite.Models.Subscriber> subscriber { get; set; }
    public DbSet<AWSGymWebsite.Models.SNSTopic> snstopic { get; set; }
   

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
