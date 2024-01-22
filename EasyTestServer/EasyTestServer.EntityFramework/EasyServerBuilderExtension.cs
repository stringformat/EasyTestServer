using EasyTestServer.Builder;

namespace EasyTestServer.EntityFramework;

public static class EasyServerBuilderExtension
{
    public static DatabaseBuilder UseDatabase(this EasyTestServerBuilder builder)
        => new(builder);
}