using carp_tm_delegati.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace carp_tm_delegati.Area.Identity.data;


public class carp_tm_delegati_UsersContext : IdentityDbContext<carp_tm_delegatiUser>
{
    public carp_tm_delegati_UsersContext(DbContextOptions<carp_tm_delegati_UsersContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        builder.ApplyConfiguration(new ApplicationUserEntityConfiguration());
    }

    public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<carp_tm_delegatiUser>
    {
      public void Configure(EntityTypeBuilder<carp_tm_delegatiUser> builder)
      {
            builder.Property(x => x.FirtName).HasMaxLength(100);
            builder.Property(x => x.LastName).HasMaxLength(100);
      }
    
    }
}
