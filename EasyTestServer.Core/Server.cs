using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace EasyTestServer.Core;

public class Server<TEntryPoint> where TEntryPoint : class
{
    private readonly Options _serverOptions = new();
    private readonly Collection<(string key, string value)> _settings = [];
    private readonly Collection<ILoggerProvider> _loggerProviders = [];
    private WebApplicationFactory<TEntryPoint> _testServer = null!;
    
    public readonly Collection<Action<IServiceCollection>> ActionsOnServiceCollection = [];
    // public readonly Collection<Action> DisposableServices = [];
    // public readonly Collection<Func<ValueTask>> DisposableServicesAsync = [];

    public Server<TEntryPoint> WithService<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        ActionsOnServiceCollection.Add(services => services.ReplaceService<TService, TImplementation>());
        return this;
    }

    public Server<TEntryPoint> WithService<TService>(TService service)
        where TService : class
    {
        ActionsOnServiceCollection.Add(services => services.ReplaceService<TService>(service));
        return this;
    }

    public Server<TEntryPoint> WithoutService<TService>()
        where TService : class
    {
        ActionsOnServiceCollection.Add(services => services.RemoveService<TService>());
        return this;
    }

    public Server<TEntryPoint> WithSubstitute<TService>(out TService substitute)
        where TService : class
    {
        var service = Substitute.For<TService>();
        ActionsOnServiceCollection.Add(services => services.AddOrReplaceService<TService>(service));

        substitute = service;

        return this;
    }

    public Server<TEntryPoint> WithSetting(string key, string value)
    {
        _settings.Add((key, value));

        return this;
    }

    public Server<TEntryPoint> WithLoggerProvider(ILoggerProvider loggerProvider)
    {
        _loggerProviders.Add(loggerProvider);

        return this;
    }

    public HttpClient Build()
    {
        _testServer = new WebApplicationFactory<TEntryPoint>()
            .WithWebHostBuilder(webBuilder =>
            {
                ConfigureTestServices(webBuilder);
                ConfigureLogging(webBuilder);
                ConfigureOptions(webBuilder);
            });

        var webApplicationFactoryClientOptions = new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = _serverOptions.AllowAutoRedirect
        };

        if (_serverOptions.BaseAddress is not null)
            webApplicationFactoryClientOptions.BaseAddress = _serverOptions.BaseAddress;

        var client = _testServer.CreateClient(webApplicationFactoryClientOptions);
        
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme: "operator");

        return client;
    }
    
    public HttpClient Build(Action<Options> options)
    {
        options(_serverOptions);

        return Build();
    }

    private void ConfigureOptions(IWebHostBuilder webBuilder)
    {
        foreach (var (key, value) in _settings)
            webBuilder.UseSetting(key, value);

        if (_serverOptions.Environment is not null)
            webBuilder.UseEnvironment(_serverOptions.Environment);

        if (_serverOptions.ContentRoot is not null)
            webBuilder.UseContentRoot(_serverOptions.ContentRoot);

        if (_serverOptions.SolutionRelativeContentRoot is not null)
            webBuilder.UseSolutionRelativeContentRoot(_serverOptions.SolutionRelativeContentRoot);

        webBuilder.UseUrls(_serverOptions.Urls);
    }

    private void ConfigureLogging(IWebHostBuilder webBuilder)
    {
        webBuilder.ConfigureLogging(builder =>
        {
            if (_serverOptions.DisableLogging)
                builder.ClearProviders();

            foreach (var loggerProvider in _loggerProviders)
                builder.AddProvider(loggerProvider);
        });
    }

    private void ConfigureTestServices(IWebHostBuilder webBuilder)
    {
        webBuilder.ConfigureTestServices(services =>
        {
            foreach (var action in ActionsOnServiceCollection)
                action(services);

            if (_serverOptions.DisableAuthentication)
                services.AddAuthentication(defaultScheme: "operator")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "operator", options => { });
                //services.TryReplaceService<IPolicyEvaluator>(new PolicyEvaluator());
        });
    }
}