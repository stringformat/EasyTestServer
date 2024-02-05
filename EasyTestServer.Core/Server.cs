using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace EasyTestServer.Core;

public class Server
{
    private readonly ServerOptions _serverOptions = new();
    private readonly Collection<(string key, string value)> _settings = [];
    private readonly Collection<ILoggerProvider> _loggerProviders = [];
    
    public readonly Collection<Action<IServiceCollection>> ActionsOnServiceCollection = [];

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

    public Server WithLoggerProvider(ILoggerProvider loggerProvider)
    {
        _loggerProviders.Add(loggerProvider);

        return this;
    }

    public HttpClient Build<TEntryPoint>() where TEntryPoint : class
    {
        var testServer = new WebApplicationFactory<TEntryPoint>()
            .WithWebHostBuilder(webBuilder =>
            {
                webBuilder.ConfigureTestServices(services =>
                {
                    foreach (var action in ActionsOnServiceCollection)
                        action(services);

                    if (_serverOptions.DisableAuthentication)
                        services.ReplaceService<IPolicyEvaluator>(new PolicyEvaluator());
                });

                webBuilder.ConfigureLogging(builder =>
                {
                    if (_serverOptions.DisableLogging)
                        builder.ClearProviders();

                    foreach (var loggerProvider in _loggerProviders)
                        builder.AddProvider(loggerProvider);
                });

                foreach (var (key, value) in _settings)
                    webBuilder.UseSetting(key, value);

                if (_serverOptions.Environment is not null)
                    webBuilder.UseEnvironment(_serverOptions.Environment);

                if (_serverOptions.ContentRoot is not null)
                    webBuilder.UseContentRoot(_serverOptions.ContentRoot);

                webBuilder.UseUrls(_serverOptions.Urls);
            });

        var webApplicationFactoryClientOptions = new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        };

        if (_serverOptions.BaseAddress is not null)
            webApplicationFactoryClientOptions.BaseAddress = _serverOptions.BaseAddress;

        return testServer.CreateClient(webApplicationFactoryClientOptions);
    }
    
    public HttpClient Build<TEntryPoint>(Action<ServerOptions> options) where TEntryPoint : class
    {
        options(_serverOptions);

        return Build<TEntryPoint>();
    }
}