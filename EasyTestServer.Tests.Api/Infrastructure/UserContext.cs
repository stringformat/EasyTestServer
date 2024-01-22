using EasyTestServer.Tests.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EasyTestServer.Tests.Api.Infrastructure;

public class UserContext(DbContextOptions<UserContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasKey(x => x.Id);
        
        modelBuilder.Entity<User>()
            .Property(x => x.Name);
    }
}