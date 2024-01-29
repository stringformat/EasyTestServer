using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace EasyTestServer.Core;

public class Server
{
    private readonly Collection<(string key, string value)> _settings = [];
    public Collection<Action<IServiceCollection>> ActionsOnServiceCollection { get; } = [];

    public Server WithService<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        ActionsOnServiceCollection.Add(services => services.ReplaceService<TService, TImplementation>());
        return this;
    }

    public Server WithService<TService>(TService service)
        where TService : class
    {
        ActionsOnServiceCollection.Add(services => services.ReplaceService<TService>(service));
        return this;
    }

    public Server WithoutService<TService>()
        where TService : class
    {
        ActionsOnServiceCollection.Add(services => services.RemoveService<TService>());
        return this;
    }

    public Server WithSubstitute<TService>(out TService substitute)
        where TService : class
    {
        var service = Substitute.For<TService>();
        ActionsOnServiceCollection.Add(services => services.AddOrReplaceService<TService>(service));
        
        substitute = service;

        return this;
    }

    public Server WithSetting(string key, string value)
    {
        _settings.Add((key, value));

        return this;
    }

    public TestServer Build<TEntryPoint>(
        string? environment = null, 
        string? contentRoot = null,
        params string[] urls) where TEntryPoint : class
    {
        return new WebApplicationFactory<TEntryPoint>()
            .WithWebHostBuilder(webBuilder =>
            {
                webBuilder
                    .ConfigureTestServices(services =>
                    {
                        foreach (var action in ActionsOnServiceCollection)
                            action(services);
                    });

                foreach (var (key, value) in _settings) 
                    webBuilder.UseSetting(key, value);

                if (environment is not null) 
                    webBuilder.UseEnvironment(environment);

                if (contentRoot is not null)
                    webBuilder.UseContentRoot(contentRoot);

                webBuilder.UseUrls(urls);
            }).Server;
    }
}