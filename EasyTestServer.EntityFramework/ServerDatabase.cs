using System.Collections.ObjectModel;

namespace EasyTestServer.EntityFramework;

public class ServerDatabase(Server builder)
{
    private readonly Collection<object> _data = [];

    public ServerDatabase WithData(object data)
    {
        ArgumentNullException.ThrowIfNull(data);

        _data.Add(data);
        return this;
    }

    public ServerDatabase WithData(IEnumerable<object> data)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        foreach (var value in data)
        {
            WithData(value);
        }

        return this;
    }

    public Server Build<TContext>(bool useInMemoryDb = true, string? dbName = null) where TContext : DbContext
    {
        if(useInMemoryDb)
            builder.ActionsOnServiceCollection.Add(ReplaceDbByTestingDb);
        
        builder.ActionsOnServiceCollection.Add(AddData);

        return builder;

        void ReplaceDbByTestingDb(IServiceCollection serviceCollection)
        {
            dbName ??= Guid.NewGuid().ToString();
            serviceCollection.RemoveService<DbContextOptions<TContext>>();
            serviceCollection.AddDbContext<TContext>(options => options.UseInMemoryDatabase(dbName, optionsBuilder => optionsBuilder.UseHierarchyId()));
        }

        void AddData(IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            context.Database.EnsureCreated();
            context.AddRange(_data);
            context.SaveChanges();
        }
    }
}