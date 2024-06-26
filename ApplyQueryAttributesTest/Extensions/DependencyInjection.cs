using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace D8.Core.Extensions;

public static class IDependencyInjectionExtensions
{

    //TODO: Add a method to include any class that uses DependencyInjectionAttribute - but I'd probably need a property for the interface 

    public static IServiceCollection AddDerivedTypes<T>(this IServiceCollection services, string? nameEndsWith = null)
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(domainAssembly => domainAssembly.GetTypes())
            .Where(type =>
                (typeof(T).IsAssignableFrom(type)
                || (nameEndsWith != null && type.Name.EndsWith(nameEndsWith, StringComparison.OrdinalIgnoreCase)))
                && type != typeof(T)
                && !type.IsAbstract
            ).ToList();

        foreach (var type in types)
        {
            var attribute = type.GetCustomAttributes<DependencyInjectedAttribute>(true).FirstOrDefault();
            if (attribute == null || attribute.Lifetime == DependencyInjectionLifetime.Transient)
                services.TryAddTransient(type);
            else if (attribute.Lifetime == DependencyInjectionLifetime.Singleton)
                services.TryAddSingleton(type);
            else if (attribute.Lifetime == DependencyInjectionLifetime.Scoped)
                services.TryAddScoped(type);
        }

        return services;
    }

}

