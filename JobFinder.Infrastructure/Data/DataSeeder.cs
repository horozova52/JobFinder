using JobFinder.Core.Entities.Common;
using JobFinder.Core.Entities.Employers;
using JobFinder.Core.Entities.Identity;
using JobFinder.Core.Entities.Jobs;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Data;

public class DataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJobFeedRepository _feedRepo;

    public DataSeeder(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IJobFeedRepository feedRepo)
    {
        _db = db;
        _userManager = userManager;
        _feedRepo = feedRepo;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Rulează seed-ul dacă avem mai puțin de 10 joburi publicate
        var publishedCount = await _db.JobPostings
            .CountAsync(j => j.Status == JobStatus.Published, ct);

        if (publishedCount >= 10)
            return;

        var employers = await EnsureEmployersAsync(ct);
        var skills = await EnsureSkillsAsync(ct);
        var jobs = BuildJobs(employers, skills);

        await _db.JobPostings.AddRangeAsync(jobs, ct);
        await _db.SaveChangesAsync(ct);
    }

    // ── Angajatori ────────────────────────────────────────────────────

    private async Task<Dictionary<string, EmployerProfile>> EnsureEmployersAsync(
        CancellationToken ct)
    {
        var result = new Dictionary<string, EmployerProfile>();

        var seedData = new[]
        {
            new { Key = "techcorp",   Email = "hr@techcorp.md",
                  Company = "TechCorp SRL",      Industry = "IT & Software",
                  Short = "Soluții software enterprise pentru piața europeană" },

            new { Key = "dataflow",   Email = "hr@dataflow.md",
                  Company = "DataFlow Systems",  Industry = "Data & AI",
                  Short = "Platforme de analiză a datelor și soluții AI" },

            new { Key = "designlab",  Email = "hr@designlab.md",
                  Company = "DesignLab Agency",  Industry = "Media & Design",
                  Short = "Agenție de design UX/UI cu peste 100 proiecte livrate" },

            new { Key = "cloudscale", Email = "hr@cloudscale.md",
                  Company = "CloudScale SRL",    Industry = "DevOps & Cloud",
                  Short = "Infrastructură cloud și DevOps pentru startup-uri" },

            new { Key = "fintech",    Email = "hr@fintechmd.md",
                  Company = "FinTech Moldova",   Industry = "Finanțe & Contabilitate",
                  Short = "Soluții financiare digitale pentru sectorul bancar" },
        };

        foreach (var sd in seedData)
        {
            var user = await _userManager.FindByEmailAsync(sd.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = sd.Email,
                    Email = sd.Email,
                    EmailConfirmed = true,
                    UserType = UserType.Employer,
                };
                var r = await _userManager.CreateAsync(user, "Seed@12345!");
                if (r.Succeeded)
                    await _userManager.AddToRoleAsync(user, "Employer");
            }

            var profile = await _db.EmployerProfiles
                .FirstOrDefaultAsync(e => e.UserId == user.Id, ct);

            if (profile == null)
            {
                profile = new EmployerProfile
                {
                    UserId = user.Id,
                    CompanyName = sd.Company,
                    Industry = sd.Industry,
                    IsVerified = true,
                    Location = "Chișinău, Moldova",
                    ShortTitle = sd.Short,
                    CompanySize = "51-200",
                    Website = $"https://www.{sd.Key}.md",
                    ContactEmail = sd.Email,
                    Description = $"Companie activă în {sd.Industry}, cu o echipă de profesioniști dedicați.",
                };
                _db.EmployerProfiles.Add(profile);
                await _db.SaveChangesAsync(ct);
            }

            result[sd.Key] = profile;
        }

        return result;
    }

    // ── Skills ────────────────────────────────────────────────────────

    private async Task<Dictionary<string, Skill>> EnsureSkillsAsync(CancellationToken ct)
    {
        var allSkills = new (string Name, string Category)[]
        {
            // IT & Software
            ("C#",              "IT & Software"), (".NET",         "IT & Software"),
            ("ASP.NET Core",    "IT & Software"), ("Entity Framework","IT & Software"),
            ("SQL Server",      "IT & Software"), ("PostgreSQL",   "IT & Software"),
            ("REST API",        "IT & Software"), ("Microservices","IT & Software"),
            // Web Development
            ("React",           "Web Development"), ("Vue.js",     "Web Development"),
            ("Angular",         "Web Development"), ("TypeScript",  "Web Development"),
            ("JavaScript",      "Web Development"), ("HTML",        "Web Development"),
            ("CSS",             "Web Development"), ("Tailwind CSS","Web Development"),
            ("Next.js",         "Web Development"),
            // DevOps & Cloud
            ("Docker",          "DevOps & Cloud"), ("Kubernetes",  "DevOps & Cloud"),
            ("CI/CD",           "DevOps & Cloud"), ("Git",         "DevOps & Cloud"),
            ("Linux",           "DevOps & Cloud"), ("AWS",         "DevOps & Cloud"),
            ("Azure",           "DevOps & Cloud"), ("Terraform",   "DevOps & Cloud"),
            ("GitHub Actions",  "DevOps & Cloud"), ("Jenkins",     "DevOps & Cloud"),
            // Data & AI
            ("Python",          "Data & AI"), ("Machine Learning","Data & AI"),
            ("SQL",             "Data & AI"), ("Power BI",       "Data & AI"),
            ("Tableau",         "Data & AI"), ("Pandas",         "Data & AI"),
            ("NumPy",           "Data & AI"), ("TensorFlow",     "Data & AI"),
            ("Data Analysis",   "Data & AI"), ("Excel",          "Data & AI"),
            // Media & Design
            ("Figma",           "Media & Design"), ("UX Design",  "Media & Design"),
            ("UI Design",       "Media & Design"), ("Prototyping","Media & Design"),
            ("Wireframing",     "Media & Design"), ("Adobe Photoshop","Media & Design"),
            ("Adobe Illustrator","Media & Design"),
            // Finanțe
            ("Contabilitate",         "Finanțe & Contabilitate"),
            ("SAP",                   "Finanțe & Contabilitate"),
            ("Excel avansat",         "Finanțe & Contabilitate"),
            ("Analiză financiară",    "Finanțe & Contabilitate"),
            ("Raportare financiară",  "Finanțe & Contabilitate"),
            // Management
            ("Agile",                  "Management & Business"),
            ("Scrum",                  "Management & Business"),
            ("Jira",                   "Management & Business"),
            ("Leadership",             "Management & Business"),
            ("Comunicare",             "Management & Business"),
            ("Managementul proiectelor","Management & Business"),
            ("Microsoft Office",       "Management & Business"),
        };

        var result = new Dictionary<string, Skill>(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, category) in allSkills)
        {
            var skill = await _db.Skills
                .FirstOrDefaultAsync(s => s.Name == name, ct);

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

    // ── Joburi ────────────────────────────────────────────────────────

    private static List<JobPosting> BuildJobs(
        Dictionary<string, EmployerProfile> emp,
        Dictionary<string, Skill> sk)
    {
        var now = DateTime.UtcNow;
        var jobs = new List<JobPosting>();
        var rng = Random.Shared;

        // Shorthand local
        JobPosting J(
            string key, string title, string desc, string req,
            string? loc, JobType jt, EmploymentType et,
            decimal? from, decimal? to, bool neg,
            params string[] skillKeys)
        => new()
        {
            EmployerProfileId = emp[key].Id,
            Title = title,
            Description = desc,
            Requirements = req,
            Location = loc,
            JobType = jt,
            EmploymentType = et,
            SalaryFrom = from,
            SalaryTo = to,
            IsSalaryNegotiable = neg,
            Status = JobStatus.Published,
            CreatedAt = now.AddDays(-rng.Next(2, 30)),
            PublishedAt = now.AddDays(-rng.Next(1, 20)),
            Skills = skillKeys
                .Where(sk.ContainsKey)
                .Select(k => new JobSkill
                {
                    Skill = sk[k],
                    RequiredLevel = SkillLevel.Intermediate,
                })
                .ToList(),
        };

        // ── TechCorp — IT & Software ──────────────────────────────────
        jobs.Add(J("techcorp",
            "Senior .NET Developer",
            "Cauți un proiect enterprise cu arhitectură curată? Lucrezi la o platformă SaaS cu mii de utilizatori activi. Echipă de 8 devs, code review serios, fără crunch.",
            "Minim 4 ani .NET • ASP.NET Core • SQL Server • Docker • Code review activ",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 2500, 4000, false,
            "C#", ".NET", "ASP.NET Core", "SQL Server", "Docker"));

        jobs.Add(J("techcorp",
            "Junior C# Developer",
            "Prima ta experiență în .NET profesional. Ești mentorat de seniori și lucrezi la features reale din prima săptămână. Creștem împreună.",
            "Cunoștințe C# și OOP • Git de bază • Dorință de a crește • SQL constituie avantaj",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 800, 1500, false,
            "C#", ".NET", "SQL", "Git"));

        jobs.Add(J("techcorp",
            "Backend Developer (ASP.NET Core)",
            "API-uri RESTful pentru aplicații mobile și web. Arhitectură CQRS, teste unitare, CI/CD automat. Lucru serios, fără tehno-datorii.",
            "3+ ani ASP.NET Core • Entity Framework • REST API • PostgreSQL",
            "Remote", JobType.Remote, EmploymentType.FullTime, 2000, 3500, false,
            "ASP.NET Core", "Entity Framework", "REST API", "PostgreSQL", "C#"));

        jobs.Add(J("techcorp",
            "Fullstack Developer (.NET + React)",
            "De la bază de date la interfața utilizatorului — tu controlezi tot. Proiecte diverse, sprint-uri bine planificate, retrospective eficiente.",
            "3+ ani .NET • 2+ ani React • TypeScript • SQL Server • REST API",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 2200, 3800, false,
            "C#", ".NET", "React", "TypeScript", "SQL Server"));

        jobs.Add(J("techcorp",
            "QA Engineer",
            "Scrii teste automate și manuale pentru aplicații .NET și React. Previi bug-urile înainte să ajungă la client.",
            "Experiență testare software • Automatizare (Selenium sau Playwright) • SQL • Atenție la detalii",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 1200, 2200, true,
            "C#", ".NET", "Git", "SQL"));

        jobs.Add(J("techcorp",
            "Arhitect Software",
            "Definești arhitectura sistemelor distribuite pentru clienți enterprise. Decizia ta contează și se vede în producție.",
            "7+ ani software development • Microservicii • Docker & Kubernetes • Bune abilități de comunicare",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 4500, 7000, false,
            "C#", ".NET", "Microservices", "Docker", "Kubernetes", "Azure"));

        // ── DataFlow — Data & AI ──────────────────────────────────────
        jobs.Add(J("dataflow",
            "Data Analyst",
            "Transformi date brute în decizii de business. Dashboard-uri Power BI, rapoarte executive, analize ad-hoc. Lucrezi direct cu CEO și COO.",
            "2+ ani analiză date • SQL avansat • Power BI sau Tableau • Comunicare clară cu non-tehnici",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 1400, 2500, false,
            "SQL", "Power BI", "Excel avansat", "Data Analysis", "Tableau"));

        jobs.Add(J("dataflow",
            "Machine Learning Engineer",
            "Modele ML care rulează în producție, nu în notebook-uri. Pipeline-uri automate, monitoring, A/B testing. Python și cloud sunt home-ul tău.",
            "3+ ani Python • TensorFlow sau PyTorch • SQL • Statistică • AWS sau Azure",
            "Remote", JobType.Remote, EmploymentType.FullTime, 3000, 5000, false,
            "Python", "Machine Learning", "TensorFlow", "SQL", "AWS"));

        jobs.Add(J("dataflow",
            "Business Intelligence Developer",
            "Construiești soluții BI scalabile. Lucrezi cu stakeholders pentru a înțelege nevoile reale de raportare și le transformi în dashboard-uri clare.",
            "3+ ani BI • Power BI • SQL Server • Data warehousing • Abilitatea de a explica tehnic non-tehnicilor",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 1800, 3000, false,
            "Power BI", "SQL", "SQL Server", "Data Analysis", "Excel"));

        jobs.Add(J("dataflow",
            "Data Engineer",
            "Pipeline-uri de date robuste pentru volume mari. Lucrezi cu sisteme distribuite și faci datele disponibile echipelor de analiză.",
            "3+ ani Python sau Scala • Apache Spark • SQL • Experiență cloud • Git",
            "Remote", JobType.Remote, EmploymentType.FullTime, 2500, 4500, false,
            "Python", "SQL", "AWS", "Docker", "Git"));

        jobs.Add(J("dataflow",
            "Data Science Intern",
            "Stagiu remunerat, proiecte reale, mentorat de la zi 1. Dacă ești student sau proaspăt absolvent pasionat de date, ești exact ce căutăm.",
            "Python de bază • Statistică • Dorință de a învăța • ML constituie avantaj",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.Internship, 500, 900, false,
            "Python", "Pandas", "NumPy", "SQL", "Excel"));

        // ── DesignLab — Media & Design ────────────────────────────────
        jobs.Add(J("designlab",
            "UX/UI Designer Senior",
            "De la research utilizatori la prototipuri high-fidelity. Lucrezi la produse digitale pentru clienți din România, Moldova și Europa de Vest.",
            "3+ ani UX/UI • Figma avansat • Research utilizatori • Portofoliu solid",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 1500, 2800, false,
            "Figma", "UX Design", "UI Design", "Prototyping", "Wireframing"));

        jobs.Add(J("designlab",
            "Graphic Designer",
            "Materiale vizuale pentru branduri și campanii digitale. Creativitate, ochi pentru detalii și pasiune pentru tipografie sunt esențiale.",
            "2+ ani design grafic • Adobe Photoshop • Illustrator • Portofoliu relevant",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 1000, 2000, false,
            "Adobe Photoshop", "Adobe Illustrator", "Figma", "UI Design"));

        jobs.Add(J("designlab",
            "Frontend Developer (Design Systems)",
            "Implementezi design systems pixel-perfect și construiești componente reutilizabile. Lucrezi strâns cu designerii.",
            "2+ ani React • TypeScript • CSS avansat • Figma • Ochi pentru detalii",
            "Remote", JobType.Remote, EmploymentType.FullTime, 1800, 3200, false,
            "React", "TypeScript", "CSS", "Tailwind CSS", "Figma"));

        jobs.Add(J("designlab",
            "Junior UX Designer",
            "Primul tău rol în UX. Participi la research, wireframes și teste cu utilizatori sub îndrumarea seniorilor.",
            "Cunoștințe Figma • Interes pentru UX • Portofoliu (chiar și proiecte de școală) • Curiozitate",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 700, 1300, false,
            "Figma", "Wireframing", "Prototyping", "UI Design"));

        // ── CloudScale — DevOps & Cloud ───────────────────────────────
        jobs.Add(J("cloudscale",
            "DevOps Engineer",
            "Infrastructura cloud și pipeline-urile CI/CD pentru echipele de produs. Uptime 99.9%, automatizare maximă, zero procese manuale.",
            "3+ ani DevOps • Docker & Kubernetes • CI/CD • Linux avansat • AWS sau Azure",
            "Remote", JobType.Remote, EmploymentType.FullTime, 2500, 4000, false,
            "Docker", "Kubernetes", "CI/CD", "Linux", "AWS"));

        jobs.Add(J("cloudscale",
            "Cloud Infrastructure Engineer",
            "Proiectezi arhitecturi cloud scalabile pentru startup-uri și scale-up-uri europene. Terraform, Kubernetes, securitate cloud.",
            "4+ ani cloud • AWS sau Azure certificat • Terraform • Kubernetes • Securitate cloud",
            "Remote", JobType.Remote, EmploymentType.FullTime, 3500, 5500, false,
            "AWS", "Azure", "Terraform", "Kubernetes", "Docker"));

        jobs.Add(J("cloudscale",
            "Site Reliability Engineer (SRE)",
            "Sistemele funcționează. Monitorizare, alerting, incident response, post-mortems. Automatizezi orice poate fi automatizat.",
            "3+ ani SRE/DevOps • Python sau Go • Kubernetes • Prometheus/Grafana • Linux",
            "Remote", JobType.Remote, EmploymentType.FullTime, 3000, 5000, false,
            "Python", "Docker", "Kubernetes", "Linux", "CI/CD", "GitHub Actions"));

        jobs.Add(J("cloudscale",
            "Junior DevOps Engineer",
            "Primul pas în DevOps profesional. Mentorat, proiecte reale, creștere rapidă. Dacă știi Linux și Git de bază, ești gata.",
            "Linux de bază • Git • Docker (basic) • Bash sau Python • Dorință de a crește",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 900, 1600, false,
            "Linux", "Git", "Docker", "Python", "CI/CD"));

        jobs.Add(J("cloudscale",
            "Kubernetes Administrator",
            "Administrezi clustere Kubernetes de producție. Zero-downtime deployments, auto-scaling, disaster recovery.",
            "3+ ani Kubernetes • Helm • Service mesh (Istio) • Monitorizare • Linux avansat",
            "Remote", JobType.Remote, EmploymentType.FullTime, 3000, 5000, false,
            "Kubernetes", "Docker", "Linux", "AWS", "Azure"));

        // ── FinTech Moldova — Finanțe & Business ──────────────────────
        jobs.Add(J("fintech",
            "Contabil Senior",
            "Contabilitate completă, raportare lunară și anuală conform standardelor naționale și IFRS. Lucrezi cu un portofoliu de clienți corporate.",
            "5+ ani experiență • Legislație fiscală Moldova • SAP sau 1C • Excel avansat",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 1500, 2500, false,
            "Contabilitate", "SAP", "Excel avansat", "Raportare financiară"));

        jobs.Add(J("fintech",
            "Analist Financiar",
            "Modele de prognoză, analize de performanță, prezentări executive. Datele tale influențează deciziile de investiție ale companiei.",
            "3+ ani analiză financiară • Excel avansat • Power BI • Modelare financiară • Comunicare executivă",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 1800, 3000, false,
            "Analiză financiară", "Excel avansat", "Power BI", "SQL", "Raportare financiară"));

        jobs.Add(J("fintech",
            "Project Manager IT",
            "Coordonezi proiectele de digitalizare internă. Lucrezi cu echipe tehnice și stakeholders non-tehnici, livrezi în timp și în buget.",
            "3+ ani PM • Agile/Scrum • Jira • Comunicare excelentă • Experiență FinTech sau IT",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 2000, 3500, false,
            "Managementul proiectelor", "Agile", "Scrum", "Jira", "Comunicare"));

        jobs.Add(J("fintech",
            "Scrum Master",
            "Facilitezi ceremoniile Agile, elimini impedimentele, promovezi cultura de îmbunătățire continuă. Echipa ta livrează constant.",
            "2+ ani Scrum Master • PSM sau CSM preferată • Facilitare • Experiență IT",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 1800, 3000, false,
            "Agile", "Scrum", "Jira", "Leadership", "Comunicare"));

        jobs.Add(J("fintech",
            "Risk Manager",
            "Identifici, evaluezi și mitigezi riscurile operaționale și financiare. Lucrezi direct cu board-ul pentru strategia de risc.",
            "4+ ani risk management sector financiar • Reglementări BNM • Analiză cantitativă",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 2200, 3500, false,
            "Analiză financiară", "Excel avansat", "Raportare financiară", "SQL"));

        return jobs;
    }
}