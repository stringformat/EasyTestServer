using System.Collections.ObjectModel;
using EasyTestServer.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace EasyTestServer.Core;

public class EasyTestServerBuilder(string? environment = null)
{
    public Collection<Action<IServiceCollection>> ActionsOnServiceCollection { get; } = new();

    public EasyTestServerBuilder WithService<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        ActionsOnServiceCollection.Add(services => services.ReplaceService<TService, TImplementation>());
        return this;
    }

    public EasyTestServerBuilder WithService<TService>(TService service)
        where TService : class
    {
        ActionsOnServiceCollection.Add(services => services.ReplaceService<TService>(service));
        return this;
    }

    public EasyTestServerBuilder WithoutService<TService>()
        where TService : class
    {
        ActionsOnServiceCollection.Add(services => services.RemoveService<TService>());
        return this;
    }

    public EasyTestServerBuilder WithSubstitute<TService>(out TService substitute)
        where TService : class
    {
        var service = Substitute.For<TService>();
        ActionsOnServiceCollection.Add(services => services.AddOrReplaceService<TService>(service));
        
        substitute = service;

        return this;
    }

    public TestServer Build<TEntryPoint>() where TEntryPoint : class
    {
        return new WebApplicationFactory<TEntryPoint>()
            .WithWebHostBuilder(webBuilder =>
            {
                webBuilder
                    //.UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureServices(services =>
                    {
                        foreach (var action in ActionsOnServiceCollection)
                            action(services);
                    });

                if (environment is not null) webBuilder.UseEnvironment(environment);
            }).Server;
    }
}