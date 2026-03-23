using JobFinder.Core.Entities.Applications;
using JobFinder.Core.Entities.Candidates;
using JobFinder.Core.Entities.Common;
using JobFinder.Core.Entities.Documents;
using JobFinder.Core.Entities.Employers;
using JobFinder.Core.Entities.Identity;
using JobFinder.Core.Entities.Jobs;
using JobFinder.Core.Entities.Messaging;
using JobFinder.Core.Entities.Validation;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    // Candidates
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<CandidateLanguage> CandidateLanguages => Set<CandidateLanguage>();

    // Employers
    public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();
    public DbSet<CompanyLocation> CompanyLocations => Set<CompanyLocation>();

    // Common
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Language> Languages => Set<Language>();

    // Jobs
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobCategory> JobCategories => Set<JobCategory>();
    public DbSet<JobSkill> JobSkills => Set<JobSkill>();

    // Applications
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories => Set<ApplicationStatusHistory>();

    // Validation
    public DbSet<EmployerVerification> EmployerVerifications => Set<EmployerVerification>();

    // Documents
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentTemplate> DocumentTemplates => Set<DocumentTemplate>();

    // Messaging
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // foarte important: Identity
        base.OnModelCreating(modelBuilder);

        // CandidateProfile
        modelBuilder.Entity<CandidateProfile>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();

            entity.HasMany(e => e.Experiences)
                  .WithOne(x => x.CandidateProfile)
                  .HasForeignKey(x => x.CandidateProfileId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Educations)
                  .WithOne(x => x.CandidateProfile)
                  .HasForeignKey(x => x.CandidateProfileId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Skills)
                  .WithOne(x => x.CandidateProfile)
                  .HasForeignKey(x => x.CandidateProfileId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Certifications)
                  .WithOne(x => x.CandidateProfile)
                  .HasForeignKey(x => x.CandidateProfileId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Languages)
                  .WithOne(x => x.CandidateProfile)
                  .HasForeignKey(x => x.CandidateProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // EmployerProfile
        modelBuilder.Entity<EmployerProfile>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CompanyName).HasMaxLength(200).IsRequired();

            entity.HasMany(e => e.Locations)
                  .WithOne(x => x.EmployerProfile)
                  .HasForeignKey(x => x.EmployerProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // JobPosting
        modelBuilder.Entity<JobPosting>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).IsRequired();

            entity.HasOne(e => e.EmployerProfile)
                  .WithMany()
                  .HasForeignKey(e => e.EmployerProfileId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Skills)
                  .WithOne(x => x.JobPosting)
                  .HasForeignKey(x => x.JobPostingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Application
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.JobPosting)
                  .WithMany()
                  .HasForeignKey(e => e.JobPostingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CandidateProfile)
                  .WithMany()
                  .HasForeignKey(e => e.CandidateProfileId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.StatusHistory)
                  .WithOne(x => x.Application)
                  .HasForeignKey(x => x.ApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // EmployerVerification
        modelBuilder.Entity<EmployerVerification>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.EmployerProfile)
                  .WithMany()
                  .HasForeignKey(e => e.EmployerProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Lookup tables
        modelBuilder.Entity<Skill>().HasKey(e => e.Id);
        modelBuilder.Entity<Language>().HasKey(e => e.Id);
        modelBuilder.Entity<JobCategory>().HasKey(e => e.Id);
        modelBuilder.Entity<ApplicationStatusHistory>().HasKey(e => e.Id);

        // Conversation
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Application)
                  .WithMany()
                  .HasForeignKey(e => e.ApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Messages)
                  .WithOne(m => m.Conversation)
                  .HasForeignKey(m => m.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.SenderUserId).HasMaxLength(450).IsRequired();
        });
    }
}
