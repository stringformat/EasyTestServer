namespace EasyTestServer.EntityFramework.InMemory;

public class InMemoryServerDatabase<TEntryPoint> : ServerDatabaseBase<TEntryPoint, InMemoryOptions> where TEntryPoint : class
{
    public InMemoryServerDatabase(Server<TEntryPoint> builder) : base(builder, new InMemoryOptions())
    {
    }

    protected override void AddDbContext<TContext>(IServiceCollection serviceCollection)
    {
        switch (DbOptions.DbType)
        {
            case InMemoryDbType.EF:
                ReplaceDbByInMemoryEF<TContext>(serviceCollection);
                break;
            case InMemoryDbType.Sqlite:
                ReplaceDbByInMemorySqlite<TContext>(serviceCollection);
                break;
        }
    }

    protected override void AddDbContext<TContextService, TContextImplementation>(IServiceCollection serviceCollection)
    {
        switch (DbOptions.DbType)
        {
            case InMemoryDbType.EF:
                ReplaceDbByInMemoryEF<TContextService, TContextImplementation>(serviceCollection);
                break;
            case InMemoryDbType.Sqlite:
                ReplaceDbByInMemorySqlite<TContextService, TContextImplementation>(serviceCollection);
                break;
        }
    }

    private static void ReplaceDbByInMemoryEF<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        serviceCollection.AddDbContext<TContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString(), optionsBuilder => optionsBuilder.UseHierarchyId()));
    }
    
    private static void ReplaceDbByInMemoryEF<TContextService, TContextImplementation>(IServiceCollection serviceCollection)
        where TContextService: DbContext
        where TContextImplementation : DbContext, TContextService
    {
        serviceCollection.AddDbContext<TContextService, TContextImplementation>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString(), optionsBuilder => optionsBuilder.UseHierarchyId()));
    }
    
    private static void ReplaceDbByInMemorySqlite<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        // serviceCollection.AddSingleton<DbConnection, SqliteConnection>(_ =>
        // {
        //     var connection = new SqliteConnection("Data Source=:memory:");
        //     connection.Open();
        //     return connection;
        // });
        //
        // serviceCollection.AddDbContext<TContext>((serviceProvider, options) =>
        // {
        //     var connection = serviceProvider.GetRequiredService<DbConnection>();
        //     options.UseSqlite(connection);
        // });
        
        serviceCollection.AddDbContext<TContext>(o => o.UseSqlite("DataSource=:memory:"));
    }
    
    private static void ReplaceDbByInMemorySqlite<TContextService, TContextImplementation>(IServiceCollection serviceCollection)
        where TContextService: DbContext
        where TContextImplementation : DbContext, TContextService
    {
        // serviceCollection.AddSingleton<DbConnection, SqliteConnection>(_ =>
        // {
        //     var connection = new SqliteConnection("Data Source=:memory:");
        //     connection.Open();
        //     return connection;
        // });
        //
        // serviceCollection.AddDbContext<TContext>((serviceProvider, options) =>
        // {
        //     var connection = serviceProvider.GetRequiredService<DbConnection>();
        //     options.UseSqlite(connection);
        // });
        
        serviceCollection.AddDbContext<TContextService, TContextImplementation>(o => o.UseSqlite("DataSource=:memory:"));
    }
}