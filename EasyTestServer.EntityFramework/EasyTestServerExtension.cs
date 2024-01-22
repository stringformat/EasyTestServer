namespace EasyTestServer.EntityFramework;

public static class EasyTestServerExtension
{
    public static EasyTestServerDatabase UseDatabase(this Core.EasyTestServer builder)
        => new(builder);
}