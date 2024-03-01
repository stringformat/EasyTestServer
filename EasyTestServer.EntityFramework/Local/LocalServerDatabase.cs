using System.Text.RegularExpressions;

namespace EasyTestServer.EntityFramework.Local;

public class LocalServerDatabase<TEntryPoint> : ServerDatabaseBase<TEntryPoint, LocalOptions>
    where TEntryPoint : class
{
    public LocalServerDatabase(Server<TEntryPoint> builder) : base(builder, new LocalOptions())
    {
    }

    protected override void AddDbContext<TContext>(IServiceCollection serviceCollection)
    {
        switch (DbOptions.DbType)
        {
            case LocalDbType.SqlServer:
                AddDbContextForLocalSqlServer<TContext>(serviceCollection);
                break;
            case LocalDbType.Sqlite:
                AddDbContextForLocalSqlite<TContext>(serviceCollection);
                break;
        }
    }

    protected override void AddDbContext<TContextService, TContextImplementation>(IServiceCollection serviceCollection)
    {
        switch (DbOptions.DbType)
        {
            case LocalDbType.SqlServer:
                AddDbContextForLocalSqlServer<TContextService, TContextImplementation>(serviceCollection);
                break;
            case LocalDbType.Sqlite:
                AddDbContextForLocalSqlite<TContextService, TContextImplementation>(serviceCollection);
                break;
        }
    }

    private void AddDbContextForLocalSqlServer<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        if (DbOptions.UseTemporaryDatabase) ReplaceSqlServerConnectionString();

        serviceCollection.AddDbContext<TContext>(o =>
            o.UseSqlServer(DbOptions.ConnectionString, builder => builder.UseHierarchyId()));
    }
    
    private void AddDbContextForLocalSqlServer<TContextService, TContextImplementation>(IServiceCollection serviceCollection)
        where TContextService: DbContext
        where TContextImplementation : DbContext, TContextService
    {
        if (DbOptions.UseTemporaryDatabase) ReplaceSqlServerConnectionString();
        
        serviceCollection.AddDbContext<TContextService, TContextImplementation>(o =>
            o.UseSqlServer(DbOptions.ConnectionString, builder => builder.UseHierarchyId()));
    }

    private void AddDbContextForLocalSqlite<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        if (DbOptions.UseTemporaryDatabase) ReplaceSqliteConnectionString();

        serviceCollection.AddDbContext<TContext>(o => o.UseSqlite(DbOptions.ConnectionString));
    }

    private void AddDbContextForLocalSqlite<TContextService, TContextImplementation>(IServiceCollection serviceCollection)
        where TContextService: DbContext
        where TContextImplementation : DbContext, TContextService
    {
        if (DbOptions.UseTemporaryDatabase) ReplaceSqliteConnectionString();
        
        serviceCollection.AddDbContext<TContextService, TContextImplementation>(o => o.UseSqlite(DbOptions.ConnectionString));
    }
    
    private void ReplaceSqlServerConnectionString()
    {
        DbOptions.ConnectionString = Regex.Replace(
            DbOptions.ConnectionString,
            @"Database=(?<dbName>[\w.]+);",
            "Database=${dbName}_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ";",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }
    
    private void ReplaceSqliteConnectionString()
    {
        DbOptions.ConnectionString = Regex.Replace(
            DbOptions.ConnectionString,
            @"Data Source=(?<path>.+\\|.+/|)(?<dbName>[\w]+)(?<extension>.db);",
            "Data Source=${path}${dbName}_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "${extension};",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }
}