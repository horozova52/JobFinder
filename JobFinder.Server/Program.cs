using JobFinder.Core.Entities.Identity;
using JobFinder.Infrastructure;
using JobFinder.Infrastructure.Data;
using JobFinder.Server.Components;
using JobFinder.Server.Components.Account;
using JobFinder.Server.Hubs;
using JobFinder.UseCases;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

namespace JobFinder;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── UI & Components ───────────────────────────────────────────
        builder.Services.AddMudServices();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();

        // ── Database ──────────────────────────────────────────────────
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // ── Infrastructure + UseCases ─────────────────────────────────
        // Toate repository-urile, DataSeeder, IJobFeedRepository
        // sunt înregistrate în AddInfrastructure()
        builder.Services.AddInfrastructure();
        builder.Services.AddUseCases();

        // ── Controllers & API ─────────────────────────────────────────
        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // ── Identity ──────────────────────────────────────────────────
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

        // ── HttpClient ────────────────────────────────────────────────
        builder.Services.AddScoped(sp =>
        {
            var request = sp.GetRequiredService<IHttpContextAccessor>()
                            .HttpContext?.Request;
            var baseAddress = request != null
                ? $"{request.Scheme}://{request.Host}"
                : "http://localhost:5054";
            return new HttpClient { BaseAddress = new Uri(baseAddress) };
        });

        // ── Cookie redirect pentru API ────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────
        var app = builder.Build();

        // ── Seed 1: Roluri Identity ───────────────────────────────────
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in new[] { "Candidate", "Employer", "Admin" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ── Seed 2: Skills + Angajatori + Joburi ─────────────────────
        using (var scope = app.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
        }

        // ── Pipeline ──────────────────────────────────────────────────
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            app.UseMigrationsEndPoint();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "JobFinder API V1"));
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