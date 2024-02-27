using System.Collections.ObjectModel;
using System.Data.Common;

namespace EasyTestServer.EntityFramework;

public abstract class ServerDatabaseBase<TEntryPoint, TOptions> where TEntryPoint : class
{
    private readonly Collection<object> _data = [];
    
    protected readonly Server<TEntryPoint> Builder;
    protected readonly TOptions DbOptions;

    protected ServerDatabaseBase(Server<TEntryPoint> builder, TOptions dbOptions)
    {
        Builder = builder;
        DbOptions = dbOptions;
    }

    public ServerDatabaseBase<TEntryPoint, TOptions> WithData(object data)
    {
        ArgumentNullException.ThrowIfNull(data);

        _data.Add(data);
        return this;
    }

    public ServerDatabaseBase<TEntryPoint, TOptions> WithData(IEnumerable<object> data)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        foreach (var value in data)
        {
            WithData(value);
        }

        return this;
    }
    
    public ServerDatabaseBase<TEntryPoint, TOptions> WithData(params object[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        foreach (var value in data)
        {
            WithData(value);
        }

        return this;
    }

    public virtual Server<TEntryPoint> Build<TContext>() where TContext : DbContext
    {
        Builder.ActionsOnServiceCollection.Add(EnsureCreated<TContext>);
        Builder.ActionsOnServiceCollection.Add(AddData<TContext>);
        //Builder.ActionsOnServiceCollection.Add(GetDbContext<TContext>);

        return Builder;
    }

    public Server<TEntryPoint> Build<TContext>(Action<TOptions> options)
        where TContext : DbContext
    {
        options(DbOptions);

        return Build<TContext>();
    }

    // public Server<TEntryPoint> Build<TContext>(out DbContext dbContext)
    //     where TContext : DbContext
    // {
    //     dbContext = _dbContext;
    //     
    //     return Build<TContext>();
    // }
    //
    // public Server<TEntryPoint> Build<TContext>(Action<TOptions> options, out DbContext dbContext)
    //     where TContext : DbContext
    // {
    //     options(DbOptions);
    //     dbContext = _dbContext;
    //     
    //     return Build<TContext>();
    // }
    
    protected static void RemoveDbContext<TContext>(IServiceCollection serviceCollection) where TContext : DbContext
    {
        serviceCollection.RemoveService<DbContextOptions<TContext>>();
        serviceCollection.RemoveService<DbConnection>();
    }

    private void AddData<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        using var scope = serviceCollection.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>().CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<TContext>();

        foreach (var data in _data)
        {
            context.Add(data);
            context.SaveChanges();
        }
    }

    // private void Migrate<TContext>(IServiceCollection serviceCollection)
    //     where TContext : DbContext
    // {
    //     using var scope = serviceCollection.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>().CreateScope();
    //     using var context = scope.ServiceProvider.GetRequiredService<TContext>();
    //     
    //     context.Database.Migrate();
    // }
    
    private void EnsureCreated<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        using var scope = serviceCollection.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>().CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<TContext>();
        
        context.Database.EnsureCreated();
    }
    
    // private void GetDbContext<TContext>(IServiceCollection serviceCollection)
    //     where TContext : DbContext
    // {
    //     using var scope = serviceCollection.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>().CreateScope();
    //     _dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
    // }
}