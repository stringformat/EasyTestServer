using EasyTestServer.Tests.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EasyTestServer.Tests.Api.Infrastructure;

public class UserContext : DbContext
{
    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasKey(x => x.Id);
        
        modelBuilder.Entity<User>()
            .Property(x => x.Name);

        modelBuilder.Entity<User>()
            .OwnsMany(x => x.Friends, builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Name);
            });
    }
}