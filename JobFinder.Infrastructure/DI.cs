using JobFinder.Infrastructure.Repositories;
using JobFinder.UseCases.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace JobFinder.Infrastructure;

public static class DI
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IJobPostingRepository, JobPostingRepository>();
        services.AddScoped<ICandidateRepository, CandidateRepository>();
        services.AddScoped<IEmployerRepository, EmployerRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();

        return services;
    }
}