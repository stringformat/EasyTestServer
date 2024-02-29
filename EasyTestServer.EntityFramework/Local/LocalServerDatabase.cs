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

    public override Server<TEntryPoint> Build<TContext>()
    {
        Builder.ActionsOnServiceCollection.Add(RemoveDbContext<TContext>);

        switch (DbOptions.DbType)
        {
            case LocalDbType.SqlServer:
                Builder.ActionsOnServiceCollection.Add(AddDbContextForLocalSqlServer<TContext>);
                break;
            case LocalDbType.Sqlite:
                Builder.ActionsOnServiceCollection.Add(AddDbContextForLocalSqlite<TContext>);
                break;
        }

        return base.Build<TContext>();
    }

    public Server<TEntryPoint> Build<TOriginalContext, TNewContext>()
        where TOriginalContext : DbContext
        where TNewContext : TOriginalContext
    {
        Builder.ActionsOnServiceCollection.Add(RemoveDbContext<TOriginalContext>);
        
        switch (DbOptions.DbType)
        {
            case LocalDbType.SqlServer:
                Builder.ActionsOnServiceCollection.Add(AddDbContextForLocalSqlServer<TNewContext>);
                break;
            case LocalDbType.Sqlite:
                Builder.ActionsOnServiceCollection.Add(AddDbContextForLocalSqlite<TNewContext>);
                break;
        }

        return base.Build<TNewContext>();
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

        //RemoveDbContext<TContext>(serviceCollection);
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

        //RemoveDbContext<TContext>(serviceCollection);
        serviceCollection.AddDbContext<TContext>(o => o.UseSqlite(DbOptions.ConnectionString));
    }
}