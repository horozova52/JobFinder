using JobFinder.Core.Entities.Identity;
using JobFinder.Core.Entities.Common;
using JobFinder.Core.Entities.Jobs;
using JobFinder.Infrastructure;
using JobFinder.Infrastructure.Data;
using JobFinder.Server.Components;
using JobFinder.Server.Components.Account;
using JobFinder.Shared.Enums;
using JobFinder.UseCases;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using JobFinder.Server.Hubs;
using JobFinder.UseCases.Contracts;
using JobFinder.Infrastructure.Repositories;

namespace JobFinder;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddMudServices();
        builder.Services.AddHttpContextAccessor();

        // Blazor + Identity UI components
        builder.Services.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        
        builder.Services.AddInfrastructure();
        builder.Services.AddUseCases();
        builder.Services.AddControllers();
        builder.Services.AddSignalR();

        // Add Swagger/OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Identity
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
            .AddIdentityCookies();

        builder.Services.AddAuthorization();

        builder.Services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
        builder.Services.AddScoped<IExperienceRepository, ExperienceRepository>();
        builder.Services.AddScoped<ICandidateLanguageRepository, CandidateLanguageRepository>();
        builder.Services.AddScoped<ICandidateSkillRepository, CandidateSkillRepository>();
        builder.Services.AddScoped<IEmployerRepository, EmployerRepository>();
        /// builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddScoped(sp =>
        {
            var request = sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request;
            var baseAddress = request != null
                ? $"{request.Scheme}://{request.Host}"
                : "http://localhost:5054";
            return new HttpClient { BaseAddress = new Uri(baseAddress) };
        });
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        });
        var app = builder.Build();

        // Seed-ul rolurilor (Candidate, Employer, Admin)
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = { "Candidate", "Employer", "Admin" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed Skills + Sample JobPostings
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!await db.Skills.AnyAsync())
            {
                var skills = new List<Skill>
                {
                    // Programming
                    new() { Name = "C#", Category = "Programming" },
                    new() { Name = ".NET", Category = "Programming" },
                    new() { Name = "ASP.NET Core", Category = "Programming" },
                    new() { Name = "JavaScript", Category = "Programming" },
                    new() { Name = "TypeScript", Category = "Programming" },
                    new() { Name = "React", Category = "Programming" },
                    new() { Name = "Angular", Category = "Programming" },
                    new() { Name = "Vue.js", Category = "Programming" },
                    new() { Name = "Node.js", Category = "Programming" },
                    new() { Name = "Python", Category = "Programming" },
                    new() { Name = "Java", Category = "Programming" },
                    new() { Name = "PHP", Category = "Programming" },
                    new() { Name = "Ruby", Category = "Programming" },
                    new() { Name = "Go", Category = "Programming" },
                    new() { Name = "Rust", Category = "Programming" },
                    new() { Name = "Swift", Category = "Programming" },
                    new() { Name = "Kotlin", Category = "Programming" },
                    // Databases
                    new() { Name = "SQL", Category = "Databases" },
                    new() { Name = "PostgreSQL", Category = "Databases" },
                    new() { Name = "MongoDB", Category = "Databases" },
                    new() { Name = "Redis", Category = "Databases" },
                    new() { Name = "MySQL", Category = "Databases" },
                    // DevOps
                    new() { Name = "Docker", Category = "DevOps" },
                    new() { Name = "Kubernetes", Category = "DevOps" },
                    new() { Name = "AWS", Category = "DevOps" },
                    new() { Name = "Azure", Category = "DevOps" },
                    new() { Name = "CI/CD", Category = "DevOps" },
                    new() { Name = "Git", Category = "DevOps" },
                    // Design
                    new() { Name = "Figma", Category = "Design" },
                    new() { Name = "UI/UX", Category = "Design" },
                    new() { Name = "Adobe Photoshop", Category = "Design" },
                    new() { Name = "CSS", Category = "Design" },
                    new() { Name = "Tailwind CSS", Category = "Design" },
                    new() { Name = "HTML", Category = "Design" },
                    // Other
                    new() { Name = "Agile", Category = "Other" },
                    new() { Name = "Scrum", Category = "Other" },
                    new() { Name = "REST API", Category = "Other" },
                    new() { Name = "GraphQL", Category = "Other" },
                    new() { Name = "Linux", Category = "Other" },
                    new() { Name = "Testing", Category = "Other" },
                    new() { Name = "Machine Learning", Category = "Other" },
                };
                db.Skills.AddRange(skills);
                await db.SaveChangesAsync();
            }

            if (!await db.JobPostings.AnyAsync() && await db.EmployerProfiles.AnyAsync())
            {
                var employer = await db.EmployerProfiles.FirstAsync();
                var allSkills = await db.Skills.ToListAsync();
                Skill? Sk(string n) => allSkills.FirstOrDefault(s => s.Name == n);

                var jobs = new List<JobPosting>
                {
                    new()
                    {
                        EmployerProfileId = employer.Id, Title = "Full Stack Developer",
                        Description = "Dezvoltă aplicații web complete folosind .NET și React.",
                        Requirements = "3+ ani experiență C#, React, SQL",
                        Responsibilities = "Dezvoltare backend și frontend, code review, deploy",
                        Location = "București", JobType = JobType.Hybrid, EmploymentType = EmploymentType.FullTime,
                        SalaryFrom = 8000, SalaryTo = 14000, IsSalaryNegotiable = true,
                        Status = JobStatus.Published, CreatedAt = DateTime.UtcNow, PublishedAt = DateTime.UtcNow,
                        Skills = new List<JobSkill>
                        {
                            new() { SkillId = Sk("C#")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("React")!.Id, RequiredLevel = SkillLevel.Intermediate },
                            new() { SkillId = Sk("SQL")!.Id, RequiredLevel = SkillLevel.Intermediate },
                            new() { SkillId = Sk(".NET")!.Id, RequiredLevel = SkillLevel.Advanced },
                        }
                    },
                    new()
                    {
                        EmployerProfileId = employer.Id, Title = "Frontend React Developer",
                        Description = "Construiește interfețe moderne pentru platforma SaaS.",
                        Requirements = "2+ ani React, TypeScript, CSS",
                        Responsibilities = "Dezvoltare componente UI, integrare API",
                        Location = "Remote", JobType = JobType.Remote, EmploymentType = EmploymentType.FullTime,
                        SalaryFrom = 7000, SalaryTo = 12000, IsSalaryNegotiable = false,
                        Status = JobStatus.Published, CreatedAt = DateTime.UtcNow, PublishedAt = DateTime.UtcNow,
                        Skills = new List<JobSkill>
                        {
                            new() { SkillId = Sk("React")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("TypeScript")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("CSS")!.Id, RequiredLevel = SkillLevel.Intermediate },
                            new() { SkillId = Sk("Git")!.Id, RequiredLevel = SkillLevel.Intermediate },
                        }
                    },
                    new()
                    {
                        EmployerProfileId = employer.Id, Title = "Backend .NET Engineer",
                        Description = "Dezvoltă microservicii performante în .NET.",
                        Requirements = "4+ ani .NET, Docker, SQL Server",
                        Responsibilities = "Arhitectură microservicii, CI/CD, optimizare performanță",
                        Location = "Cluj-Napoca", JobType = JobType.Hybrid, EmploymentType = EmploymentType.FullTime,
                        SalaryFrom = 10000, SalaryTo = 16000, IsSalaryNegotiable = true,
                        Status = JobStatus.Published, CreatedAt = DateTime.UtcNow, PublishedAt = DateTime.UtcNow,
                        Skills = new List<JobSkill>
                        {
                            new() { SkillId = Sk("C#")!.Id, RequiredLevel = SkillLevel.Expert },
                            new() { SkillId = Sk(".NET")!.Id, RequiredLevel = SkillLevel.Expert },
                            new() { SkillId = Sk("Docker")!.Id, RequiredLevel = SkillLevel.Intermediate },
                            new() { SkillId = Sk("SQL")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("CI/CD")!.Id, RequiredLevel = SkillLevel.Intermediate },
                        }
                    },
                    new()
                    {
                        EmployerProfileId = employer.Id, Title = "DevOps Engineer",
                        Description = "Gestionează infrastructura cloud și pipeline-urile CI/CD.",
                        Requirements = "3+ ani Docker, Kubernetes, AWS/Azure",
                        Responsibilities = "Automatizare deployment, monitoring, securitate",
                        Location = "Remote", JobType = JobType.Remote, EmploymentType = EmploymentType.Contract,
                        SalaryFrom = 12000, SalaryTo = 18000, IsSalaryNegotiable = true,
                        Status = JobStatus.Published, CreatedAt = DateTime.UtcNow, PublishedAt = DateTime.UtcNow,
                        Skills = new List<JobSkill>
                        {
                            new() { SkillId = Sk("Docker")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("Kubernetes")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("AWS")!.Id, RequiredLevel = SkillLevel.Intermediate },
                            new() { SkillId = Sk("CI/CD")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("Linux")!.Id, RequiredLevel = SkillLevel.Advanced },
                        }
                    },
                    new()
                    {
                        EmployerProfileId = employer.Id, Title = "Python Data Engineer",
                        Description = "Construiește pipeline-uri de date și modele ML.",
                        Requirements = "2+ ani Python, SQL, Machine Learning",
                        Responsibilities = "ETL, analiză date, modele predictive",
                        Location = "Iași", JobType = JobType.OnSite, EmploymentType = EmploymentType.FullTime,
                        SalaryFrom = 9000, SalaryTo = 15000, IsSalaryNegotiable = false,
                        Status = JobStatus.Published, CreatedAt = DateTime.UtcNow, PublishedAt = DateTime.UtcNow,
                        Skills = new List<JobSkill>
                        {
                            new() { SkillId = Sk("Python")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("SQL")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("Machine Learning")!.Id, RequiredLevel = SkillLevel.Intermediate },
                            new() { SkillId = Sk("Docker")!.Id, RequiredLevel = SkillLevel.Beginner },
                        }
                    },
                    new()
                    {
                        EmployerProfileId = employer.Id, Title = "UI/UX Designer",
                        Description = "Creează experiențe digitale intuitive și atractive.",
                        Requirements = "2+ ani Figma, UI/UX, prototipare",
                        Responsibilities = "Design interfețe, wireframing, user research",
                        Location = "Timișoara", JobType = JobType.Hybrid, EmploymentType = EmploymentType.FullTime,
                        SalaryFrom = 6000, SalaryTo = 10000, IsSalaryNegotiable = true,
                        Status = JobStatus.Published, CreatedAt = DateTime.UtcNow, PublishedAt = DateTime.UtcNow,
                        Skills = new List<JobSkill>
                        {
                            new() { SkillId = Sk("Figma")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("UI/UX")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("Adobe Photoshop")!.Id, RequiredLevel = SkillLevel.Intermediate },
                            new() { SkillId = Sk("HTML")!.Id, RequiredLevel = SkillLevel.Beginner },
                            new() { SkillId = Sk("CSS")!.Id, RequiredLevel = SkillLevel.Beginner },
                        }
                    },
                    new()
                    {
                        EmployerProfileId = employer.Id, Title = "Java Backend Developer",
                        Description = "Dezvoltă servicii enterprise robuste în Java.",
                        Requirements = "3+ ani Java, Spring Boot, PostgreSQL",
                        Responsibilities = "Dezvoltare API REST, integrări externe, testare",
                        Location = "București", JobType = JobType.OnSite, EmploymentType = EmploymentType.FullTime,
                        SalaryFrom = 9000, SalaryTo = 14000, IsSalaryNegotiable = false,
                        Status = JobStatus.Published, CreatedAt = DateTime.UtcNow, PublishedAt = DateTime.UtcNow,
                        Skills = new List<JobSkill>
                        {
                            new() { SkillId = Sk("Java")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("PostgreSQL")!.Id, RequiredLevel = SkillLevel.Intermediate },
                            new() { SkillId = Sk("REST API")!.Id, RequiredLevel = SkillLevel.Advanced },
                            new() { SkillId = Sk("Git")!.Id, RequiredLevel = SkillLevel.Intermediate },
                        }
                    },
                };
                db.JobPostings.AddRange(jobs);
                await db.SaveChangesAsync();
            }
        }

        // Pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            app.UseMigrationsEndPoint();
            // Enable Swagger UI in Development
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "JobFinder API V1");
                // default RoutePrefix is "swagger" -> UI available at /swagger
            });
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.UseStaticFiles();
        app.MapControllers();
        app.MapHub<ChatHub>("/hubs/chat");
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.MapAdditionalIdentityEndpoints();

        await app.RunAsync();
    }
}
