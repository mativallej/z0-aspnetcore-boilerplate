using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Z0.Application.Mappings;
using Z0.Application.Services;

namespace Z0.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Services
        services.AddScoped<IItemService, ItemService>();

        return services;
    }
}
