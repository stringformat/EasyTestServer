using EasyTestServer.Core;
using EasyTestServer.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasyTestServer.EntityFramework;

public class EasyTestServerDatabaseBuilder(EasyTestServerBuilder builder)
{
    private readonly List<object> _data = new();

    public EasyTestServerDatabaseBuilder WithData(object data)
    {
        ArgumentNullException.ThrowIfNull(data);

        _data.Add(data);
        return this;
    }

    public EasyTestServerDatabaseBuilder WithData(IEnumerable<object> data)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        foreach (var value in data)
        {
            WithData(value);
        }

        return this;
    }

    public EasyTestServerBuilder Build<TContext>(string? dbName = null) where TContext : DbContext
    {
        builder.ActionsOnServiceCollection.Add(CreateTestingDbAndAddData);

        return builder;

        void CreateTestingDbAndAddData(IServiceCollection services)
        {
            dbName ??= Guid.NewGuid().ToString();

            services.RemoveService<DbContextOptions<TContext>>();
            services.AddDbContext<TContext>(options => { options.UseInMemoryDatabase(dbName); });

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            context.Database.EnsureCreated();
            context.AddRange(_data);
            context.SaveChanges();
        }
    }
}