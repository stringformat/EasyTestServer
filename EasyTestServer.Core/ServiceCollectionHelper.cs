using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyTestServer.Core;

public static class ServiceCollectionHelper
{
    public static IServiceCollection RemoveService<TService>(this IServiceCollection services)
        where TService : class
    {
        var descriptor = services.TryFindDescriptor(typeof(TService));
        if (descriptor is null) 
            return services;

        services.Remove(descriptor);

        return services;
    }

    public static IServiceCollection AddOrReplaceService<TService>(
        this IServiceCollection services,
        object service,
        ServiceLifetime lifetimeOnAdd = ServiceLifetime.Transient) 
        where TService : class
    {
        var descriptor = services.TryFindDescriptor<TService>();
        if (descriptor is not null)
            return services.Replace(descriptor.ToReplaceDescriptor(service));

        var addDescriptor = new ServiceDescriptor(typeof(TService), _ => service, lifetimeOnAdd);
        services.Add(addDescriptor);
        return services;
    }
    
    public static IServiceCollection ReplaceService<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        var descriptor = services.FindDescriptor(typeof(TService));
        services.Replace(new ServiceDescriptor(typeof(TService), typeof(TImplementation), descriptor.Lifetime));
        return services;
    }
    
    public static IServiceCollection TryReplaceService<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        var descriptor = services.TryFindDescriptor<TService>();
        if (descriptor is not null)
            services.Replace(new ServiceDescriptor(typeof(TService), typeof(TImplementation), descriptor.Lifetime));
        
        return services;
    }

    public static IServiceCollection ReplaceService<TService>(this IServiceCollection services, object service)
        where TService : class
    {
        var serviceType = typeof(TService);
        if (!serviceType.IsInstanceOfType(service))
            throw new InvalidOperationException($"{service.GetType().Name} is not an instance of {serviceType.Name}");

        var newDescriptor = services.FindDescriptor(serviceType).ToReplaceDescriptor(service);
        return services.Replace(newDescriptor);
    }
    
    public static IServiceCollection TryReplaceService<TService>(this IServiceCollection services, object service)
        where TService : class
    {
        var descriptor = services.TryFindDescriptor<TService>();
        if (descriptor is null) 
            return services;
        
        var serviceType = typeof(TService);
        if (!serviceType.IsInstanceOfType(service))
            throw new InvalidOperationException($"{service.GetType().Name} is not an instance of {serviceType.Name}");

        var newDescriptor = services.FindDescriptor(serviceType).ToReplaceDescriptor(service);
        return services.Replace(newDescriptor);
    }

    private static ServiceDescriptor ToReplaceDescriptor(this ServiceDescriptor source, object service)
        => new(source.ServiceType, _ => service, source.Lifetime);

    private static ServiceDescriptor? TryFindDescriptor<TService>(this IServiceCollection services)
        where TService : class
        => services.TryFindDescriptor(typeof(TService));

    private static ServiceDescriptor? TryFindDescriptor(this IServiceCollection services, Type serviceType)
        => services.SingleOrDefault(d => d.ServiceType == serviceType);

    private static ServiceDescriptor FindDescriptor(this IServiceCollection services, Type serviceType)
    {
        return services.TryFindDescriptor(serviceType) ?? throw new NotSupportedException($"{serviceType.Name} does not exist");
    }
}