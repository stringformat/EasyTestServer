using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace EasyTestServer.EntityFramework.InMemory;

public class InMemoryServerDatabase<TEntryPoint> : ServerDatabaseBase<TEntryPoint, InMemoryOptions> where TEntryPoint : class
{
    public InMemoryServerDatabase(Server<TEntryPoint> builder) : base(builder, new InMemoryOptions())
    {
    }

    public override Server<TEntryPoint> Build<TContext>()
    {
        switch (DbOptions.DbType)
        {
            case InMemoryDbType.EF:
                Builder.ActionsOnServiceCollection.Add(ReplaceDbByInMemoryEF<TContext>);
                break;
            case InMemoryDbType.Sqlite:
                Builder.ActionsOnServiceCollection.Add(ReplaceDbByInMemorySqlite<TContext>);
                break;
        }

        return base.Build<TContext>();
    }
    
    private static void ReplaceDbByInMemoryEF<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        RemoveDbContext<TContext>(serviceCollection);
        serviceCollection.AddDbContext<TContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString(), optionsBuilder => optionsBuilder.UseHierarchyId()));
    }
    
    private static void ReplaceDbByInMemorySqlite<TContext>(IServiceCollection serviceCollection)
        where TContext : DbContext
    {
        RemoveDbContext<TContext>(serviceCollection);
        
        serviceCollection.AddSingleton<DbConnection, SqliteConnection>(_ =>
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            return connection;
        });

        serviceCollection.AddDbContext<TContext>((serviceProvider, options) =>
        {
            var connection = serviceProvider.GetRequiredService<DbConnection>();
            options.UseSqlite(connection);
        });
        
        //serviceCollection.AddDbContext<TContext>(o => o.UseSqlite("DataSource=:memory:"));
    }
}