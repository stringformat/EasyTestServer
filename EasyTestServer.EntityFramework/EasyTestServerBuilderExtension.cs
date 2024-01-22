using EasyTestServer.Builder;

namespace EasyTestServer.EntityFramework;

public static class EasyTestServerBuilderExtension
{
    public static EasyTestServerDatabaseBuilder UseDatabase(this EasyTestServerBuilder builder)
        => new(builder);
}