using System;
using D8.Core.Extensions;

namespace D8.Maui.Extensions;

public static class PageServiceExtensions
{
    public static IServiceCollection AddPages(this IServiceCollection services)
    {
        services.AddDerivedTypes<ContentPage>("Page");

        return services;
    }
}

