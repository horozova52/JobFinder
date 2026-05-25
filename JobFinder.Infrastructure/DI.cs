using JobFinder.Infrastructure.Data;
using JobFinder.Infrastructure.Repositories;
using JobFinder.UseCases.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace JobFinder.Infrastructure;

public static class DI
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Repositories existente
        services.AddScoped<IJobPostingRepository, JobPostingRepository>();
        services.AddScoped<ICandidateRepository, CandidateRepository>();
        services.AddScoped<IEmployerRepository, EmployerRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IEducationRepository, EducationRepository>();
        services.AddScoped<IExperienceRepository, ExperienceRepository>();
        services.AddScoped<ICandidateLanguageRepository, CandidateLanguageRepository>();
        services.AddScoped<ICandidateSkillRepository, CandidateSkillRepository>();
        services.AddScoped<IJobFeedRepository, JobFeedRepository>();
        services.AddScoped<DataSeeder>();
        services.AddScoped<DemoSeeder>();
        services.AddScoped<IEmploymentConfirmationRepository, EmploymentConfirmationRepository>();
        return services;
    }
}