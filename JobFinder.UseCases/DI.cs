using FluentValidation;
using JobFinder.UseCases.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace JobFinder.UseCases;

public static class DI
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DI).Assembly));

        services.AddAutoMapper(typeof(DI).Assembly);

        services.AddValidatorsFromAssembly(typeof(DI).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}