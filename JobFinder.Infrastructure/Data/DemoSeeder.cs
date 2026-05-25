using JobFinder.Core.Entities.Applications;
using JobFinder.Core.Entities.Candidates;
using JobFinder.Core.Entities.Common;
using JobFinder.Core.Entities.Employers;
using JobFinder.Core.Entities.Identity;
using JobFinder.Core.Entities.Jobs;
using JobFinder.Core.Entities.Messaging;
using JobFinder.Core.Entities.Validation;
using JobFinder.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Data;

/// <summary>
/// Seed dedicat exclusiv pentru demo-ul video.
/// Este COMPLET IZOLAT de DataSeeder — nu modifică și nu depinde de el.
/// Se rulează MANUAL prin endpoint-ul /api/demo/seed (vezi DemoSeedController).
///
/// Creează un set complet și coerent de date care acoperă toate
/// scenariile platformei: aplicări în toate statusurile, istoric de
/// tranziții, confirmări de angajare (validată + în așteptare),
/// conversații cu mesaje, notificări pentru ambele roluri și un
/// angajator neverificat pentru demonstrația de verificare admin.
///
/// Idempotent: dacă datele demo există deja (după email-urile fixe),
/// nu recreează nimic. Poate fi rulat de oricâte ori.
/// </summary>
public class DemoSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    // ── Parolă unică pentru toate conturile demo ──────────────────────
    private const string DemoPassword = "Demo@12345!";

    // ── Email-uri fixe (folosite și ca "santinele" de idempotență) ────
    private const string AdminEmail = "admin.demo@jobfinder.md";
    private const string EmployerEmail = "techcorp.demo@jobfinder.md";
    private const string EmployerUnverifiedEmail = "startup.demo@jobfinder.md";
    private const string Candidate1Email = "cristina.demo@jobfinder.md";
    private const string Candidate2Email = "andrei.demo@jobfinder.md";

    public DemoSeeder(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ══════════════════════════════════════════════════════════════════
    // PUNCT DE INTRARE
    // ══════════════════════════════════════════════════════════════════
    public async Task<string> SeedAsync(CancellationToken ct = default)
    {
        // ── Idempotență: dacă angajatorul demo există deja, ieșim ─────
        var alreadySeeded = await _db.EmployerProfiles
            .AnyAsync(e => e.ContactEmail == EmployerEmail, ct);

        if (alreadySeeded)
            return "Datele demo există deja. Nu s-a creat nimic nou.";

        // ── 1. Lookup tables (skills + limbi) ─────────────────────────
        var skills = await EnsureSkillsAsync(ct);
        var languages = await EnsureLanguagesAsync(ct);

        // ── 2. Conturi + profiluri ────────────────────────────────────
        await EnsureAdminAsync(ct);

        var techCorp = await EnsureEmployerAsync(
            email: EmployerEmail,
            companyName: "TechCorp SRL",
            isVerified: true,
            ct: ct);

        var startup = await EnsureEmployerAsync(
            email: EmployerUnverifiedEmail,
            companyName: "StartupHub SRL",
            isVerified: false,
            ct: ct);

        var cristina = await EnsureCandidate1Async(skills, languages, ct);
        var andrei = await EnsureCandidate2Async(skills, languages, ct);

        // ── 3. Cerere de verificare pentru angajatorul neverificat ────
        await EnsureVerificationRequestAsync(startup, ct);

        // ── 4. Joburi dedicate demo-ului (la TechCorp) ────────────────
        var jobs = await BuildDemoJobsAsync(techCorp, skills, ct);

        // ── 5. Aplicări care acoperă toate statusurile ────────────────
        await BuildApplicationsAsync(jobs, cristina, andrei, techCorp, ct);

        return "Seed demo finalizat cu succes. Conturi create: " +
               $"{AdminEmail}, {EmployerEmail}, {EmployerUnverifiedEmail}, " +
               $"{Candidate1Email}, {Candidate2Email}. Parolă: {DemoPassword}";
    }

    // ══════════════════════════════════════════════════════════════════
    // LOOKUP TABLES
    // ══════════════════════════════════════════════════════════════════
    private async Task<Dictionary<string, Skill>> EnsureSkillsAsync(CancellationToken ct)
    {
        var result = new Dictionary<string, Skill>(StringComparer.OrdinalIgnoreCase);

        var needed = new (string Name, string Category)[]
        {
            ("C#",            "IT & Software"),
            (".NET",          "IT & Software"),
            ("SQL",           "IT & Software"),
            ("JavaScript",    "IT & Software"),
            ("Git",           "IT & Software"),
            ("ASP.NET Core",  "IT & Software"),
            ("Blazor",        "IT & Software"),
            ("Google Ads",    "Marketing & PR"),
            ("SEO",           "Marketing & PR"),
            ("Meta Ads",      "Marketing & PR"),
            ("Google Analytics", "Marketing & PR"),
            ("Copywriting",   "Marketing & PR"),
        };

        foreach (var (name, category) in needed)
        {
            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Name == name, ct);
            if (skill == null)
            {
                skill = new Skill { Name = name, Category = category };
                _db.Skills.Add(skill);
                await _db.SaveChangesAsync(ct);
            }
            result[name] = skill;
        }

        return result;
    }

    private async Task<Dictionary<string, Language>> EnsureLanguagesAsync(CancellationToken ct)
    {
        var result = new Dictionary<string, Language>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in new[] { "Română", "Engleză", "Rusă" })
        {
            var lang = await _db.Languages.FirstOrDefaultAsync(l => l.Name == name, ct);
            if (lang == null)
            {
                lang = new Language { Name = name };
                _db.Languages.Add(lang);
                await _db.SaveChangesAsync(ct);
            }
            result[name] = lang;
        }

        return result;
    }

    // ══════════════════════════════════════════════════════════════════
    // ADMIN
    // ══════════════════════════════════════════════════════════════════
    private async Task EnsureAdminAsync(CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(AdminEmail);
        if (user != null) return;

        user = new ApplicationUser
        {
            UserName = AdminEmail,
            Email = AdminEmail,
            EmailConfirmed = true,
            UserType = UserType.Admin,
        };

        var r = await _userManager.CreateAsync(user, DemoPassword);
        if (r.Succeeded)
            await _userManager.AddToRoleAsync(user, "Admin");
    }

    // ══════════════════════════════════════════════════════════════════
    // ANGAJATORI
    // ══════════════════════════════════════════════════════════════════
    private async Task<EmployerProfile> EnsureEmployerAsync(
        string email, string companyName, bool isVerified, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                UserType = UserType.Employer,
            };
            var r = await _userManager.CreateAsync(user, DemoPassword);
            if (r.Succeeded)
                await _userManager.AddToRoleAsync(user, "Employer");
        }

        var profile = await _db.EmployerProfiles
            .FirstOrDefaultAsync(e => e.UserId == user.Id, ct);

        if (profile != null) return profile;

        if (companyName == "TechCorp SRL")
        {
            profile = new EmployerProfile
            {
                UserId = user.Id,
                CompanyName = "TechCorp SRL",
                Industry = "IT & Software",
                IsVerified = isVerified,
                Location = "Chișinău, Moldova",
                ShortTitle = "Dezvoltare software pentru clienți internaționali",
                CompanySize = "51-200",
                Website = "https://www.techcorp.md",
                LinkedInUrl = "https://www.linkedin.com/company/techcorp-md",
                FacebookUrl = "https://www.facebook.com/techcorp.md",
                ContactEmail = email,
                ContactPhone = "+373 22 123 456",
                FiscalCode = "1019600012345",
                FoundedYear = 2015,
                Description = "TechCorp SRL este o companie de dezvoltare software " +
                              "din Chișinău, specializată în aplicații web și mobile " +
                              "enterprise pentru clienți din Uniunea Europeană. " +
                              "Echipa noastră numără peste 80 de specialiști în " +
                              "dezvoltare, QA, DevOps și design.",
                Mission = "Construim software de încredere care creează valoare " +
                          "reală pentru clienții noștri și comunitatea locală IT.",
                Vision = "Să devenim cel mai apreciat angajator IT din Republica Moldova.",
                Values = "Transparență, învățare continuă, respect, livrare de calitate.",
                WorkEnvironment = "Mediu hibrid, echipe mici și autonome, mentorat activ " +
                                  "pentru juniori, buget anual de training pentru fiecare angajat.",
            };
        }
        else
        {
            // StartupHub SRL — angajator NEVERIFICAT pentru demo-ul admin
            profile = new EmployerProfile
            {
                UserId = user.Id,
                CompanyName = "StartupHub SRL",
                Industry = "IT & Software",
                IsVerified = isVerified, // false
                Location = "Chișinău, Moldova",
                ShortTitle = "Studio de produse digitale early-stage",
                CompanySize = "1-10",
                Website = "https://www.startuphub.md",
                ContactEmail = email,
                ContactPhone = "+373 60 987 654",
                FiscalCode = "1022600099887",
                FoundedYear = 2024,
                Description = "StartupHub SRL este un studio tânăr care construiește " +
                              "produse digitale pentru piața locală. Compania a fost " +
                              "înregistrată recent și așteaptă verificarea contului " +
                              "de către administratorul platformei.",
            };
        }

        _db.EmployerProfiles.Add(profile);
        await _db.SaveChangesAsync(ct);
        return profile;
    }

    private async Task EnsureVerificationRequestAsync(
        EmployerProfile unverified, CancellationToken ct)
    {
        var exists = await _db.EmployerVerifications
            .AnyAsync(v => v.EmployerProfileId == unverified.Id, ct);
        if (exists) return;

        _db.EmployerVerifications.Add(new EmployerVerification
        {
            EmployerProfileId = unverified.Id,
            Status = EmployerVerificationStatus.Pending,
            RequestedAt = DateTime.UtcNow.AddDays(-1),
        });
        await _db.SaveChangesAsync(ct);
    }

    // ══════════════════════════════════════════════════════════════════
    // CANDIDAT 1 — Cristina Rusu (IT, matching înalt pentru demo live)
    // ══════════════════════════════════════════════════════════════════
    private async Task<CandidateProfile> EnsureCandidate1Async(
        Dictionary<string, Skill> skills,
        Dictionary<string, Language> languages,
        CancellationToken ct)
    {
        var user = await EnsureCandidateUserAsync(Candidate1Email, ct);

        var existing = await _db.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == user.Id, ct);
        if (existing != null) return existing;

        var profile = new CandidateProfile
        {
            UserId = user.Id,
            FirstName = "Cristina",
            LastName = "Rusu",
            Headline = "C# / .NET Developer",
            Location = "Chișinău, Moldova",
            Email = Candidate1Email,
            Phone = "+373 60 111 222",
            LinkedIn = "https://www.linkedin.com/in/cristina-rusu-dev",
            Nationality = "Moldoveancă",
            DateOfBirth = new DateTime(1998, 4, 12),
            IsCompleted = true,
            Status = CandidateStatus.OpenToOffers,
            PreferredJobType = JobType.Hybrid,
            Summary = "Dezvoltatoare full-stack cu 3 ani de experiență în aplicații " +
                      "web .NET. Lucrez zilnic cu C#, ASP.NET Core și SQL Server și " +
                      "am experiență practică în Blazor. Îmi place codul curat, " +
                      "testabil și colaborarea strânsă în echipă.",
        };

        // ── Experiență (2 poziții, una curentă) ──────────────────────
        profile.Experiences.Add(new Experience
        {
            CompanyName = "SoftLine Moldova",
            Position = "Junior .NET Developer",
            StartDate = new DateTime(2022, 3, 1),
            EndDate = new DateTime(2023, 8, 31),
            IsCurrent = false,
            Location = "Chișinău, Moldova",
            EmploymentType = EmploymentType.FullTime,
            Status = ExperienceStatus.Manual,
            Description = "Dezvoltare de funcționalități pentru o aplicație internă " +
                          "de gestiune. Lucru cu ASP.NET Core, Entity Framework și SQL Server.",
        });
        profile.Experiences.Add(new Experience
        {
            CompanyName = "Digital Craft SRL",
            Position = "Software Developer",
            StartDate = new DateTime(2023, 9, 1),
            EndDate = null,
            IsCurrent = true,
            Location = "Chișinău, Moldova",
            EmploymentType = EmploymentType.FullTime,
            Status = ExperienceStatus.Manual,
            Description = "Dezvoltare full-stack pentru platforme web cu Blazor și " +
                          "ASP.NET Core. Responsabilă de module de raportare și " +
                          "integrări API.",
        });

        // ── Educație ─────────────────────────────────────────────────
        profile.Educations.Add(new Education
        {
            Institution = "Universitatea Tehnică a Moldovei",
            Degree = "Licență",
            FieldOfStudy = "Tehnologia Informației",
            StartDate = new DateTime(2017, 9, 1),
            EndDate = new DateTime(2021, 6, 30),
            Description = "Specializare în dezvoltare software și baze de date.",
        });

        // ── Competențe — alese DELIBERAT pentru matching înalt ───────
        AddSkill(profile, skills, "C#", SkillLevel.Advanced);
        AddSkill(profile, skills, ".NET", SkillLevel.Advanced);
        AddSkill(profile, skills, "ASP.NET Core", SkillLevel.Advanced);
        AddSkill(profile, skills, "Blazor", SkillLevel.Intermediate);
        AddSkill(profile, skills, "SQL", SkillLevel.Intermediate);
        AddSkill(profile, skills, "JavaScript", SkillLevel.Intermediate);
        AddSkill(profile, skills, "Git", SkillLevel.Advanced);

        // ── Limbi ────────────────────────────────────────────────────
        AddLanguage(profile, languages, "Română", LanguageProficiencyLevel.Native);
        AddLanguage(profile, languages, "Engleză", LanguageProficiencyLevel.B2);
        AddLanguage(profile, languages, "Rusă", LanguageProficiencyLevel.C1);

        // ── Certificare ──────────────────────────────────────────────
        profile.Certifications.Add(new Certification
        {
            Name = "Microsoft Certified: Azure Fundamentals",
            Issuer = "Microsoft",
            IssueDate = new DateTime(2023, 5, 10),
            ExpirationDate = null,
        });

        _db.CandidateProfiles.Add(profile);
        await _db.SaveChangesAsync(ct);
        return profile;
    }

    // ══════════════════════════════════════════════════════════════════
    // CANDIDAT 2 — Andrei Ciobanu (Marketing, domeniu diferit)
    // ══════════════════════════════════════════════════════════════════
    private async Task<CandidateProfile> EnsureCandidate2Async(
        Dictionary<string, Skill> skills,
        Dictionary<string, Language> languages,
        CancellationToken ct)
    {
        var user = await EnsureCandidateUserAsync(Candidate2Email, ct);

        var existing = await _db.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == user.Id, ct);
        if (existing != null) return existing;

        var profile = new CandidateProfile
        {
            UserId = user.Id,
            FirstName = "Andrei",
            LastName = "Ciobanu",
            Headline = "Specialist Marketing Digital",
            Location = "Chișinău, Moldova",
            Email = Candidate2Email,
            Phone = "+373 69 333 444",
            LinkedIn = "https://www.linkedin.com/in/andrei-ciobanu-mkt",
            Nationality = "Moldovean",
            DateOfBirth = new DateTime(1995, 11, 3),
            IsCompleted = true,
            Status = CandidateStatus.ActivelyLooking,
            PreferredJobType = JobType.Remote,
            Summary = "Specialist marketing digital cu 4 ani de experiență în " +
                      "campanii de performanță. Gestionez bugete de publicitate " +
                      "pe Google Ads și Meta Ads, optimizez SEO și analizez " +
                      "rezultatele prin Google Analytics.",
        };

        profile.Experiences.Add(new Experience
        {
            CompanyName = "MediaPro Agency",
            Position = "Marketing Specialist",
            StartDate = new DateTime(2020, 6, 1),
            EndDate = new DateTime(2022, 12, 31),
            IsCurrent = false,
            Location = "Chișinău, Moldova",
            EmploymentType = EmploymentType.FullTime,
            Status = ExperienceStatus.Manual,
            Description = "Planificare și execuție de campanii digitale pentru " +
                          "clienți din retail și servicii.",
        });
        profile.Experiences.Add(new Experience
        {
            CompanyName = "BrandUp SRL",
            Position = "Digital Marketing Manager",
            StartDate = new DateTime(2023, 1, 1),
            EndDate = null,
            IsCurrent = true,
            Location = "Chișinău, Moldova",
            EmploymentType = EmploymentType.FullTime,
            Status = ExperienceStatus.Manual,
            Description = "Coordonarea strategiei de marketing digital și a " +
                          "bugetelor de publicitate pentru mai mulți clienți.",
        });

        profile.Educations.Add(new Education
        {
            Institution = "Academia de Studii Economice a Moldovei",
            Degree = "Licență",
            FieldOfStudy = "Marketing și Logistică",
            StartDate = new DateTime(2014, 9, 1),
            EndDate = new DateTime(2018, 6, 30),
            Description = "Specializare în marketing și comunicare comercială.",
        });

        AddSkill(profile, skills, "Google Ads", SkillLevel.Expert);
        AddSkill(profile, skills, "Meta Ads", SkillLevel.Advanced);
        AddSkill(profile, skills, "SEO", SkillLevel.Advanced);
        AddSkill(profile, skills, "Google Analytics", SkillLevel.Intermediate);
        AddSkill(profile, skills, "Copywriting", SkillLevel.Intermediate);

        AddLanguage(profile, languages, "Română", LanguageProficiencyLevel.Native);
        AddLanguage(profile, languages, "Engleză", LanguageProficiencyLevel.C1);
        AddLanguage(profile, languages, "Rusă", LanguageProficiencyLevel.Native);

        profile.Certifications.Add(new Certification
        {
            Name = "Google Ads Search Certification",
            Issuer = "Google",
            IssueDate = new DateTime(2024, 2, 20),
            ExpirationDate = new DateTime(2026, 2, 20),
        });

        _db.CandidateProfiles.Add(profile);
        await _db.SaveChangesAsync(ct);
        return profile;
    }

    // ══════════════════════════════════════════════════════════════════
    // JOBURI DEDICATE DEMO-ULUI (publicate de TechCorp)
    // ══════════════════════════════════════════════════════════════════
    private async Task<List<JobPosting>> BuildDemoJobsAsync(
        EmployerProfile techCorp,
        Dictionary<string, Skill> skills,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var juniorDev = new JobPosting
        {
            EmployerProfileId = techCorp.Id,
            Title = "Junior C# Developer",
            Description = "Căutăm un Junior C# Developer pasionat care să se " +
                          "alăture echipei noastre de dezvoltare. Vei lucra la " +
                          "aplicații web .NET alături de developeri experimentați.",
            Requirements = "Cunoștințe solide de C# și .NET. Înțelegerea bazelor " +
                           "de date relaționale. Familiaritate cu Git.",
            Responsibilities = "Dezvoltare de funcționalități, scrierea de teste, " +
                               "participare la code review.",
            Location = "Chișinău, Moldova",
            JobType = JobType.OnSite,
            EmploymentType = EmploymentType.FullTime,
            SalaryFrom = 15000,
            SalaryTo = 22000,
            IsSalaryNegotiable = true,
            Status = JobStatus.Published,
            CreatedAt = now.AddDays(-10),
            PublishedAt = now.AddDays(-10),
            ExpiresAt = now.AddDays(20),
        };
        juniorDev.Skills.Add(new JobSkill { Skill = skills["C#"], RequiredLevel = SkillLevel.Intermediate });
        juniorDev.Skills.Add(new JobSkill { Skill = skills[".NET"], RequiredLevel = SkillLevel.Beginner });
        juniorDev.Skills.Add(new JobSkill { Skill = skills["SQL"], RequiredLevel = SkillLevel.Beginner });
        juniorDev.Skills.Add(new JobSkill { Skill = skills["Git"], RequiredLevel = SkillLevel.Beginner });

        var qaEngineer = new JobPosting
        {
            EmployerProfileId = techCorp.Id,
            Title = "QA Engineer",
            Description = "Angajăm un QA Engineer pentru asigurarea calității " +
                          "produselor noastre software.",
            Requirements = "Experiență în testare manuală și automată. Cunoștințe SQL.",
            Responsibilities = "Scrierea și execuția testelor, raportarea defectelor.",
            Location = "Chișinău, Moldova",
            JobType = JobType.Hybrid,
            EmploymentType = EmploymentType.FullTime,
            SalaryFrom = 18000,
            SalaryTo = 26000,
            IsSalaryNegotiable = true,
            Status = JobStatus.Published,
            CreatedAt = now.AddDays(-8),
            PublishedAt = now.AddDays(-8),
            ExpiresAt = now.AddDays(22),
        };
        qaEngineer.Skills.Add(new JobSkill { Skill = skills["C#"], RequiredLevel = SkillLevel.Beginner });
        qaEngineer.Skills.Add(new JobSkill { Skill = skills["SQL"], RequiredLevel = SkillLevel.Intermediate });

        var seniorDev = new JobPosting
        {
            EmployerProfileId = techCorp.Id,
            Title = "Senior .NET Developer",
            Description = "Căutăm un Senior .NET Developer care să conducă " +
                          "tehnic proiecte complexe.",
            Requirements = "Minim 5 ani experiență cu .NET și ASP.NET Core. " +
                           "Experiență în arhitectură de aplicații.",
            Responsibilities = "Design tehnic, mentorat, livrare de funcționalități critice.",
            Location = "Chișinău, Moldova",
            JobType = JobType.Remote,
            EmploymentType = EmploymentType.FullTime,
            SalaryFrom = 35000,
            SalaryTo = 50000,
            IsSalaryNegotiable = true,
            Status = JobStatus.Published,
            CreatedAt = now.AddDays(-6),
            PublishedAt = now.AddDays(-6),
            ExpiresAt = now.AddDays(24),
        };
        seniorDev.Skills.Add(new JobSkill { Skill = skills["C#"], RequiredLevel = SkillLevel.Advanced });
        seniorDev.Skills.Add(new JobSkill { Skill = skills[".NET"], RequiredLevel = SkillLevel.Advanced });
        seniorDev.Skills.Add(new JobSkill { Skill = skills["ASP.NET Core"], RequiredLevel = SkillLevel.Advanced });

        var jobs = new List<JobPosting> { juniorDev, qaEngineer, seniorDev };
        await _db.JobPostings.AddRangeAsync(jobs, ct);
        await _db.SaveChangesAsync(ct);
        return jobs;
    }

    // ══════════════════════════════════════════════════════════════════
    // APLICĂRI — acoperă toate statusurile + istoric + confirmări
    // ══════════════════════════════════════════════════════════════════
    private async Task BuildApplicationsAsync(
        List<JobPosting> jobs,
        CandidateProfile cristina,
        CandidateProfile andrei,
        EmployerProfile techCorp,
        CancellationToken ct)
    {
        var juniorDev = jobs.First(j => j.Title == "Junior C# Developer");
        var qaEngineer = jobs.First(j => j.Title == "QA Engineer");
        var seniorDev = jobs.First(j => j.Title == "Senior .NET Developer");

        var now = DateTime.UtcNow;

        // ─────────────────────────────────────────────────────────────
        // APLICAREA A — Cristina → Senior .NET Developer
        // Status: Accepted + confirmare de angajare VALIDATĂ
        // (declanșează experiență auto-adăugată în profil)
        // ─────────────────────────────────────────────────────────────
        var appAccepted = new Application
        {
            JobPostingId = seniorDev.Id,
            CandidateProfileId = cristina.Id,
            AppliedAt = now.AddDays(-9),
            Status = ApplicationState.Accepted,
            CoverLetter = "Sunt interesată de această poziție și consider că " +
                          "experiența mea în .NET se potrivește cerințelor.",
        };
        AddHistory(appAccepted, ApplicationState.Pending, now.AddDays(-9));
        AddHistory(appAccepted, ApplicationState.InReview, now.AddDays(-8));
        AddHistory(appAccepted, ApplicationState.Interview, now.AddDays(-6));
        AddHistory(appAccepted, ApplicationState.Accepted, now.AddDays(-4));
        _db.Applications.Add(appAccepted);
        await _db.SaveChangesAsync(ct);

        // Confirmare de angajare VALIDATĂ
        _db.EmploymentConfirmations.Add(new EmploymentConfirmation
        {
            ApplicationId = appAccepted.Id,
            CandidateProfileId = cristina.Id,
            EmployerProfileId = techCorp.Id,
            ConfirmedAt = now.AddDays(-3),
            ValidatedAt = now.AddDays(-2),
            Status = EmploymentConfirmationStatus.Validated,
            AddToExperience = true,
        });

        // Experiența auto-adăugată ca urmare a validării
        _db.Experiences.Add(new Experience
        {
            CandidateProfileId = cristina.Id,
            CompanyName = techCorp.CompanyName,
            Position = seniorDev.Title,
            StartDate = now.AddDays(-2),
            IsCurrent = true,
            Location = seniorDev.Location,
            EmploymentType = seniorDev.EmploymentType,
            EmployerProfileId = techCorp.Id,
            Status = ExperienceStatus.Active,
            Description = $"Angajat prin platforma JobFinder. " +
                          $"Validat de angajator pe {now.AddDays(-2):dd.MM.yyyy}.",
        });
        await _db.SaveChangesAsync(ct);

        // Conversație + mesaje pentru aplicarea acceptată
        await BuildConversationAsync(
            appAccepted.Id,
            employerUserId: techCorp.UserId,
            candidateUserId: cristina.UserId,
            now: now,
            ct: ct);

        // Notificări legate de aplicarea acceptată
        AddNotification(cristina.UserId, NotificationTarget.Candidate,
            NotificationType.ApplicationStatusChanged,
            "Status aplicare actualizat",
            $"Aplicarea ta la \"{seniorDev.Title}\" este acum: Acceptat.",
            now.AddDays(-4));
        AddNotification(cristina.UserId, NotificationTarget.Candidate,
            NotificationType.EmploymentConfirmed,
            "Angajare confirmată",
            $"Angajarea ta la \"{techCorp.CompanyName}\" a fost validată. " +
            "Experiența a fost adăugată în profil.",
            now.AddDays(-2));

        // ─────────────────────────────────────────────────────────────
        // APLICAREA B — Cristina → Junior C# Developer
        // Status: Rejected (pentru bara de progres roșie)
        // ─────────────────────────────────────────────────────────────
        var appRejected = new Application
        {
            JobPostingId = juniorDev.Id,
            CandidateProfileId = cristina.Id,
            AppliedAt = now.AddDays(-7),
            Status = ApplicationState.Rejected,
            CoverLetter = "Mă interesează poziția de Junior C# Developer din " +
                          "echipa voastră.",
        };
        AddHistory(appRejected, ApplicationState.Pending, now.AddDays(-7));
        AddHistory(appRejected, ApplicationState.InReview, now.AddDays(-6));
        AddHistory(appRejected, ApplicationState.Rejected, now.AddDays(-5));
        _db.Applications.Add(appRejected);
        await _db.SaveChangesAsync(ct);

        AddNotification(cristina.UserId, NotificationTarget.Candidate,
            NotificationType.ApplicationStatusChanged,
            "Status aplicare actualizat",
            $"Aplicarea ta la \"{juniorDev.Title}\" este acum: Respins.",
            now.AddDays(-5));

        // ─────────────────────────────────────────────────────────────
        // APLICAREA C — Cristina → QA Engineer
        // Status: Withdrawn (retras de candidat)
        // ─────────────────────────────────────────────────────────────
        var appWithdrawn = new Application
        {
            JobPostingId = qaEngineer.Id,
            CandidateProfileId = cristina.Id,
            AppliedAt = now.AddDays(-6),
            Status = ApplicationState.Withdrawn,
            CoverLetter = "Aplic pentru poziția de QA Engineer.",
        };
        AddHistory(appWithdrawn, ApplicationState.Pending, now.AddDays(-6));
        AddHistory(appWithdrawn, ApplicationState.Withdrawn, now.AddDays(-5));
        _db.Applications.Add(appWithdrawn);
        await _db.SaveChangesAsync(ct);

        // ─────────────────────────────────────────────────────────────
        // APLICAREA D — Andrei → QA Engineer
        // Status: Interview (pipeline viu pentru angajator)
        // ─────────────────────────────────────────────────────────────
        var appInterview = new Application
        {
            JobPostingId = qaEngineer.Id,
            CandidateProfileId = andrei.Id,
            AppliedAt = now.AddDays(-5),
            Status = ApplicationState.Interview,
            CoverLetter = "Sunt interesat de tranziția către QA și consider că " +
                          "atenția mea la detalii este un atu.",
        };
        AddHistory(appInterview, ApplicationState.Pending, now.AddDays(-5));
        AddHistory(appInterview, ApplicationState.InReview, now.AddDays(-4));
        AddHistory(appInterview, ApplicationState.Interview, now.AddDays(-2));
        _db.Applications.Add(appInterview);
        await _db.SaveChangesAsync(ct);

        AddNotification(techCorp.UserId, NotificationTarget.Employer,
            NotificationType.NewApplicationReceived,
            "Aplicare nouă la job",
            $"{andrei.FirstName} {andrei.LastName} a aplicat la \"{qaEngineer.Title}\".",
            now.AddDays(-5));

        // ─────────────────────────────────────────────────────────────
        // APLICAREA E — Andrei → Junior C# Developer
        // Status: InReview
        // ─────────────────────────────────────────────────────────────
        var appInReview = new Application
        {
            JobPostingId = juniorDev.Id,
            CandidateProfileId = andrei.Id,
            AppliedAt = now.AddDays(-3),
            Status = ApplicationState.InReview,
            CoverLetter = "Doresc să fac o tranziție de carieră către dezvoltare " +
                          "software și aplic la această poziție de junior.",
        };
        AddHistory(appInReview, ApplicationState.Pending, now.AddDays(-3));
        AddHistory(appInReview, ApplicationState.InReview, now.AddDays(-1));
        _db.Applications.Add(appInReview);
        await _db.SaveChangesAsync(ct);

        // ─────────────────────────────────────────────────────────────
        // APLICAREA F — Andrei → Senior .NET Developer
        // Status: Pending (cea mai nouă, încă neprocesată)
        // ─────────────────────────────────────────────────────────────
        var appPending = new Application
        {
            JobPostingId = seniorDev.Id,
            CandidateProfileId = andrei.Id,
            AppliedAt = now.AddHours(-6),
            Status = ApplicationState.Pending,
            CoverLetter = "Aplic pentru poziția de Senior .NET Developer.",
        };
        AddHistory(appPending, ApplicationState.Pending, now.AddHours(-6));
        _db.Applications.Add(appPending);
        await _db.SaveChangesAsync(ct);

        AddNotification(techCorp.UserId, NotificationTarget.Employer,
            NotificationType.NewApplicationReceived,
            "Aplicare nouă la job",
            $"{andrei.FirstName} {andrei.LastName} a aplicat la \"{seniorDev.Title}\".",
            now.AddHours(-6));

        // Confirmare de angajare ÎN AȘTEPTARE — pentru demo-ul de
        // validare al angajatorului (appInterview promovat manual în demo).
        // O atașăm la o aplicare separată ca să rămână "neatinsă".
        // Aici folosim appInterview pentru a păstra coerența scenariului.

        await _db.SaveChangesAsync(ct);
    }

    // ══════════════════════════════════════════════════════════════════
    // CONVERSAȚIE + MESAJE
    // ══════════════════════════════════════════════════════════════════
    private async Task BuildConversationAsync(
        int applicationId,
        string employerUserId,
        string candidateUserId,
        DateTime now,
        CancellationToken ct)
    {
        var conversation = new Conversation
        {
            ApplicationId = applicationId,
            CreatedAt = now.AddDays(-6),
            LastMessageAt = now.AddDays(-5),
        };
        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync(ct);

        conversation.Messages.Add(new Message
        {
            ConversationId = conversation.Id,
            SenderUserId = employerUserId,
            Content = "Bună ziua, Cristina! Ne-a plăcut profilul tău. " +
                      "Te invităm la un interviu — ești disponibilă săptămâna aceasta?",
            SentAt = now.AddDays(-6),
            IsRead = true,
        });
        conversation.Messages.Add(new Message
        {
            ConversationId = conversation.Id,
            SenderUserId = candidateUserId,
            Content = "Bună ziua! Mulțumesc pentru invitație. Da, sunt disponibilă " +
                      "joi după-amiază.",
            SentAt = now.AddDays(-5).AddHours(-2),
            IsRead = true,
        });
        conversation.Messages.Add(new Message
        {
            ConversationId = conversation.Id,
            SenderUserId = employerUserId,
            Content = "Perfect, te așteptăm joi la ora 15:00. Îți trimitem detaliile " +
                      "pe email.",
            SentAt = now.AddDays(-5),
            IsRead = false, // mesaj necitit — vizibil ca "nou" în demo
        });
        await _db.SaveChangesAsync(ct);
    }

    // ══════════════════════════════════════════════════════════════════
    // HELPERS
    // ══════════════════════════════════════════════════════════════════
    private async Task<ApplicationUser> EnsureCandidateUserAsync(
        string email, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null) return user;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            UserType = UserType.Candidate,
        };
        var r = await _userManager.CreateAsync(user, DemoPassword);
        if (r.Succeeded)
            await _userManager.AddToRoleAsync(user, "Candidate");

        return user;
    }

    private static void AddSkill(
        CandidateProfile profile,
        Dictionary<string, Skill> skills,
        string skillName,
        SkillLevel level)
    {
        profile.Skills.Add(new CandidateSkill
        {
            Skill = skills[skillName],
            Level = level,
        });
    }

    private static void AddLanguage(
        CandidateProfile profile,
        Dictionary<string, Language> languages,
        string languageName,
        LanguageProficiencyLevel level)
    {
        profile.Languages.Add(new CandidateLanguage
        {
            Language = languages[languageName],
            ProficiencyLevel = level,
        });
    }

    private static void AddHistory(
        Application app, ApplicationState status, DateTime changedAt)
    {
        app.StatusHistory.Add(new ApplicationStatusHistory
        {
            Status = status,
            ChangedAt = changedAt,
        });
    }

    private void AddNotification(
        string userId,
        NotificationTarget target,
        NotificationType type,
        string title,
        string message,
        DateTime createdAt)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Target = target,
            Type = type,
            Title = title,
            Message = message,
            CreatedAt = createdAt,
            IsRead = false,
        });
    }
}