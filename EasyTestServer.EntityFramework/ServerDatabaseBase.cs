using System.Collections.ObjectModel;
using System.Data.Common;

namespace EasyTestServer.EntityFramework;

public abstract class ServerDatabaseBase<TEntryPoint, TOptions> where TEntryPoint : class
{
    private readonly Server<TEntryPoint> _builder;
    private readonly Collection<object> _data = [];
    private readonly Collection<string> _sqls = [];

    protected readonly TOptions DbOptions;

    protected ServerDatabaseBase(Server<TEntryPoint> builder, TOptions dbOptions)
    {
        _builder = builder;
        DbOptions = dbOptions;
    }
    
    protected abstract void AddDbContext<TContext>(IServiceCollection serviceCollection) where TContext : DbContext;
    
    protected abstract void AddDbContext<TContext>(Func<DbContextOptions<TContext>, TContext> func, IServiceCollection serviceCollection) where TContext : DbContext;

    protected abstract void AddDbContext<TContextService, TContextImplementation>(IServiceCollection serviceCollection)
        where TContextService: DbContext
        where TContextImplementation : DbContext, TContextService;

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

    public Server<TEntryPoint> Build<TContext>() where TContext : DbContext
    {
        _builder.ActionsOnServiceCollection.Add(RemoveDbContext<TContext>);
        _builder.ActionsOnServiceCollection.Add(AddDbContext<TContext>);
        _builder.ActionsOnServiceCollection.Add(CreateAndFillDb<TContext>);
        
        return _builder;
    }
    
    public Server<TEntryPoint> Build<TContext>(Func<DbContextOptions<TContext>, TContext> func) where TContext : DbContext
    {
        _builder.ActionsOnServiceCollection.Add(RemoveDbContext<TContext>);
        _builder.ActionsOnServiceCollection.Add(x => AddDbContext(func, x));
        _builder.ActionsOnServiceCollection.Add(CreateAndFillDb<TContext>);
        
        return _builder;
    }

    public Server<TEntryPoint> Build<TContextService, TContextImplementation>()
        where TContextService: DbContext
        where TContextImplementation : DbContext, TContextService
    {
        _builder.ActionsOnServiceCollection.Add(RemoveDbContext<TContextService>);
        _builder.ActionsOnServiceCollection.Add(AddDbContext<TContextService, TContextImplementation>);
        _builder.ActionsOnServiceCollection.Add(CreateAndFillDb<TContextService>);

        return _builder;
    }

    public Server<TEntryPoint> Build<TContext>(Action<TOptions> options)
        where TContext : DbContext
    {
        options(DbOptions);

        return Build<TContext>();
    }
    
    public Server<TEntryPoint> Build<TContext>(Func<DbContextOptions<TContext>, TContext> func, Action<TOptions> options)
        where TContext : DbContext
    {
        options(DbOptions);

        return Build(func);
    }
    
    public Server<TEntryPoint> Build<TOriginalContext, TNewContext>(Action<TOptions> options)
        where TOriginalContext : DbContext
        where TNewContext : TOriginalContext
    {
        options(DbOptions);

        return Build<TOriginalContext, TNewContext>();
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

    private static void RemoveDbContext<TContext>(IServiceCollection serviceCollection) where TContext : DbContext
    {
        serviceCollection.RemoveService<DbContextOptions<TContext>>();
        serviceCollection.RemoveService<DbConnection>();
    }
}