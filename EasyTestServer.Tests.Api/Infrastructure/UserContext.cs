using EasyTestServer.Tests.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EasyTestServer.Tests.Api.Infrastructure;

public class UserContext(DbContextOptions options) : DbContext(options)
{
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

public class TestContext(DbContextOptions options) : UserContext(options);