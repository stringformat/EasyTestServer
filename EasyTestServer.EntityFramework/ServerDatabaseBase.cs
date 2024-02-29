using System.Collections.ObjectModel;
using System.Data.Common;

namespace EasyTestServer.EntityFramework;

public abstract class ServerDatabaseBase<TEntryPoint, TOptions> where TEntryPoint : class
{
    private readonly Collection<object> _data = [];
    private readonly Collection<string> _sqls = [];
    
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
    
    public ServerDatabaseBase<TEntryPoint, TOptions> WithSql(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        _sqls.Add(sql);
        return this;
    }

    public ServerDatabaseBase<TEntryPoint, TOptions> WithData(IEnumerable<string> sqls)
    {
        ArgumentNullException.ThrowIfNull(sqls);
        
        foreach (var sql in sqls)
        {
            WithSql(sql);
        }

        return this;
    }
    
    public ServerDatabaseBase<TEntryPoint, TOptions> WithData(params string[] sqls)
    {
        ArgumentNullException.ThrowIfNull(sqls);
        
        foreach (var sql in sqls)
        {
            WithSql(sql);
        }

        return this;
    }

    public virtual Server<TEntryPoint> Build<TContext>() where TContext : DbContext
    {
        Builder.ActionsOnServiceCollection.Add(CreateAndFillDb<TContext>);

        return Builder;
    }

    public Server<TEntryPoint> Build<TContext>(Action<TOptions> options)
        where TContext : DbContext
    {
        options(DbOptions);

        return Build<TContext>();
    }
    
    private void CreateAndFillDb<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        using var scope = serviceCollection.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>().CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<TContext>();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        foreach (var sql in _sqls)
        {
            context.Database.ExecuteSqlRaw(sql);
        }
        
        foreach (var data in _data)
        {
            context.Add(data);
            context.SaveChanges();
        }
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
    

    // private void Migrate<TContext>(IServiceCollection serviceCollection)
    //     where TContext : DbContext
    // {
    //     using var scope = serviceCollection.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>().CreateScope();
    //     using var context = scope.ServiceProvider.GetRequiredService<TContext>();
    //     
    //     context.Database.Migrate();
    // }
    
    // private void GetDbContext<TContext>(IServiceCollection serviceCollection)
    //     where TContext : DbContext
    // {
    //     using var scope = serviceCollection.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>().CreateScope();
    //     _dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
    // }
}