using Microsoft.EntityFrameworkCore;

namespace EasyTestServer.Tests.Api.Infrastructure;

public class TestContext(DbContextOptions<TestContext> options) : DbContext(options)
{
    public DbSet<TestEntity> TestEntities { get; set; }
}