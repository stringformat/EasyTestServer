namespace EasyTestServer.EntityFramework;

public static class ServerExtension
{
    public static ServerDatabase UseDatabase(this Server builder)
        => new(builder);
}