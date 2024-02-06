namespace EasyTestServer.EntityFramework;

public static class ServerExtension
{
    public static ServerDatabase<TEntryPoint> UseDatabase<TEntryPoint>(this Server<TEntryPoint> builder) 
        where TEntryPoint : class
    {
        return new ServerDatabase<TEntryPoint>(builder);
    }
}