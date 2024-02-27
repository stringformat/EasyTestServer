using System.Text.RegularExpressions;

namespace EasyTestServer.EntityFramework.Local;

public class LocalServerDatabase<TEntryPoint> : ServerDatabaseBase<TEntryPoint, LocalOptions>
    where TEntryPoint : class
{
    private static string SqlServerDatabaseNameRegex => @"Database=(?<dbName>[\w.]+);";

    private static string SqliteDatabaseNameRegex => @"Data Source=(?<path>.+\\|.+/|)(?<dbName>[\w]+)(?<extension>.db);";
    
    public LocalServerDatabase(Server<TEntryPoint> builder) : base(builder, new LocalOptions())
    {
    }

    public override Server<TEntryPoint> Build<TContext>()
    {
        switch (DbOptions.DbType)
        {
            case LocalDbType.SqlServer:
                Builder.ActionsOnServiceCollection.Add(ReplaceDbByLocalSqlServer<TContext>);
                break;
            case LocalDbType.Sqlite:
                Builder.ActionsOnServiceCollection.Add(ReplaceDbByLocalSqlite<TContext>);
                break;
        }
        
        return base.Build<TContext>();
    }
    
    private void ReplaceDbByLocalSqlServer<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        if(DbOptions.UseTemporaryDatabase) 
            DbOptions.ConnectionString = Regex.Replace(
                DbOptions.ConnectionString, 
                SqlServerDatabaseNameRegex, 
                "Database=${dbName}_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ";", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        RemoveDbContext<TContext>(serviceCollection);
        serviceCollection.AddDbContext<TContext>(o => o.UseSqlServer(DbOptions.ConnectionString));
    }

    private void ReplaceDbByLocalSqlite<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        if(DbOptions.UseTemporaryDatabase) 
            DbOptions.ConnectionString = Regex.Replace(
                DbOptions.ConnectionString, 
                SqliteDatabaseNameRegex, 
                "Data Source=${path}${dbName}_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "${extension};", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        RemoveDbContext<TContext>(serviceCollection);
        serviceCollection.AddDbContext<TContext>(o => o.UseSqlite(DbOptions.ConnectionString));
    }
}