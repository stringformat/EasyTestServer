using EasyTestServer.EntityFramework.InMemory;
using EasyTestServer.EntityFramework.Local;

namespace EasyTestServer.EntityFramework;

public static class ServerExtension
{
    public static ServerDatabaseBase<TEntryPoint, InMemoryOptions> UseInMemoryDatabase<TEntryPoint>(this Server<TEntryPoint> builder) 
        where TEntryPoint : class
    {
        return new InMemoryServerDatabase<TEntryPoint>(builder);
    }
    
    public static ServerDatabaseBase<TEntryPoint, LocalOptions> UseLocalDatabase<TEntryPoint>(this Server<TEntryPoint> builder) 
        where TEntryPoint : class
    {
        return new LocalServerDatabase<TEntryPoint>(builder);
    }
}