using System.Text.RegularExpressions;

namespace EasyTestServer.EntityFramework.Local;

public class LocalServerDatabase<TEntryPoint> : ServerDatabaseBase<TEntryPoint, LocalOptions>
    where TEntryPoint : class
{
    private static string SqlServerDatabaseNameRegex => @"Database=(?<dbName>[\w.]+);";

    private static string SqliteDatabaseNameRegex =>
        @"Data Source=(?<path>.+\\|.+/|)(?<dbName>[\w]+)(?<extension>.db);";

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

    private void AddDbContextForLocalSqlServer<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        if (DbOptions.UseTemporaryDatabase)
            DbOptions.ConnectionString = Regex.Replace(
                DbOptions.ConnectionString,
                SqlServerDatabaseNameRegex,
                "Database=${dbName}_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ";",
                RegexOptions.Compiled | RegexOptions.CultureInvariant);
        
        serviceCollection.AddDbContext<TContext>(o =>
            o.UseSqlServer(DbOptions.ConnectionString, builder => builder.UseHierarchyId()));
    }

    private void AddDbContextForLocalSqlite<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        if (DbOptions.UseTemporaryDatabase)
            DbOptions.ConnectionString = Regex.Replace(
                DbOptions.ConnectionString,
                SqliteDatabaseNameRegex,
                "Data Source=${path}${dbName}_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "${extension};",
                RegexOptions.Compiled | RegexOptions.CultureInvariant);
        
        serviceCollection.AddDbContext<TContext>(o => o.UseSqlite(DbOptions.ConnectionString));
    }
}