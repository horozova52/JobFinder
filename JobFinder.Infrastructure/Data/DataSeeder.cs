using JobFinder.Core.Entities.Candidates;
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
        var publishedCount = await _db.JobPostings
            .CountAsync(j => j.Status == JobStatus.Published, ct);

        if (publishedCount >= 150)
            return;

        var categories = await EnsureCategoriesAsync(ct);
        var employers = await EnsureEmployersAsync(ct);
        var skills = await EnsureSkillsAsync(ct);
        await EnsureCandidatesAsync(ct);

        var jobs = BuildJobs(employers, skills, categories);
        await _db.JobPostings.AddRangeAsync(jobs, ct);
        await _db.SaveChangesAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // CATEGORII
    // ═══════════════════════════════════════════════════════════════

    private async Task<Dictionary<string, JobCategory>> EnsureCategoriesAsync(CancellationToken ct)
    {
        var result = new Dictionary<string, JobCategory>(StringComparer.OrdinalIgnoreCase);

        var allCats = new (string Name, string Icon)[]
        {
            ("Administrativ / Oficiu",      "bi bi-briefcase"),
            ("Construcții & Imobiliare",    "bi bi-building"),
            ("Data & AI",                   "bi bi-cpu"),
            ("DevOps & Cloud",              "bi bi-cloud"),
            ("Educație",                    "bi bi-mortarboard"),
            ("Finanțe & Contabilitate",     "bi bi-cash-stack"),
            ("HR & Recrutare",              "bi bi-people"),
            ("Inginerie & Tehnic",          "bi bi-tools"),
            ("IT & Software",              "bi bi-code-slash"),
            ("Juridic",                     "bi bi-scales"),
            ("Management & Business",       "bi bi-graph-up"),
            ("Marketing & PR",              "bi bi-megaphone"),
            ("Media & Design",              "bi bi-palette"),
            ("Sănătate",                    "bi bi-heart-pulse"),
            ("Transport & Logistică",       "bi bi-truck"),
            ("Vânzări & Retail",            "bi bi-bag"),
        };

        foreach (var (name, icon) in allCats)
        {
            var cat = await _db.JobCategories
                .FirstOrDefaultAsync(c => c.Name == name, ct);

            if (cat == null)
            {
                cat = new JobCategory { Name = name, Icon = icon };
                _db.JobCategories.Add(cat);
                await _db.SaveChangesAsync(ct);
            }

            result[name] = cat;
        }

        return result;
    }

    // ═══════════════════════════════════════════════════════════════
    // ANGAJATORI (20 companii moldovenești)
    // ═══════════════════════════════════════════════════════════════

    private async Task<Dictionary<string, EmployerProfile>> EnsureEmployersAsync(CancellationToken ct)
    {
        var result = new Dictionary<string, EmployerProfile>();

        var seedData = new[]
        {
            // IT & Software
            new { Key = "techcorp",      Email = "hr@techcorp.md",
                  Company = "TechCorp SRL",           Industry = "IT & Software",
                  Size = "51-200",   City = "Chișinău",
                  Short = "Soluții software enterprise pentru piața europeană",
                  Desc  = "TechCorp SRL este o companie moldovenească de dezvoltare software fondată în 2012, specializată în soluții enterprise pentru clienți din UE. Cu o echipă de peste 80 de ingineri, livrăm produse .NET, Java și cloud-native." },

            new { Key = "softmind",      Email = "hr@softmind.md",
                  Company = "SoftMind Solutions",     Industry = "IT & Software",
                  Size = "11-50",    City = "Bălți",
                  Short = "Aplicații mobile și web pentru startup-uri din Europa",
                  Desc  = "SoftMind Solutions activează din 2018 în Bălți, oferind servicii de dezvoltare mobilă React Native și Flutter, precum și aplicații web cu Angular și Node.js pentru clienți din Germania, Franța și UK." },

            // Data & AI
            new { Key = "dataflow",      Email = "hr@dataflow.md",
                  Company = "DataFlow Systems",       Industry = "Data & AI",
                  Size = "11-50",    City = "Chișinău",
                  Short = "Platforme de analiză a datelor și soluții AI",
                  Desc  = "DataFlow Systems este pionierul analitic din Moldova, cu proiecte de machine learning și Business Intelligence implementate în sectorul bancar și retail din țară și regiune." },

            new { Key = "intellidata",   Email = "hr@intellidata.md",
                  Company = "IntelliData MD",         Industry = "Data & AI",
                  Size = "1-10",     City = "Chișinău",
                  Short = "Consultanță în date și automatizare procese cu AI",
                  Desc  = "IntelliData MD oferă servicii de consultanță în transformarea digitală bazată pe date, cu accent pe automatizare RPA și modele predictive pentru IMM-uri moldovenești." },

            // DevOps & Cloud
            new { Key = "cloudscale",    Email = "hr@cloudscale.md",
                  Company = "CloudScale SRL",         Industry = "DevOps & Cloud",
                  Size = "11-50",    City = "Chișinău",
                  Short = "Infrastructură cloud și DevOps pentru startup-uri",
                  Desc  = "CloudScale SRL proiectează și gestionează infrastructuri cloud (AWS, Azure, GCP) pentru companii din Moldova și Europa de Est. Suntem parteneri certificați AWS și Microsoft." },

            // Media & Design
            new { Key = "designlab",     Email = "hr@designlab.md",
                  Company = "DesignLab Agency",       Industry = "Media & Design",
                  Size = "11-50",    City = "Chișinău",
                  Short = "Agenție de design UX/UI cu peste 100 proiecte livrate",
                  Desc  = "DesignLab Agency este cea mai premiată agenție de design din Moldova, cu portofoliu de branding, UX/UI și motion design pentru clienți locali și internaționali." },

            new { Key = "pixelmedia",    Email = "hr@pixelmedia.md",
                  Company = "PixelMedia SRL",         Industry = "Media & Design",
                  Size = "1-10",     City = "Cahul",
                  Short = "Studio creativ: foto, video, grafică și animație",
                  Desc  = "PixelMedia SRL este un studio creativ din Cahul care produce conținut vizual de calitate: fotografii de produs, reclame video, animații 2D și identitate vizuală." },

            // Finanțe & Contabilitate
            new { Key = "fintech",       Email = "hr@fintechmd.md",
                  Company = "FinTech Moldova",        Industry = "Finanțe & Contabilitate",
                  Size = "51-200",   City = "Chișinău",
                  Short = "Soluții financiare digitale pentru sectorul bancar",
                  Desc  = "FinTech Moldova dezvoltă platforma de plăți digitale utilizată de 3 bănci din Republica Moldova, oferind soluții de open banking, procesare tranzacții și compliance AML/KYC." },

            new { Key = "auditpro",      Email = "hr@auditpro.md",
                  Company = "AuditPro Consulting",    Industry = "Finanțe & Contabilitate",
                  Size = "11-50",    City = "Chișinău",
                  Short = "Consultanță fiscală, audit și contabilitate pentru IMM",
                  Desc  = "AuditPro Consulting oferă servicii complete de contabilitate, audit financiar și consultanță fiscală pentru over 200 de companii din Moldova, cu experți certificați ACCA și CIPA." },

            // HR & Recrutare
            new { Key = "hrprime",       Email = "hr@hrprime.md",
                  Company = "HR Prime Moldova",       Industry = "HR & Recrutare",
                  Size = "11-50",    City = "Chișinău",
                  Short = "Agenție de recrutare și outsourcing HR pentru companii",
                  Desc  = "HR Prime Moldova este lider pe piața de recrutare din țară, plasând anual peste 500 de candidați în companii din IT, Retail și Producție. Oferim și servicii de payroll și formare profesională." },

            // Management & Business
            new { Key = "bizmoldova",    Email = "hr@bizmoldova.md",
                  Company = "BizMoldova Consulting",  Industry = "Management & Business",
                  Size = "11-50",    City = "Chișinău",
                  Short = "Consultanță în management strategic și transformare organizațională",
                  Desc  = "BizMoldova Consulting ajută companiile moldovenești să crească prin strategie de business, optimizare procese și management al schimbării. Partenerim cu EBRD și OIM în proiecte de dezvoltare." },

            // Marketing & PR
            new { Key = "markethub",     Email = "hr@markethub.md",
                  Company = "MarketHub Agency",       Industry = "Marketing & PR",
                  Size = "11-50",    City = "Chișinău",
                  Short = "Agenție de marketing digital și comunicare integrată",
                  Desc  = "MarketHub Agency gestionează campaniile de marketing digital pentru branduri de top din Moldova: SEO, SEM, Social Media, Email Marketing și PR. Portofoliu de 50+ clienți activi." },

            new { Key = "prmotion",      Email = "hr@prmotion.md",
                  Company = "PRmotion SRL",           Industry = "Marketing & PR",
                  Size = "1-10",     City = "Bălți",
                  Short = "Relații publice și comunicare corporativă în nordul țării",
                  Desc  = "PRmotion SRL oferă servicii de comunicare corporativă, relații cu presa și organizare evenimente pentru companii din nordul Moldovei și zona Bălți-Soroca." },

            // Educație
            new { Key = "edupro",        Email = "hr@edupro.md",
                  Company = "EduPro Center",          Industry = "Educație",
                  Size = "11-50",    City = "Chișinău",
                  Short = "Centru de formare profesională și cursuri corporate",
                  Desc  = "EduPro Center este un centru acreditat de formare profesională din Chișinău, cu programe de IT, limbi străine, management și soft skills pentru adulți și companii." },

            // Sănătate
            new { Key = "medclinic",     Email = "hr@medclinic.md",
                  Company = "MedClinic Moldova",      Industry = "Sănătate",
                  Size = "201-500",  City = "Chișinău",
                  Short = "Rețea de clinici private cu servicii medicale complete",
                  Desc  = "MedClinic Moldova operează 5 clinici private în Chișinău și Bălți, oferind servicii medicale de înaltă calitate: consultații, diagnostic imagistic, laborator și chirurgie ambulatorie." },

            // Transport & Logistică
            new { Key = "logisticmd",    Email = "hr@logisticmd.md",
                  Company = "LogisticMD SRL",         Industry = "Transport & Logistică",
                  Size = "51-200",   City = "Chișinău",
                  Short = "Transport internațional și logistică pentru import-export",
                  Desc  = "LogisticMD SRL este unul dintre liderii pieței de transport și logistică din Moldova, cu o flotă de 80 de vehicule TIR și servicii de depozitare și distribuție pe întreg teritoriul țării." },

            new { Key = "moldtrans",     Email = "hr@moldtrans.md",
                  Company = "MoldTrans Group",        Industry = "Transport & Logistică",
                  Size = "51-200",   City = "Ungheni",
                  Short = "Grup de transport rutier cu acoperire europeană",
                  Desc  = "MoldTrans Group operează din Ungheni, punct strategic la granița cu România, asigurând transport rutier regulat pe rute Moldova-UE cu 120+ camioane proprii." },

            // Vânzări & Retail
            new { Key = "retailmd",      Email = "hr@retailmd.md",
                  Company = "RetailMD SA",            Industry = "Vânzări & Retail",
                  Size = "201-500",  City = "Chișinău",
                  Short = "Lanț de magazine alimentare și non-alimentare în Moldova",
                  Desc  = "RetailMD SA operează 25 de magazine în toată Moldova sub brandul GreenMart, cu focus pe produse locale și alimentație sănătoasă. Suntem în expansiune continuă și angajăm activ." },

            // Construcții & Imobiliare
            new { Key = "constructmd",   Email = "hr@constructmd.md",
                  Company = "ConstructMD SRL",        Industry = "Construcții & Imobiliare",
                  Size = "51-200",   City = "Chișinău",
                  Short = "Construcții rezidențiale și comerciale de înaltă calitate",
                  Desc  = "ConstructMD SRL realizează proiecte de construcție rezidențială și comercială în Chișinău și împrejurimi. Am livrat peste 1200 de apartamente și 15 proiecte comerciale în ultimii 10 ani." },

            // Juridic
            new { Key = "legalmd",       Email = "hr@legalmd.md",
                  Company = "LegalMD Asociați",       Industry = "Juridic",
                  Size = "11-50",    City = "Chișinău",
                  Short = "Cabinet de avocatură specializat în drept corporativ și comercial",
                  Desc  = "LegalMD Asociați oferă servicii juridice complete pentru companii: drept corporativ, contracte comerciale, litigii, proprietate intelectuală și conformitate GDPR." },
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
                    Location = $"{sd.City}, Moldova",
                    ShortTitle = sd.Short,
                    CompanySize = sd.Size,
                    Website = $"https://www.{sd.Key}.md",
                    ContactEmail = sd.Email,
                    Description = sd.Desc,
                };
                _db.EmployerProfiles.Add(profile);
                await _db.SaveChangesAsync(ct);
            }

            result[sd.Key] = profile;
        }

        return result;
    }

    // ═══════════════════════════════════════════════════════════════
    // CANDIDAȚI (12 profiluri complete)
    // ═══════════════════════════════════════════════════════════════

    private async Task EnsureCandidatesAsync(CancellationToken ct)
    {
        var candidates = new[]
        {
            new { Email = "ion.moraru@mail.md",    Pass = "Seed@12345!", First = "Ion",      Last = "Moraru",
                  Title = "Senior .NET Developer",      City = "Chișinău", Phone = "+37360100001",
                  Summary = "Dezvoltator .NET cu 6 ani experiență în aplicații enterprise. Expert în C#, ASP.NET Core și arhitecturi microservicii." },

            new { Email = "ana.cojocaru@mail.md",  Pass = "Seed@12345!", First = "Ana",      Last = "Cojocaru",
                  Title = "UX/UI Designer",             City = "Chișinău", Phone = "+37360100002",
                  Summary = "Designer cu pasiune pentru experiențe digitale intuitive. Figma, Adobe XD și cercetare utilizatori sunt instrumentele mele principale." },

            new { Email = "dmitri.rusu@mail.md",   Pass = "Seed@12345!", First = "Dmitri",   Last = "Rusu",
                  Title = "Data Analyst",               City = "Bălți",    Phone = "+37360100003",
                  Summary = "Analist de date specializat în Python, SQL și Power BI. Experiență în sectorul bancar și retail din Moldova." },

            new { Email = "maria.lungu@mail.md",   Pass = "Seed@12345!", First = "Maria",    Last = "Lungu",
                  Title = "Contabil Senior",            City = "Chișinău", Phone = "+37360100004",
                  Summary = "Contabil cu 8 ani experiență, certificare CIPA, expert în 1C și raportare financiară conform standardelor moldovenești și IFRS." },

            new { Email = "vadim.popescu@mail.md", Pass = "Seed@12345!", First = "Vadim",    Last = "Popescu",
                  Title = "DevOps Engineer",            City = "Chișinău", Phone = "+37360100005",
                  Summary = "Inginer DevOps cu experiență în Docker, Kubernetes și CI/CD pe AWS. Pasionat de automatizare și infrastructură ca cod." },

            new { Email = "elena.micu@mail.md",    Pass = "Seed@12345!", First = "Elena",    Last = "Micu",
                  Title = "HR Manager",                 City = "Chișinău", Phone = "+37360100006",
                  Summary = "Manager HR cu 5 ani experiență în recrutare IT și procese de onboarding. Certificare în People Management și coaching." },

            new { Email = "andrei.stefan@mail.md", Pass = "Seed@12345!", First = "Andrei",   Last = "Ștefan",
                  Title = "Marketing Manager Digital",  City = "Chișinău", Phone = "+37360100007",
                  Summary = "Specialist marketing digital cu experiență în Google Ads, Meta Ads și SEO. Am gestionat bugete de campanie de până la 500.000 MDL/lună." },

            new { Email = "natalia.gros@mail.md",  Pass = "Seed@12345!", First = "Natalia",  Last = "Grosu",
                  Title = "Avocat corporativ",          City = "Chișinău", Phone = "+37360100008",
                  Summary = "Avocat cu specializare în drept comercial și corporativ. Experiență în tranzacții M&A, contracte și conformitate GDPR pentru companii internaționale." },

            new { Email = "victor.carp@mail.md",   Pass = "Seed@12345!", First = "Victor",   Last = "Carp",
                  Title = "Inginer Constructor",        City = "Chișinău", Phone = "+37360100009",
                  Summary = "Inginer constructor cu 10 ani experiență în proiecte rezidențiale și comerciale. Expert AutoCAD, Revit și managementul șantierului." },

            new { Email = "olga.botan@mail.md",    Pass = "Seed@12345!", First = "Olga",     Last = "Botan",
                  Title = "Asistent Administrativ",     City = "Soroca",   Phone = "+37360100010",
                  Summary = "Asistent administrativ organizată și orientată spre detalii, cu experiență în secretariat, gestiune documente și coordonare activități de birou." },

            new { Email = "sergiu.turcan@mail.md", Pass = "Seed@12345!", First = "Sergiu",   Last = "Turcan",
                  Title = "Logistician Senior",         City = "Ungheni",  Phone = "+37360100011",
                  Summary = "Logistician cu 7 ani experiență în transport internațional și gestionare stocuri. Cunosc perfect piața TIR Moldova-UE." },

            new { Email = "cristina.palade@mail.md", Pass = "Seed@12345!", First = "Cristina", Last = "Palade",
                  Title = "Manager Vânzări",            City = "Chișinău", Phone = "+37360100012",
                  Summary = "Manager vânzări cu track record dovedit în retail și B2B. Am crescut echipe de la 5 la 20 de oameni și am depășit constant targetele trimestriale." },
        };

        foreach (var c in candidates)
        {
            var user = await _userManager.FindByEmailAsync(c.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = c.Email,
                    Email = c.Email,
                    EmailConfirmed = true,
                    UserType = UserType.Candidate,
                };
                var r = await _userManager.CreateAsync(user, c.Pass);
                if (r.Succeeded)
                    await _userManager.AddToRoleAsync(user, "Candidate");
            }

            var profile = await _db.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id, ct);

            if (profile == null)
            {
                profile = new CandidateProfile
                {
                    UserId = user.Id,
                    FirstName = c.First,
                    LastName = c.Last,
                    Headline = c.Title,
                    Location = $"{c.City}, Moldova",
                    Phone = c.Phone,
                    Email = c.Email,
                    Summary = c.Summary,
                    IsCompleted = true,
                    Status = CandidateStatus.ActivelyLooking,
                    Nationality = "Moldovean",
                    DateOfBirth = new DateTime(1990, 1, 1).AddDays(Random.Shared.Next(0, 3650)),
                };
                _db.CandidateProfiles.Add(profile);
                await _db.SaveChangesAsync(ct);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // SKILLS
    // ═══════════════════════════════════════════════════════════════

    private async Task<Dictionary<string, Skill>> EnsureSkillsAsync(CancellationToken ct)
    {
        var allSkills = new (string Name, string Category)[]
        {
            // IT & Software
            ("C#",               "IT & Software"), (".NET",              "IT & Software"),
            ("ASP.NET Core",     "IT & Software"), ("Entity Framework",  "IT & Software"),
            ("SQL Server",       "IT & Software"), ("PostgreSQL",        "IT & Software"),
            ("REST API",         "IT & Software"), ("Microservices",     "IT & Software"),
            ("Blazor",           "IT & Software"), ("SignalR",           "IT & Software"),
            // Web Development
            ("React",            "IT & Software"), ("Vue.js",            "IT & Software"),
            ("Angular",          "IT & Software"), ("TypeScript",        "IT & Software"),
            ("JavaScript",       "IT & Software"), ("HTML",              "IT & Software"),
            ("CSS",              "IT & Software"), ("Next.js",           "IT & Software"),
            ("Node.js",          "IT & Software"), ("Flutter",           "IT & Software"),
            ("React Native",     "IT & Software"), ("Kotlin",            "IT & Software"),
            // DevOps & Cloud
            ("Docker",           "DevOps & Cloud"), ("Kubernetes",       "DevOps & Cloud"),
            ("CI/CD",            "DevOps & Cloud"), ("Git",              "DevOps & Cloud"),
            ("Linux",            "DevOps & Cloud"), ("AWS",              "DevOps & Cloud"),
            ("Azure",            "DevOps & Cloud"), ("Terraform",        "DevOps & Cloud"),
            ("GitHub Actions",   "DevOps & Cloud"), ("Jenkins",          "DevOps & Cloud"),
            ("GCP",              "DevOps & Cloud"), ("Ansible",          "DevOps & Cloud"),
            // Data & AI
            ("Python",           "Data & AI"), ("Machine Learning",      "Data & AI"),
            ("SQL",              "Data & AI"), ("Power BI",              "Data & AI"),
            ("Tableau",          "Data & AI"), ("Pandas",               "Data & AI"),
            ("NumPy",            "Data & AI"), ("TensorFlow",            "Data & AI"),
            ("Data Analysis",    "Data & AI"), ("Excel",                "Data & AI"),
            ("Spark",            "Data & AI"), ("Hadoop",               "Data & AI"),
            ("R",                "Data & AI"), ("PyTorch",              "Data & AI"),
            // Media & Design
            ("Figma",            "Media & Design"), ("UX Design",        "Media & Design"),
            ("UI Design",        "Media & Design"), ("Prototyping",      "Media & Design"),
            ("Wireframing",      "Media & Design"), ("Adobe Photoshop",  "Media & Design"),
            ("Adobe Illustrator","Media & Design"), ("Adobe Premiere",   "Media & Design"),
            ("After Effects",    "Media & Design"), ("Canva",            "Media & Design"),
            // Finanțe
            ("Contabilitate",         "Finanțe & Contabilitate"),
            ("SAP",                   "Finanțe & Contabilitate"),
            ("Excel avansat",         "Finanțe & Contabilitate"),
            ("Analiză financiară",    "Finanțe & Contabilitate"),
            ("Raportare financiară",  "Finanțe & Contabilitate"),
            ("1C Contabilitate",      "Finanțe & Contabilitate"),
            ("IFRS",                  "Finanțe & Contabilitate"),
            ("Audit",                 "Finanțe & Contabilitate"),
            // Management & Business
            ("Agile",                   "Management & Business"),
            ("Scrum",                   "Management & Business"),
            ("Jira",                    "Management & Business"),
            ("Leadership",              "Management & Business"),
            ("Comunicare",              "Management & Business"),
            ("Managementul proiectelor","Management & Business"),
            ("Microsoft Office",        "Management & Business"),
            ("Power Point",             "Management & Business"),
            // Marketing & PR
            ("SEO",              "Marketing & PR"), ("Google Ads",       "Marketing & PR"),
            ("Meta Ads",         "Marketing & PR"), ("Email Marketing",  "Marketing & PR"),
            ("Content Writing",  "Marketing & PR"), ("Social Media",     "Marketing & PR"),
            ("Google Analytics", "Marketing & PR"), ("Copywriting",      "Marketing & PR"),
            // HR
            ("Recrutare",        "HR & Recrutare"), ("Interviuri",       "HR & Recrutare"),
            ("Onboarding",       "HR & Recrutare"), ("Payroll",          "HR & Recrutare"),
            ("HR Analytics",     "HR & Recrutare"), ("Employer Branding","HR & Recrutare"),
            // Juridic
            ("Drept comercial",  "Juridic"), ("Drept corporativ",        "Juridic"),
            ("Contracte",        "Juridic"), ("Litigii",                  "Juridic"),
            ("GDPR",             "Juridic"), ("Proprietate intelectuală", "Juridic"),
            // Construcții
            ("AutoCAD",          "Construcții & Imobiliare"), ("Revit",  "Construcții & Imobiliare"),
            ("Management șantier","Construcții & Imobiliare"), ("BIM",   "Construcții & Imobiliare"),
            ("Deviz",            "Construcții & Imobiliare"),
            // Sănătate
            ("Îngrijire pacienți","Sănătate"), ("Diagnostic",           "Sănătate"),
            ("Farmacologie",     "Sănătate"), ("EMR",                   "Sănătate"),
            // Transport
            ("Logistică",        "Transport & Logistică"), ("SAP WM",    "Transport & Logistică"),
            ("Gestionare flotă", "Transport & Logistică"), ("CMR",       "Transport & Logistică"),
            // Vânzări
            ("Negociere",        "Vânzări & Retail"), ("CRM",            "Vânzări & Retail"),
            ("Salesforce",       "Vânzări & Retail"), ("Prezentări",     "Vânzări & Retail"),
            ("Managementul clienților","Vânzări & Retail"),
            // Educație
            ("Predare",          "Educație"), ("Curriculum Design",       "Educație"),
            ("E-learning",       "Educație"), ("Evaluare",               "Educație"),
            // Inginerie
            ("AutoCAD Electrical","Inginerie & Tehnic"), ("PLC",         "Inginerie & Tehnic"),
            ("SCADA",            "Inginerie & Tehnic"), ("Mentenanță",   "Inginerie & Tehnic"),
            ("Desenare tehnică", "Inginerie & Tehnic"),
            // Administrativ
            ("Secretariat",      "Administrativ / Oficiu"), ("Gestiune documente","Administrativ / Oficiu"),
            ("Recepție",         "Administrativ / Oficiu"), ("Coordonare","Administrativ / Oficiu"),
        };

        var result = new Dictionary<string, Skill>(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, category) in allSkills)
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

    // ═══════════════════════════════════════════════════════════════
    // JOBURI (~180 oferte distribuite pe toate 16 categorii)
    // ═══════════════════════════════════════════════════════════════

    private static List<JobPosting> BuildJobs(
        Dictionary<string, EmployerProfile> emp,
        Dictionary<string, Skill> sk,
        Dictionary<string, JobCategory> cats)
    {
        var now = DateTime.UtcNow;
        var jobs = new List<JobPosting>();
        var rng = Random.Shared;

        JobPosting J(
            string empKey, string catKey, string title,
            string desc, string req, string resp,
            string? loc, JobType jt, EmploymentType et,
            decimal? from, decimal? to, bool neg,
            string[] skillNames)
        {
            var jp = new JobPosting
            {
                EmployerProfile = emp[empKey],
                Category = cats[catKey],
                Title = title,
                Description = desc,
                Requirements = req,
                Responsibilities = resp,
                Location = loc,
                JobType = jt,
                EmploymentType = et,
                SalaryFrom = from,
                SalaryTo = to,
                IsSalaryNegotiable = neg,
                Status = JobStatus.Published,
                CreatedAt = now.AddDays(-rng.Next(1, 60)),
                PublishedAt = now.AddDays(-rng.Next(0, 30)),
                ExpiresAt = now.AddDays(rng.Next(15, 90)),
                Skills = skillNames
                    .Where(n => sk.ContainsKey(n))
                    .Select(n => new JobSkill { Skill = sk[n] })
                    .ToList()
            };
            return jp;
        }

        // ────────────────────────────────────────────────────────
        // 1. IT & SOFTWARE (20 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("techcorp", "IT & Software", "Senior C# / .NET Developer",
            "Căutăm un Senior .NET Developer pentru echipa noastră de produs din Chișinău. Vei lucra la o platformă enterprise utilizată de companii din 5 țări europene.",
            "Minim 4 ani experiență cu C# și .NET 6+. Cunoaștere solidă de SQL Server, REST API și arhitecturi microservicii. Engleza la nivel B2.",
            "Proiectează și implementează funcționalități backend complexe. Participă la code review și mentorat juniori. Colaborează cu echipa de produs și clienți externi.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 30000, 50000, false,
            ["C#", ".NET", "ASP.NET Core", "SQL Server", "REST API", "Microservices"]));

        jobs.Add(J("techcorp", "IT & Software", "Junior C# Developer",
            "Oportunitate excelentă pentru absolvenți sau developeri cu 1-2 ani experiență. Vom investi în formarea ta profesională cu mentorat dedicat.",
            "1-2 ani experiență C#. Cunoaștere de baze de date relaționale. Dorință de a învăța și creștere profesională.",
            "Scrie cod C# curat și testabil sub supervizarea seniorilor. Participă activ la daily standups. Documentează funcționalitățile implementate.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 12000, 20000, false,
            ["C#", ".NET", "SQL Server", "Git"]));

        jobs.Add(J("techcorp", "IT & Software", "Fullstack Developer (.NET + React)",
            "Căutăm un developer fullstack care să lucreze atât pe backend C#/ASP.NET Core, cât și pe frontend React.",
            "3+ ani experiență fullstack. Cunoaștere solidă .NET și React/TypeScript. Experiență cu REST API și integrări externe.",
            "Dezvoltă și menține funcționalități end-to-end. Participă la design tehnic. Rezolvă bug-uri și optimizează performanța aplicației.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 22000, 38000, false,
            ["C#", ".NET", "React", "TypeScript", "REST API", "Git"]));

        jobs.Add(J("softmind", "IT & Software", "React Native Developer",
            "SoftMind caută un developer React Native pentru o aplicație mobilă cu sute de mii de utilizatori. Proiect interesant, echipă tânără.",
            "2+ ani experiență React Native. Cunoaștere JavaScript/TypeScript. Experiență cu publicare pe App Store și Google Play.",
            "Dezvoltă și menține aplicația mobilă. Implementează design-uri Figma în cod. Optimizează performanța și experiența utilizator.",
            "Bălți, Moldova", JobType.Remote, EmploymentType.FullTime, 20000, 35000, true,
            ["React Native", "JavaScript", "TypeScript", "React"]));

        jobs.Add(J("softmind", "IT & Software", "Flutter Developer",
            "Proiect nou: aplicație mobilă cross-platform pentru un client din Germania. Stack Flutter + Dart + Firebase.",
            "1+ ani experiență Flutter. Cunoaștere Dart. Experiență cu integrare REST API în aplicații mobile.",
            "Construiești aplicația Flutter de la zero. Colaborezi cu backend-ul ASP.NET Core. Participi la demo-uri cu clientul.",
            "Bălți, Moldova", JobType.Remote, EmploymentType.FullTime, 18000, 30000, false,
            ["Flutter", "JavaScript", "REST API"]));

        jobs.Add(J("softmind", "IT & Software", "Backend Node.js Developer",
            "Echipă de 8 oameni caută un backend developer Node.js pentru microservicii noi. Proiect SaaS B2B în creștere.",
            "2+ ani Node.js. Cunoaștere Express sau Fastify. Experiență cu PostgreSQL și Redis. Engleza B2+.",
            "Proiectezi și implementezi microservicii Node.js. Scrii teste automate. Participi la arhitectura sistemului.",
            "Bălți, Moldova", JobType.Hybrid, EmploymentType.FullTime, 18000, 32000, false,
            ["Node.js", "JavaScript", "PostgreSQL", "REST API", "Git"]));

        jobs.Add(J("techcorp", "IT & Software", "QA Engineer (Manual + Automation)",
            "Căutăm un QA Engineer care să asigure calitatea produselor noastre software înainte de lansare. Vei lucra cu o echipă de 30+ developeri.",
            "2+ ani QA. Cunoaștere Selenium sau Playwright. Experiență în testare API (Postman). Atenție la detalii.",
            "Creezi și execuți teste manuale și automate. Raportezi bug-uri și urmărești rezolvarea lor. Participi la sprint planning.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 15000, 25000, false,
            ["REST API", "Git", "SQL"]));

        jobs.Add(J("softmind", "IT & Software", "Frontend Angular Developer",
            "Proiect enterprise pentru o bancă din Moldova. Interfețe complexe de administrare și raportare în Angular 17.",
            "3+ ani Angular. Cunoaștere TypeScript, RxJS și Material Design. Experiență cu aplicații enterprise.",
            "Implementezi module noi în aplicația Angular. Optimizezi performanța și accesibilitatea. Colaborezi cu designeri și backend.",
            "Bălți, Moldova", JobType.OnSite, EmploymentType.FullTime, 20000, 35000, false,
            ["Angular", "TypeScript", "JavaScript", "CSS", "Git"]));

        jobs.Add(J("techcorp", "IT & Software", "Blazor WebAssembly Developer",
            "Proiect intern: portalul nostru de management intern se migrează la Blazor WASM. Căutăm un developer entuziast.",
            "Experiență C# și .NET. Cunoștințe HTML/CSS. Experiență Blazor sau dorință de a o dobândi rapid.",
            "Migrezi funcționalități din MVC la Blazor WASM. Scrii componente reutilizabile. Colaborezi cu echipa backend.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 18000, 30000, true,
            ["C#", ".NET", "Blazor", "HTML", "CSS"]));

        jobs.Add(J("softmind", "IT & Software", "Mobile Developer iOS (Swift)",
            "Aplicație de e-commerce pentru un retailer moldovenesc major. Versiunea iOS se lansează în Q3.",
            "2+ ani Swift/SwiftUI. Cunoaștere iOS SDK. Experiență cu publicare App Store.",
            "Dezvolți funcționalități noi în aplicația iOS. Colaborezi cu echipa Android pentru consistența UX. Participi la code review.",
            "Bălți, Moldova", JobType.Remote, EmploymentType.FullTime, 22000, 38000, false,
            ["REST API", "Git"]));

        jobs.Add(J("techcorp", "IT & Software", "Architect Software (.NET)",
            "Nivel senior: arhitect software responsabil de decizii tehnice majore pentru platforma noastră enterprise.",
            "7+ ani .NET. Experiență arhitecturi microservicii, DDD, CQRS. Capacitate de leadership tehnic.",
            "Definești arhitectura sistemului. Ghidezi echipa în decizii tehnice. Colaborezi cu CTO-ul. Faci research de noi tehnologii.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 50000, 80000, true,
            ["C#", ".NET", "Microservices", "SQL Server", "Azure", "Docker"]));

        jobs.Add(J("softmind", "IT & Software", "Intern Developer (C# sau JavaScript)",
            "Program de internship cu durata 3 luni, posibilitate de angajare la final. Ideal pentru studenți în ultimii ani.",
            "Student la informatică sau domeniu conex. Cunoaștere de bază C# sau JavaScript. Motivație și dorință de a învăța.",
            "Participi la proiecte reale sub supervizare. Înveți procesele Agile. Primești mentorat zilnic.",
            "Bălți, Moldova", JobType.OnSite, EmploymentType.Internship, 5000, 8000, false,
            ["C#", "JavaScript", "Git"]));

        jobs.Add(J("techcorp", "IT & Software", "Scrum Master / Agile Coach",
            "Căutăm un Scrum Master experimentat care să faciliteze echipele noastre de produs (3 echipe, ~25 persoane).",
            "Certificare CSM sau PSM. 3+ ani ca Scrum Master în echipe software. Bună înțelegere a proceselor Agile.",
            "Facilitezi ceremoniile Agile. Elimini impedimentele echipelor. Coaching pentru adoptarea practicilor Agile.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 25000, 40000, false,
            ["Agile", "Scrum", "Jira", "Comunicare"]));

        jobs.Add(J("softmind", "IT & Software", "Technical Support Engineer",
            "Suport tehnic de nivel 2 pentru clienții noștri enterprise. Rol care combină cunoștințe tehnice cu comunicare excelentă.",
            "Cunoaștere SQL, sisteme Linux, debugging aplicații web. Engleza C1. Răbdare și orientare spre client.",
            "Rezolvi incidente tehnice escalate de la nivelul 1. Documentezi soluțiile în knowledge base. Colaborezi cu developerii.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 14000, 22000, false,
            ["SQL", "Linux", "REST API"]));

        jobs.Add(J("techcorp", "IT & Software", "Product Manager (SaaS)",
            "Product Manager pentru unul din produsele noastre SaaS cu 200+ clienți activi din Europa.",
            "3+ ani PM în produse software. Cunoaștere metodologii Agile. Experiență cu analiza pieței și roadmap de produs.",
            "Definești viziunea și roadmap-ul produsului. Prioritizezi backlog-ul. Colaborezi cu echipele de dev, design și sales.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 30000, 50000, true,
            ["Agile", "Scrum", "Jira", "Comunicare"]));

        jobs.Add(J("softmind", "IT & Software", "Kotlin Android Developer",
            "Aplicație Android pentru un proiect fintech regional. Stack: Kotlin, Jetpack Compose, Retrofit.",
            "2+ ani Kotlin. Cunoaștere Jetpack Compose. Experiență cu integrare API și publicare Google Play.",
            "Dezvolți noi funcționalități Android. Scrii teste unitare. Colaborezi cu echipa backend și design.",
            "Bălți, Moldova", JobType.Remote, EmploymentType.FullTime, 20000, 36000, false,
            ["Kotlin", "REST API", "Git"]));

        jobs.Add(J("techcorp", "IT & Software", "Database Administrator (SQL Server)",
            "DBA pentru infrastructura de baze de date a platformei noastre. Responsabil de performanță, backup și securitate.",
            "4+ ani SQL Server. Cunoaștere tuning, index, backup/restore. Experiență cu AlwaysOn sau clustering.",
            "Administrezi bazele de date de producție. Monitorizezi performanța și optimizezi query-urile. Planifici capacitatea.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 22000, 38000, false,
            ["SQL Server", "SQL", "Azure"]));

        jobs.Add(J("softmind", "IT & Software", "Vue.js Frontend Developer",
            "Proiect: dashboard de analytics pentru un client din retail. Stack Vue 3 + Composition API + TailwindCSS.",
            "2+ ani Vue.js sau React. Cunoaștere TypeScript, Pinia/Vuex. Experiență cu design sisteme.",
            "Implementezi componente Vue.js performante. Integrezi API-uri REST. Colaborezi cu designerul UX.",
            "Bălți, Moldova", JobType.Remote, EmploymentType.FullTime, 18000, 30000, false,
            ["Vue.js", "JavaScript", "TypeScript", "CSS", "Git"]));

        jobs.Add(J("techcorp", "IT & Software", "Cybersecurity Analyst",
            "Analist de securitate cibernetică pentru protejarea infrastructurii și produselor noastre.",
            "3+ ani în securitate IT. Cunoaștere OWASP, pentesting, SIEM. Certificare CEH sau CISSP avantaj.",
            "Efectuezi audituri de securitate. Monitorizezi incidentele de securitate. Propui și implementezi măsuri de protecție.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 28000, 45000, false,
            ["Git", "Linux", "SQL"]));

        jobs.Add(J("softmind", "IT & Software", "Next.js Developer (SSR/SSG)",
            "Proiect: portal informațional cu trafic mare. Next.js 14 cu App Router, optimizare SEO și performanță.",
            "2+ ani Next.js sau React cu SSR. Cunoaștere Vercel, CDN, optimizare Core Web Vitals.",
            "Construiești pagini Next.js cu SSR/SSG. Optimizezi LCP, CLS și INP. Integrezi CMS headless.",
            "Bălți, Moldova", JobType.Remote, EmploymentType.FullTime, 18000, 32000, false,
            ["Next.js", "React", "TypeScript", "JavaScript", "CSS"]));

        // ────────────────────────────────────────────────────────
        // 2. DATA & AI (12 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("dataflow", "Data & AI", "Data Engineer (Python + Spark)",
            "Construiești pipeline-uri de date pentru platforma noastră de analytics. Procesăm zilnic milioane de tranzacții.",
            "3+ ani Python. Experiență Apache Spark sau Flink. Cunoaștere SQL avansat și ecosistem Hadoop.",
            "Proiectezi și implementezi pipeline-uri ETL. Optimizezi joburi Spark. Colaborezi cu echipa de Data Science.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 25000, 42000, false,
            ["Python", "Spark", "SQL", "Hadoop", "Git"]));

        jobs.Add(J("dataflow", "Data & AI", "Machine Learning Engineer",
            "Construiești și deploji modele ML în producție pentru sectorul bancar din Moldova și regiune.",
            "3+ ani ML. Cunoaștere scikit-learn, TensorFlow sau PyTorch. Experiență cu MLOps și modele în producție.",
            "Antrenezi și evaluezi modele predictive. Implementezi API-uri de inferență. Monitorizezi performanța modelelor.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 30000, 50000, true,
            ["Python", "Machine Learning", "TensorFlow", "PyTorch", "SQL"]));

        jobs.Add(J("intellidata", "Data & AI", "Data Analyst (Power BI)",
            "Analist de date pentru clienții noștri din retail și producție. Creezi dashboarduri și rapoarte care influențează decizii strategice.",
            "2+ ani Power BI sau Tableau. Cunoaștere SQL avansat. Experiență cu modelare date și DAX.",
            "Creezi rapoarte și dashboarduri Power BI. Analizezi datele și identifici tendințe. Prezinți concluzii managementului.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 15000, 28000, false,
            ["Power BI", "SQL", "Excel", "Data Analysis"]));

        jobs.Add(J("dataflow", "Data & AI", "Business Intelligence Developer",
            "Construiești soluții BI end-to-end: de la modele de date la dashboarduri interactive.",
            "3+ ani BI development. Cunoaștere SQL Server Analysis Services, Power BI sau Tableau. Experiență ETL.",
            "Proiectezi modele dimensionale. Implementezi ETL-uri. Creezi rapoarte automate și dashboarduri interactive.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 20000, 35000, false,
            ["Power BI", "SQL", "SQL Server", "Data Analysis", "Tableau"]));

        jobs.Add(J("intellidata", "Data & AI", "AI/ML Consultant",
            "Consultant care ajută companiile moldovenești să identifice și să implementeze cazuri de utilizare AI.",
            "5+ ani experiență tehnică în AI/ML. Capacitate de a traduce nevoile de business în soluții tehnice. Comunicare excelentă.",
            "Analizezi procesele clientului și identifici oportunități AI. Propui soluții și estimezi ROI. Supervizezi implementarea.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 35000, 60000, true,
            ["Python", "Machine Learning", "Data Analysis", "Comunicare"]));

        jobs.Add(J("dataflow", "Data & AI", "Python Developer (Data & Automation)",
            "Developer Python pentru automatizarea proceselor interne și construirea de tool-uri de analiză.",
            "2+ ani Python. Cunoaștere pandas, numpy, requests. Experiență cu automatizare și scripting.",
            "Scrii scripturi Python pentru automatizare date. Construiești tool-uri interne. Menții pipeline-uri existente.",
            "Chișinău, Moldova", JobType.Remote, EmploymentType.FullTime, 18000, 32000, false,
            ["Python", "Pandas", "NumPy", "SQL", "Git"]));

        jobs.Add(J("intellidata", "Data & AI", "Data Quality Analyst",
            "Asiguri calitatea datelor în sistemele noastre de date. Identifici și rezolvi probleme de consistență și acuratețe.",
            "2+ ani în data quality sau BI. Cunoaștere SQL. Experiență cu profilarea datelor și reguli de validare.",
            "Definești reguli de calitate a datelor. Monitorizezi pipelines. Raportezi anomalii și colaborezi cu inginerii.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 14000, 24000, false,
            ["SQL", "Excel", "Data Analysis"]));

        jobs.Add(J("dataflow", "Data & AI", "Data Scientist (NLP / Text Analytics)",
            "Proiect interesant: analiză sentiment și extragere informații din documente pentru un client financiar.",
            "3+ ani Data Science. Experiență cu NLP (spaCy, NLTK, transformers). Python avansat.",
            "Construiești modele NLP pentru clasificare și extragere informații. Evaluezi și îmbunătățești modelele. Prezinți rezultatele.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 28000, 48000, false,
            ["Python", "Machine Learning", "TensorFlow", "Pandas", "NumPy"]));

        jobs.Add(J("intellidata", "Data & AI", "RPA Developer (UiPath)",
            "Automatizăm procese manuale repetitive pentru clienți din banking și asigurări. Stack: UiPath + Python.",
            "1+ ani UiPath sau Automation Anywhere. Cunoaștere procese business. Logică de programare solidă.",
            "Analizezi și documentezi procesele de automatizat. Dezvolți boți RPA. Menții și îmbunătățești roboții existenți.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 16000, 28000, false,
            ["Python", "SQL"]));

        jobs.Add(J("dataflow", "Data & AI", "Cloud Data Architect",
            "Arhitect de date cloud responsabil de migrarea infrastructurii de date on-premise în Azure / AWS.",
            "5+ ani arhitectură date. Experiență cu Azure Synapse, AWS Redshift sau GCP BigQuery. Cunoaștere Lake House.",
            "Proiectezi arhitectura data platform cloud. Migrezi date existente. Stabilești standardele și best practices.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 40000, 70000, true,
            ["AWS", "Azure", "Spark", "SQL", "Python"]));

        jobs.Add(J("intellidata", "Data & AI", "Tableau Developer",
            "Specialist vizualizare date Tableau pentru rapoartele strategice ale clienților noștri corporativi.",
            "2+ ani Tableau. Cunoaștere Tableau Desktop și Server. SQL intermediar. Simț estetic pentru vizualizări.",
            "Creezi dashboard-uri Tableau interactive. Optimizezi performanța rapoartelor. Antrenezi utilizatorii finali.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 15000, 26000, false,
            ["Tableau", "SQL", "Data Analysis", "Excel"]));

        jobs.Add(J("dataflow", "Data & AI", "Junior Data Analyst",
            "Prima ta poziție în data analytics! Echipă prietenoasă, mentorat, proiecte reale de la prima săptămână.",
            "Studii în matematică, statistică, informatică sau domeniu conex. Excel avansat. Cunoaștere de bază SQL.",
            "Cureți și analizezi seturi de date. Creezi rapoarte în Excel și Power BI. Înveți de la colegi seniori.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 10000, 16000, false,
            ["Excel", "SQL", "Data Analysis", "Power BI"]));

        // ────────────────────────────────────────────────────────
        // 3. DEVOPS & CLOUD (10 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("cloudscale", "DevOps & Cloud", "Senior DevOps Engineer (AWS)",
            "CloudScale caută un Senior DevOps pentru a gestiona infrastructura cloud a clienților noștri enterprise.",
            "5+ ani DevOps/SRE. Expert AWS (EC2, EKS, RDS, S3). Terraform, GitHub Actions, monitoring. Engleza B2+.",
            "Proiectezi infrastructuri cloud scalabile. Implementezi CI/CD pipelines. Asiguri disponibilitatea și securitatea.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 35000, 60000, false,
            ["AWS", "Docker", "Kubernetes", "Terraform", "CI/CD", "Git"]));

        jobs.Add(J("cloudscale", "DevOps & Cloud", "Kubernetes Administrator",
            "Administrator Kubernetes pentru clustere de producție cu zeci de microservicii. Mediu AWS EKS.",
            "3+ ani Kubernetes în producție. Cunoaștere Helm, Istio, monitoring (Prometheus/Grafana). Linux avansat.",
            "Administrezi clustere Kubernetes. Implementezi upgrade-uri și patch-uri. Optimizezi resursele și costurile cloud.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 30000, 50000, false,
            ["Kubernetes", "Docker", "AWS", "Linux", "CI/CD"]));

        jobs.Add(J("cloudscale", "DevOps & Cloud", "Cloud Solutions Architect (Azure)",
            "Arhitect soluții Azure pentru clienți corporativi care migrează din on-premise.",
            "6+ ani IT, din care 3+ Azure. Certificare AZ-305 sau AZ-104. Experiență cu migrări cloud.",
            "Proiectezi arhitecturi Azure. Prezinți soluții clienților. Supervizezi implementarea și migrarea.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 45000, 75000, true,
            ["Azure", "Terraform", "Docker", "CI/CD", "Git"]));

        jobs.Add(J("cloudscale", "DevOps & Cloud", "Site Reliability Engineer (SRE)",
            "SRE responsabil de fiabilitatea și performanța platformelor clientilor noștri cu SLA 99.9%.",
            "4+ ani SRE sau DevOps. Cunoaștere SLI/SLO/SLA, incident management, chaos engineering.",
            "Definești și monitorizezi SLI-uri. Gestionezi incidentele de producție. Implementezi soluții de auto-healing.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 32000, 55000, false,
            ["Kubernetes", "AWS", "Linux", "CI/CD", "Docker"]));

        jobs.Add(J("cloudscale", "DevOps & Cloud", "CI/CD Pipeline Engineer",
            "Specialist care construiește și optimizează pipeline-uri de integrare și livrare continuă.",
            "2+ ani CI/CD. Cunoaștere GitHub Actions, Jenkins sau GitLab CI. Docker. Cunoaștere scripting Bash/Python.",
            "Construiești și menții pipeline-uri CI/CD. Automatizezi teste și deployment. Documentezi procesele DevOps.",
            "Chișinău, Moldova", JobType.Remote, EmploymentType.FullTime, 22000, 38000, false,
            ["CI/CD", "Docker", "GitHub Actions", "Jenkins", "Git", "Linux"]));

        jobs.Add(J("cloudscale", "DevOps & Cloud", "Linux System Administrator",
            "Administrator sisteme Linux pentru infrastructura on-premise și cloud a companiei noastre.",
            "3+ ani administrare Linux (RHEL/Ubuntu). Cunoaștere bash scripting, rețele, securitate. Disponibilitate on-call.",
            "Administrezi servere Linux de producție. Implementezi patch-uri și upgrade-uri. Monitorizezi și troubleshootezi.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 20000, 35000, false,
            ["Linux", "CI/CD", "Git", "Docker"]));

        jobs.Add(J("cloudscale", "DevOps & Cloud", "Terraform / Infrastructure as Code Specialist",
            "Specialist IaC care standardizează provisioning-ul infrastructurii cloud pentru 20+ clienți.",
            "2+ ani Terraform. Cunoaștere AWS sau Azure. Experiență cu module Terraform și state management.",
            "Scrii și menții module Terraform. Migrezi infrastructura manuală în cod. Revizuiești PRs de infrastructură.",
            "Chișinău, Moldova", JobType.Remote, EmploymentType.FullTime, 25000, 42000, false,
            ["Terraform", "AWS", "Azure", "Git", "Linux"]));

        jobs.Add(J("cloudscale", "DevOps & Cloud", "Junior DevOps Engineer",
            "Prima ta poziție DevOps! Vei învăța de la ingineri cu experiență, pe proiecte reale AWS.",
            "Cunoaștere Linux de bază. Scripting Bash sau Python. Cunoaștere Git. Motivație pentru automatizare.",
            "Ajuți la configurarea și monitorizarea infrastructurii. Înveți Terraform și Docker. Participi la on-call rotație.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 14000, 22000, false,
            ["Linux", "Git", "Docker", "CI/CD"]));

        jobs.Add(J("cloudscale", "DevOps & Cloud", "GCP Cloud Engineer",
            "Engineer GCP pentru proiecte data platform și machine learning pe Google Cloud.",
            "2+ ani GCP. Cunoaștere BigQuery, Dataflow, Cloud Run. Terraform. Experiență cu proiecte data sau ML.",
            "Provisionezi și menții resurse GCP. Optimizezi costurile cloud. Colaborezi cu echipele de data science.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 28000, 48000, false,
            ["GCP", "Terraform", "Docker", "Python", "Git"]));

        jobs.Add(J("cloudscale", "DevOps & Cloud", "Ansible Automation Engineer",
            "Automatizezi configurarea și gestionarea a sute de servere folosind Ansible și alte tool-uri IaC.",
            "2+ ani Ansible. Cunoaștere YAML, playbooks, roles, inventories. Linux. Experiență cu CM la scară.",
            "Scrii playbooks Ansible pentru provisioning și configurare. Menții inventory-ul de servere. Documentezi procesele.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 22000, 38000, false,
            ["Ansible", "Linux", "Git", "CI/CD"]));

        // ────────────────────────────────────────────────────────
        // 4. MEDIA & DESIGN (10 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("designlab", "Media & Design", "Senior UX/UI Designer (Figma)",
            "Designer UX/UI experimentat pentru produse digitale complexe. Lucrezi cu clienți din 3 țări.",
            "4+ ani UX/UI. Expert Figma. Experiență cu cercetare utilizatori, wireframing și prototipare. Portofoliu solid.",
            "Conduci procesul de design de la research la prototip. Prezinți soluții clienților. Mentorezezi juniori.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 22000, 38000, false,
            ["Figma", "UX Design", "UI Design", "Prototyping", "Wireframing"]));

        jobs.Add(J("designlab", "Media & Design", "Motion Designer (After Effects)",
            "Motion designer pentru reclame video, animații UI și content social media pentru branduri mari.",
            "2+ ani motion design. Expert After Effects. Cunoaștere Premiere Pro. Simț estetic dezvoltat.",
            "Creezi animații și video-uri pentru clienți. Colaborezi cu echipa de branding. Respecți deadline-urile strânse.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 15000, 28000, false,
            ["After Effects", "Adobe Premiere", "Adobe Illustrator"]));

        jobs.Add(J("pixelmedia", "Media & Design", "Fotograf & Videograf Comercial",
            "Studio din Cahul caută fotograf/videograf pentru shooting-uri de produs, corporate și reclame.",
            "2+ ani experiență foto/video comercial. Echipament propriu avantaj. Cunoaștere editare (Lightroom, Premiere).",
            "Realizezi shooting-uri foto și video. Editezi materialele în post-producție. Livrezi materiale conform brief.",
            "Cahul, Moldova", JobType.OnSite, EmploymentType.FullTime, 12000, 22000, true,
            ["Adobe Premiere", "Adobe Photoshop"]));

        jobs.Add(J("designlab", "Media & Design", "Brand Designer",
            "Designer responsabil de identitățile vizuale ale brandurilor clienților noștri: logo, culori, tipografie, guidelines.",
            "3+ ani brand design. Expert Adobe Illustrator și Photoshop. Portofoliu cu proiecte de branding.",
            "Creezi identități vizuale complete. Proiectezi materiale print și digital. Prezinți și aperi conceptele de design.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 16000, 28000, false,
            ["Adobe Illustrator", "Adobe Photoshop", "Figma", "UI Design"]));

        jobs.Add(J("pixelmedia", "Media & Design", "Graphic Designer (Social Media)",
            "Designer creativ pentru conținut social media al clienților noștri: postări, stories, reclame.",
            "1+ ani design grafic. Cunoaștere Photoshop și/sau Canva. Creativitate și viteză de execuție.",
            "Creezi materiale vizuale zilnice pentru social media. Respecți identitățile vizuale ale clienților. Lucrezi cu brief-uri.",
            "Cahul, Moldova", JobType.Remote, EmploymentType.FullTime, 10000, 18000, false,
            ["Adobe Photoshop", "Adobe Illustrator", "Canva"]));

        jobs.Add(J("designlab", "Media & Design", "Web Designer (Figma + HTML/CSS)",
            "Designer web care poate livra atât design-uri Figma, cât și să le implementeze în HTML/CSS.",
            "2+ ani web design. Cunoaștere Figma și HTML/CSS. Înțelegere responsive design și accesibilitate.",
            "Proiectezi pagini web în Figma. Implementezi design-uri în HTML/CSS. Colaborezi cu developerii frontend.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 14000, 24000, false,
            ["Figma", "UI Design", "HTML", "CSS", "Wireframing"]));

        jobs.Add(J("pixelmedia", "Media & Design", "Video Editor",
            "Editor video pentru producții comerciale, tutoriale și reclame. Clientelă variată, proiecte interesante.",
            "1+ ani editare video profesională. Expert Premiere Pro și After Effects. Atenție la detalii și storytelling vizual.",
            "Editezi footage-uri brute în produse finale. Adaugi grafică, muzică și efecte. Respecți brandurile clienților.",
            "Cahul, Moldova", JobType.OnSite, EmploymentType.FullTime, 10000, 20000, false,
            ["Adobe Premiere", "After Effects"]));

        jobs.Add(J("designlab", "Media & Design", "Junior UX Designer",
            "Rol de junior UX pentru proaspăt absolvenți sau autodidacți cu portofoliu. Mentorat garantat.",
            "Cunoaștere Figma. Interes pentru UX research și design thinking. Portofoliu cu proiecte personale sau universitare.",
            "Participi la ateliere de user research. Creezi wireframes și prototipuri low-fidelity. Înveți procesele de design.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 9000, 15000, false,
            ["Figma", "Wireframing", "UX Design", "Prototyping"]));

        jobs.Add(J("pixelmedia", "Media & Design", "Content Creator (Video + Social Media)",
            "Creator de conținut video pentru platformele social media ale clienților noștri locali.",
            "Experiență în creare conținut video TikTok/Instagram/YouTube. Editare video de bază. Creativitate.",
            "Filmezi și editezi conținut video scurt. Administrezi calendarul editorial. Analizezi performanța postărilor.",
            "Cahul, Moldova", JobType.OnSite, EmploymentType.PartTime, 7000, 12000, true,
            ["Adobe Premiere", "Canva"]));

        jobs.Add(J("designlab", "Media & Design", "3D Visualizator (Arhitectură / Produs)",
            "Specialist vizualizări 3D pentru proiecte arhitecturale și de produs ale clienților noștri.",
            "2+ ani 3D rendering (3ds Max, Blender sau Cinema 4D). Cunoaștere lumini, materiale și postprocesare.",
            "Creezi randări 3D fotorealiste. Colaborezi cu arhitecți și designeri industriali. Respecți termenele livrate.",
            "Chișinău, Moldova", JobType.Remote, EmploymentType.FullTime, 14000, 26000, true,
            ["Adobe Photoshop", "Adobe Illustrator"]));

        // ────────────────────────────────────────────────────────
        // 5. FINANȚE & CONTABILITATE (12 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("fintech", "Finanțe & Contabilitate", "Contabil Senior (1C + IFRS)",
            "Contabil senior pentru compania noastră fintech. Responsabil de raportarea financiară lunară și anuală.",
            "5+ ani contabilitate. Expert 1C Contabilitate. Cunoaștere IFRS și standardele moldovenești. CIPA avantaj.",
            "Pregătești rapoartele financiare lunare și anuale. Gestionezi contabilitatea primară. Colaborezi cu auditorii.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 18000, 30000, false,
            ["Contabilitate", "1C Contabilitate", "IFRS", "Excel avansat", "Raportare financiară"]));

        jobs.Add(J("auditpro", "Finanțe & Contabilitate", "Auditor Financiar (ACCA)",
            "Auditor pentru misiuni de audit extern la companii din Moldova. Echipă cu experiență internațională.",
            "Certificare ACCA sau în curs. 2+ ani audit. Cunoaștere ISA și IFRS. Mobilitate pentru deplasări scurte.",
            "Efectuezi proceduri de audit la clienți. Documentezi probele de audit. Redactezi rapoarte de audit.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 20000, 35000, false,
            ["Audit", "IFRS", "Excel avansat", "Analiză financiară"]));

        jobs.Add(J("fintech", "Finanțe & Contabilitate", "Analist Financiar",
            "Analist financiar pentru departamentul de planning & analysis al companiei noastre.",
            "2+ ani FP&A sau analiză financiară. Excel avansat. Cunoaștere Power BI avantaj. Gândire analitică.",
            "Construiești modele financiare și bugete. Analizezi varianțele față de plan. Pregătești prezentări pentru management.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 16000, 28000, false,
            ["Analiză financiară", "Excel avansat", "Raportare financiară", "Power BI"]));

        jobs.Add(J("auditpro", "Finanțe & Contabilitate", "Consultant Fiscal",
            "Consultant fiscal care oferă consultanță clienților noștri IMM în optimizare fiscală și conformitate.",
            "3+ ani consultanță fiscală. Cunoaștere legislației fiscale moldovenești. Capacitate de comunicare cu clienții.",
            "Consiliezi clienții în probleme fiscale. Verifici declarații fiscale. Reprezinți clienții în fața autorităților.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 18000, 32000, true,
            ["Contabilitate", "Excel avansat", "Raportare financiară"]));

        jobs.Add(J("fintech", "Finanțe & Contabilitate", "Risk Manager (Sectorul Bancar)",
            "Manager de risc pentru platforma noastră de plăți digitale. Responsabil de managementul riscului de credit și operațional.",
            "4+ ani managementul riscului în bancă sau fintech. Cunoaștere Basel III, AML, KYC. Engleza B2.",
            "Identifici și evaluezi riscurile. Propui și implementezi măsuri de mitigare. Raportezi comitetului de risc.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 25000, 45000, false,
            ["Analiză financiară", "IFRS", "Excel avansat"]));

        jobs.Add(J("auditpro", "Finanțe & Contabilitate", "Contabil Junior",
            "Prima ta poziție în contabilitate! Ghidare și mentorat din prima zi. Creștem împreună.",
            "Studii în contabilitate sau economie. Cunoaștere de bază 1C sau Excel. Atenție la detalii.",
            "Înregistrezi tranzacțiile contabile primare. Verifici documente. Asistezi contabilii seniori.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 8000, 14000, false,
            ["Contabilitate", "1C Contabilitate", "Excel"]));

        jobs.Add(J("fintech", "Finanțe & Contabilitate", "Controller Financiar",
            "Controller pentru subsidiare din Moldova ale unui grup internațional. Raportezi în format IFRS.",
            "5+ ani controlling financiar. IFRS obligatoriu. SAP avantaj major. Engleza B2.",
            "Pregătești raportarea lunară IFRS. Analizezi abateri de buget. Participi la audit anual.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 25000, 42000, false,
            ["IFRS", "SAP", "Excel avansat", "Raportare financiară", "Analiză financiară"]));

        jobs.Add(J("auditpro", "Finanțe & Contabilitate", "Specialist Payroll",
            "Specialist salarizare pentru portofoliul nostru de 50+ clienți outsourcing payroll.",
            "2+ ani calcul salarii. Cunoaștere legislației muncii moldovenești. 1C Salarizare. Acuratețe și discreție.",
            "Calculezi salariile lunare ale angajaților clienților. Pregătești declarațiile BASS. Răspunzi la întrebările angajaților.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 12000, 22000, false,
            ["Contabilitate", "1C Contabilitate", "Excel avansat"]));

        jobs.Add(J("fintech", "Finanțe & Contabilitate", "Compliance Officer (AML/KYC)",
            "Ofițer de conformitate specializat în prevenirea spălării banilor pentru platforma noastră de plăți.",
            "3+ ani în compliance bancară sau fintech. Cunoaștere AML/KYC. Certificare CAMS avantaj.",
            "Monitorizezi tranzacțiile suspecte. Efectuezi due diligence clienți. Raportezi autorităților de reglementare.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 20000, 35000, false,
            ["Analiză financiară", "IFRS"]));

        jobs.Add(J("auditpro", "Finanțe & Contabilitate", "Trezorier / Cash Manager",
            "Manager trezorerie responsabil de lichiditatea companiei și relațiile cu băncile.",
            "3+ ani trezorerie sau corporate banking. Cunoaștere instrumente financiare. Excel financiar avansat.",
            "Gestionezi fluxurile de numerar zilnice. Negociezi cu băncile. Optimizezi costurile de finanțare.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 18000, 32000, false,
            ["Excel avansat", "Analiză financiară", "Raportare financiară"]));

        jobs.Add(J("fintech", "Finanțe & Contabilitate", "SAP FI/CO Consultant",
            "Consultant SAP pentru implementarea și optimizarea modulelor financiare la clienții noștri enterprise.",
            "3+ ani SAP FI/CO. Experiență în implementări SAP. Cunoaștere procese financiare end-to-end.",
            "Configurezi module SAP FI/CO. Suporți utilizatorii cheie. Documentezi configurațiile și procesele.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 28000, 48000, true,
            ["SAP", "IFRS", "Analiză financiară"]));

        jobs.Add(J("auditpro", "Finanțe & Contabilitate", "Director Financiar (CFO) part-time",
            "Serviciu de CFO externalizat pentru companii în creștere care nu au nevoie de un CFO full-time.",
            "10+ ani în poziții financiare senior. Experiență CFO sau director financiar. MBA sau master în finanțe avantaj.",
            "Supervizezi funcția financiară a clientului. Consiliezi board-ul. Supraveghezi bugetare, audit și conformitate.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.PartTime, 30000, 60000, true,
            ["IFRS", "Audit", "Analiză financiară", "Raportare financiară", "SAP"]));

        // ────────────────────────────────────────────────────────
        // 6. MARKETING & PR (10 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("markethub", "Marketing & PR", "Digital Marketing Manager",
            "Manager marketing digital pentru un portofoliu de 15 branduri locale. Echipă de 8 specialiști.",
            "4+ ani marketing digital. Expert Google Ads, Meta Ads, SEO. Cunoaștere Google Analytics 4.",
            "Gestionezi campaniile PPC și SEO ale clienților. Analizezi performanța și optimizezi ROI. Raportezi lunar clienților.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 20000, 35000, false,
            ["Google Ads", "Meta Ads", "SEO", "Google Analytics", "Email Marketing"]));

        jobs.Add(J("markethub", "Marketing & PR", "SEO Specialist",
            "Specialist SEO pentru clienți din e-commerce, servicii și B2B. Rezultate măsurabile, abordare data-driven.",
            "2+ ani SEO on-page și off-page. Cunoaștere Ahrefs/SEMrush. Înțelegere HTML de bază. Cunoaștere algoritmi Google.",
            "Efectuezi audituri SEO. Optimizezi conținutul și structura site-urilor. Construiești profil de link-uri. Raportezi.",
            "Chișinău, Moldova", JobType.Remote, EmploymentType.FullTime, 14000, 25000, false,
            ["SEO", "Google Analytics", "Content Writing"]));

        jobs.Add(J("markethub", "Marketing & PR", "Specialist Google Ads (PPC)",
            "Specialist PPC care gestionează conturi Google Ads cu bugete de până la 200k MDL/lună.",
            "2+ ani Google Ads. Certificare Google avantaj. Cunoaștere Google Analytics. Analitică și optimizare continuă.",
            "Creezi și optimizezi campanii Search, Display, Shopping. Analizezi datele și îmbunătățești ROAS. Raportezi.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 15000, 28000, false,
            ["Google Ads", "Google Analytics", "Meta Ads"]));

        jobs.Add(J("prmotion", "Marketing & PR", "PR Manager",
            "Manager PR pentru clienți corporativi din nordul Moldovei. Relații cu presa, comunicare de criză.",
            "3+ ani PR sau comunicare. Relații bune cu presa moldovenească. Scriitură excelentă în română și rusă.",
            "Redactezi comunicate de presă. Organizezi conferințe de presă. Gestionezi relațiile cu jurnaliștii.",
            "Bălți, Moldova", JobType.OnSite, EmploymentType.FullTime, 14000, 24000, false,
            ["Copywriting", "Comunicare", "Content Writing"]));

        jobs.Add(J("markethub", "Marketing & PR", "Social Media Manager",
            "Manager social media pentru branduri din food, fashion și servicii. Creativitate și consistență.",
            "2+ ani social media management. Cunoaștere Facebook Business Manager. Simț estetic. Copywriting.",
            "Creezi și publici conținut pe social media. Gestionezi comunitatea. Planifici calendarul editorial.",
            "Chișinău, Moldova", JobType.Remote, EmploymentType.FullTime, 12000, 22000, false,
            ["Social Media", "Copywriting", "Meta Ads", "Content Writing"]));

        jobs.Add(J("markethub", "Marketing & PR", "Content Marketing Specialist",
            "Specialist content pentru strategia de marketing de conținut a agenției și clienților ei.",
            "2+ ani content marketing sau jurnalism. Scriitură excelentă. Cunoaștere SEO de bază. Creativitate.",
            "Redactezi articole de blog, whitepaper și studii de caz. Optimizezi conținut pentru SEO. Coordonezi alți autori.",
            "Chișinău, Moldova", JobType.Remote, EmploymentType.FullTime, 12000, 22000, false,
            ["Content Writing", "SEO", "Copywriting", "Email Marketing"]));

        jobs.Add(J("prmotion", "Marketing & PR", "Specialist Email Marketing",
            "Specialist email marketing pentru campanii automatizate de nurturing și retention.",
            "1+ ani email marketing. Cunoaștere Mailchimp sau Klaviyo. Segmentare audiențe. A/B testing.",
            "Creezi campanii email automate. Segmentezi lista de contacte. Analizezi ratele de deschidere și conversie.",
            "Bălți, Moldova", JobType.Remote, EmploymentType.FullTime, 11000, 20000, false,
            ["Email Marketing", "Copywriting", "Google Analytics"]));

        jobs.Add(J("markethub", "Marketing & PR", "Brand Manager",
            "Brand Manager care dezvoltă și execută strategia de brand pentru 3-5 clienți din portofoliu.",
            "4+ ani brand management sau marketing. Gândire strategică. Experiență cu cercetare de piață și consumatori.",
            "Definești pozitionarea brandului. Supervizezi execuția campaniilor. Analizezi brand health metrics.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 20000, 35000, true,
            ["Social Media", "Google Analytics", "Comunicare"]));

        jobs.Add(J("markethub", "Marketing & PR", "Junior Marketing Specialist",
            "Rol de intrare în marketing! Vei sprijini echipa în execuția campaniilor și analiza datelor.",
            "Studii în marketing sau comunicare. Cunoaștere de bază social media. Excel. Entuziasm și dorința de a învăța.",
            "Asistezi la campaniile PPC și social media. Creezi rapoarte în Excel. Înveți tool-urile de marketing digital.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 8000, 14000, false,
            ["Social Media", "Google Analytics", "Email Marketing"]));

        jobs.Add(J("prmotion", "Marketing & PR", "Event Manager",
            "Manager de evenimente pentru conferințe corporate, lansări de produs și evenimente B2B în Moldova.",
            "2+ ani organizare evenimente. Rețea de furnizori (locații, catering, AV). Organizare și management timp.",
            "Planifici și organizezi evenimente de la A la Z. Coordonezi furnizorii. Asiguri desfășurarea fără probleme.",
            "Bălți, Moldova", JobType.OnSite, EmploymentType.FullTime, 12000, 22000, false,
            ["Comunicare", "Managementul proiectelor"]));

        // ────────────────────────────────────────────────────────
        // 7. HR & RECRUTARE (8 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("hrprime", "HR & Recrutare", "Senior Recruiter (IT)",
            "Recrutor senior specializat pe profile IT: developeri, DevOps, Data. Networking activ și headhunting.",
            "3+ ani recrutare IT. Cunoaștere profilurilor tehnice. Experiență LinkedIn Recruiter. Networking puternic.",
            "Identifici, contactezi și evaluezi candidați IT. Conduci interviuri HR. Colaborezi cu managerii tehnici.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 18000, 32000, false,
            ["Recrutare", "Interviuri", "HR Analytics", "Employer Branding"]));

        jobs.Add(J("hrprime", "HR & Recrutare", "HR Business Partner",
            "HR BP care sprijină managerii de linie în toate aspectele legate de oameni: performanță, dezvoltare, conflicte.",
            "5+ ani HR. Experiență ca HR BP sau HR Generalist. Cunoaștere legislației muncii. Comunicare excelentă.",
            "Consiliezi managerii în probleme HR. Gestionezi procese de performanță. Participi la planificarea forței de muncă.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 20000, 35000, false,
            ["HR Analytics", "Onboarding", "Interviuri", "Comunicare"]));

        jobs.Add(J("hrprime", "HR & Recrutare", "Specialist Payroll & Administrare Personal",
            "Specialist salarizare și administrare personal pentru clienții noștri de outsourcing HR.",
            "2+ ani payroll Moldova. 1C Salarizare. Cunoaștere Codul Muncii. Acuratețe și confidențialitate.",
            "Calculezi salariile. Pregătești ordinele de personal. Gestionezi fișele de pontaj. Răspunzi la angajați.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 12000, 20000, false,
            ["Payroll", "Onboarding", "HR Analytics"]));

        jobs.Add(J("hrprime", "HR & Recrutare", "Learning & Development Specialist",
            "Specialist L&D care proiectează și implementează programe de formare pentru clienții noștri corporativi.",
            "3+ ani L&D sau training. Cunoaștere design instrucțional. Experiență cu platforme e-learning (Moodle, TalentLMS).",
            "Identifici nevoile de formare. Proiectezi cursuri și workshop-uri. Facilitezi sesiuni de training. Măsori impactul.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 15000, 26000, false,
            ["HR Analytics", "Comunicare", "Onboarding"]));

        jobs.Add(J("hrprime", "HR & Recrutare", "Recrutor Junior",
            "Prima ta poziție în recrutare! Vei învăța întreg procesul de recrutare sub ghidarea unui senior.",
            "Studii în HR, psihologie sau domeniu conex. Comunicare bună. Cunoaștere LinkedIn avantaj.",
            "Postezi anunțurile de angajare. Faci screening-ul CV-urilor. Contactezi candidații. Programezi interviuri.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 8000, 14000, false,
            ["Recrutare", "Interviuri", "Comunicare"]));

        jobs.Add(J("hrprime", "HR & Recrutare", "Employer Branding Specialist",
            "Specialist care construiește și promovează brandul de angajator al clienților noștri.",
            "2+ ani marketing sau HR. Înțelegere employer branding. Cunoaștere social media. Scriere creativă.",
            "Dezvolți strategia de employer branding. Creezi conținut pentru LinkedIn și job boards. Organizezi târguri de joburi.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 13000, 22000, false,
            ["Employer Branding", "Social Media", "Comunicare", "Recrutare"]));

        jobs.Add(J("hrprime", "HR & Recrutare", "HR Manager",
            "HR Manager pentru o companie de producție cu 150 angajați. Responsabil de toată funcția HR.",
            "5+ ani HR, din care 2+ management. Cunoaștere completă procese HR. Lidereship și comunicare.",
            "Conduci echipa HR. Gestionezi recrutarea, salarizarea și administrarea personalului. Raportezi directorului general.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 22000, 38000, false,
            ["Recrutare", "Payroll", "HR Analytics", "Onboarding", "Leadership"]));

        jobs.Add(J("hrprime", "HR & Recrutare", "Specialist Compensații & Beneficii",
            "Specialist C&B care proiectează și gestionează pachetele de compensare pentru clienții noștri.",
            "3+ ani în compensații și beneficii sau analiză HR. Excel avansat. Cunoaștere benchmarking salarial.",
            "Efectuezi studii de benchmarking salarial. Proiectezi structuri de salarii. Analizezi datele de compensare.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 16000, 28000, false,
            ["HR Analytics", "Payroll", "Excel avansat"]));

        // ────────────────────────────────────────────────────────
        // 8. MANAGEMENT & BUSINESS (8 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("bizmoldova", "Management & Business", "Business Consultant",
            "Consultant de business pentru IMM-uri moldovenești: strategie, eficiență operațională, creștere.",
            "4+ ani consultanță sau management. Gândire analitică și strategică. Experiență cu diagnostice de business.",
            "Analizezi situația clientului. Propui soluții de îmbunătățire. Supervizezi implementarea. Raportezi progresul.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 22000, 40000, true,
            ["Agile", "Leadership", "Comunicare", "Managementul proiectelor"]));

        jobs.Add(J("bizmoldova", "Management & Business", "Project Manager (PMI)",
            "Manager de proiect pentru inițiative de transformare organizațională la clienți corporate.",
            "4+ ani PM. Certificare PMP sau PRINCE2. Experiență cu proiecte complexe multi-stakeholder.",
            "Planifici și execuți proiecte de la initiere la closing. Gestionezi riscurile și schimbările. Raportezi statusul.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 25000, 42000, false,
            ["Managementul proiectelor", "Agile", "Scrum", "Jira", "Leadership"]));

        jobs.Add(J("bizmoldova", "Management & Business", "Business Development Manager",
            "Manager de dezvoltare business pentru atragerea de noi clienți și parteneri în Moldova și regiune.",
            "4+ ani business development sau sales B2B. Rețea de contacte. Negociere și prezentări executive.",
            "Identifici și contactezi potențiali clienți. Negociezi contracte. Reprezinți compania la events și conferințe.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 20000, 40000, true,
            ["Comunicare", "Leadership", "Negociere", "Microsoft Office"]));

        jobs.Add(J("bizmoldova", "Management & Business", "Operations Manager",
            "Manager operațional pentru supervizarea activităților zilnice ale companiei noastre de consultanță.",
            "5+ ani management operațional. Experiență cu optimizare procese. Leadership puternic și organizare.",
            "Supervizezi echipele operaționale. Optimizezi procesele interne. Asiguri atingerea KPI-urilor operaționale.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 25000, 45000, false,
            ["Leadership", "Managementul proiectelor", "Agile", "Comunicare"]));

        jobs.Add(J("bizmoldova", "Management & Business", "Analist Business (BA)",
            "Analist de business care identifică cerințele și traduce nevoile clientului în soluții.",
            "3+ ani BA. Experiență cu BPMN, use cases, user stories. Cunoaștere Agile. Comunicare excelentă.",
            "Colectezi și documentezi cerințele. Creezi diagrame de procese. Colaborezi cu echipele IT și business.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 18000, 32000, false,
            ["Agile", "Scrum", "Jira", "Comunicare", "Microsoft Office"]));

        jobs.Add(J("bizmoldova", "Management & Business", "Change Management Consultant",
            "Consultant specializat în gestionarea schimbării organizaționale pentru proiecte de transformare.",
            "5+ ani change management sau OD. Certificare Prosci sau ADKAR avantaj. Facilitare și coaching.",
            "Evaluezi impactul schimbării. Proiectezi planuri de change management. Facilitezi ateliere cu stakeholderi.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 25000, 45000, true,
            ["Leadership", "Comunicare", "Managementul proiectelor"]));

        jobs.Add(J("bizmoldova", "Management & Business", "Strategy Analyst",
            "Analist strategie care sprijină consultanții seniori în cercetare de piață și analize strategice.",
            "2+ ani analiză strategică sau consultanță. Excel și PowerPoint avantajate. Cercetare și sinteză de date.",
            "Efectuezi cercetare de piață. Analizezi competitive landscape. Pregătești slide deck-uri pentru clienți.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 15000, 26000, false,
            ["Microsoft Office", "Power Point", "Excel avansat", "Comunicare"]));

        jobs.Add(J("bizmoldova", "Management & Business", "General Manager (Delegat)",
            "Manager general pentru filiale ale unor companii internaționale care intră pe piața moldovenească.",
            "8+ ani în poziții de management general. Experiență cu P&L responsability. Cunoaștere piața moldovenească.",
            "Conduci operațiunile filialei. Atingi targetele de profitabilitate. Construiești echipa și cultura organizațională.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 50000, 90000, true,
            ["Leadership", "Managementul proiectelor", "Comunicare", "Agile"]));

        // ────────────────────────────────────────────────────────
        // 9. JURIDIC (8 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("legalmd", "Juridic", "Avocat Drept Corporativ",
            "Avocat pentru asistența juridică a companiilor în tranzacții, structuri corporative și conformitate.",
            "3+ ani drept corporativ. Cunoaștere Codul Civil și legislația societăților comerciale din Moldova. Engleza B2.",
            "Redactezi și negociezi contracte comerciale. Asistezi la înregistrarea și restructurarea companiilor.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 20000, 38000, false,
            ["Drept corporativ", "Drept comercial", "Contracte"]));

        jobs.Add(J("legalmd", "Juridic", "Jurist Intern (In-house Lawyer)",
            "Jurist intern pentru o companie de retail cu 500+ angajați. Variatie mare de teme juridice.",
            "2+ ani experiență juridică. Cunoaștere drept muncii, contracte, litigii. Organizare și autonomie.",
            "Redactezi contracte și documente juridice. Consiliezi departamentele interne. Monitorizezi legislația.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 16000, 28000, false,
            ["Contracte", "Drept comercial", "GDPR"]));

        jobs.Add(J("legalmd", "Juridic", "Specialist GDPR & Protecția Datelor",
            "Specialist GDPR pentru asistența companiilor din Moldova în conformitatea cu regulamentul european.",
            "2+ ani GDPR sau drept IT. Cunoaștere aprofundată GDPR și Legea nr. 133/2011 Moldova. Certificare avantaj.",
            "Efectuezi audituri GDPR. Redactezi politici de confidențialitate și consimțământ. Asistezi DPA.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 18000, 32000, false,
            ["GDPR", "Contracte", "Drept corporativ"]));

        jobs.Add(J("legalmd", "Juridic", "Avocat Litigii Comerciale",
            "Avocat specializat în reprezentarea companiilor în fața instanțelor judecătorești și arbitraj.",
            "4+ ani litigii. Experiență cu procedura civilă moldovenească. Cunoaștere drept comercial și contracte.",
            "Reprezinți clienții în instanță. Redactezi memorii și cereri. Negociezi soluționări amiabile.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 22000, 40000, false,
            ["Litigii", "Drept comercial", "Contracte"]));

        jobs.Add(J("legalmd", "Juridic", "Specialist Proprietate Intelectuală",
            "Specialist PI pentru protejarea mărcilor, brevetelor și drepturilor de autor ale clienților.",
            "2+ ani proprietate intelectuală. Cunoaștere legislației PI Moldova și internaționale (OMPI, EUIPO).",
            "Gestionezi portofoliul de mărci al clienților. Monitorizezi încălcările PI. Redactezi contracte de licență.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 18000, 32000, false,
            ["Proprietate intelectuală", "Drept corporativ", "Contracte"]));

        jobs.Add(J("legalmd", "Juridic", "Asistent Juridic / Paralegal",
            "Asistent juridic care sprijină avocații cu cercetare juridică, pregătire documente și administrare.",
            "Studii în drept. Cunoaștere sisteme juridice. Atenție la detalii. Cunoaștere MS Office.",
            "Efectuezi cercetare juridică. Pregătești schițe de documente. Gestionezi dosarele clienților.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 9000, 16000, false,
            ["Drept comercial", "Contracte", "Microsoft Office"]));

        jobs.Add(J("legalmd", "Juridic", "Avocat Dreptul Muncii",
            "Avocat specializat în consultanță și litigii de dreptul muncii pentru angajatori.",
            "3+ ani dreptul muncii. Cunoaștere Codul Muncii RM. Experiență cu litigii de muncă și negocieri colective.",
            "Consiliezi angajatorii în probleme de muncă. Redactezi acte normative interne. Reprezinți în litigii.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 20000, 36000, false,
            ["Drept comercial", "Litigii", "Contracte"]));

        jobs.Add(J("legalmd", "Juridic", "Legal Counsel (M&A)",
            "Consilier juridic specializat în fuziuni și achiziții. Proiecte transfrontaliere cu firme din UE.",
            "5+ ani M&A sau drept corporativ internațional. Engleza C1. Experiență due diligence și structurare tranzacții.",
            "Condii due diligence juridic. Negociezi și redactezi SPA, SHA. Coordonezi echipele locale și internaționale.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 35000, 65000, true,
            ["Drept corporativ", "Contracte", "Drept comercial", "GDPR"]));

        // ────────────────────────────────────────────────────────
        // 10. EDUCAȚIE (7 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("edupro", "Educație", "Trainer IT (C# / .NET)",
            "Trainer care predă cursuri de programare C#/.NET pentru adulți și reconversie profesională.",
            "5+ ani C#/.NET. Experiență în predare sau mentorat. Abilitate de a explica concepte complexe simplu.",
            "Predai cursuri de C# pentru începători și intermediari. Creezi materiale didactice. Evaluezi progresul.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.PartTime, 12000, 22000, true,
            ["C#", ".NET", "Predare", "Curriculum Design"]));

        jobs.Add(J("edupro", "Educație", "Profesor Limbi Străine (Engleză / Franceză)",
            "Profesor de limbi străine pentru cursuri corporate și individual. Certificare CELTA sau DELF avantaj.",
            "Certificare în predarea limbii. 2+ ani experiență. Metode moderne de predare. Entuziasm și răbdare.",
            "Predai cursuri de limbă în grup și individual. Evaluezi nivelul și progresul. Pregătești lecțiile.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.PartTime, 8000, 16000, false,
            ["Predare", "Comunicare", "E-learning"]));

        jobs.Add(J("edupro", "Educație", "Instructor Marketing Digital",
            "Instructor pentru cursurile noastre de marketing digital: Google Ads, SEO, Social Media.",
            "3+ ani marketing digital practic. Experiență în predare sau prezentări. Actualizezi cursurile constant.",
            "Predai module de marketing digital. Realizezi workshop-uri practice. Consiliezi cursanții în proiecte.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.PartTime, 10000, 18000, false,
            ["Google Ads", "SEO", "Social Media", "Predare"]));

        jobs.Add(J("edupro", "Educație", "E-learning Developer (Articulate / Moodle)",
            "Developer de conținut e-learning care convertește cursurile fizice în module online interactive.",
            "2+ ani e-learning development. Cunoaștere Articulate 360, Rise sau Lectora. Moodle sau TalentLMS.",
            "Proiectezi module e-learning interactive. Convertești materiale existente. Administrezi platforma LMS.",
            "Chișinău, Moldova", JobType.Remote, EmploymentType.FullTime, 14000, 24000, false,
            ["E-learning", "Curriculum Design", "Microsoft Office"]));

        jobs.Add(J("edupro", "Educație", "Coordonator Programe Educaționale",
            "Coordonator care gestionează portofoliul de cursuri, instructori și cursanți ai centrului.",
            "3+ ani coordonare educațională sau event management. Organizare și comunicare excelentă.",
            "Planifici calendarul de cursuri. Coordonezi instructorii. Comunici cu cursanții și partenerii corporativi.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 12000, 20000, false,
            ["Comunicare", "Managementul proiectelor", "Microsoft Office"]));

        jobs.Add(J("edupro", "Educație", "Psiholog Educațional / Career Coach",
            "Psiholog sau coach care susține cursanții în orientare profesională și dezvoltare personală.",
            "Diplomă în psihologie. Certificare coaching avantaj. Empatie și abilități de consiliere.",
            "Conduci sesiuni de career counseling. Facilitezi workshop-uri de soft skills. Evaluezi potențialul cursanților.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.PartTime, 9000, 16000, true,
            ["Comunicare", "Evaluare"]));

        jobs.Add(J("edupro", "Educație", "Instructor Contabilitate & Fiscalitate",
            "Instructor pentru cursurile de contabilitate practică și fiscalitate moldovenească.",
            "5+ ani contabilitate. Certificare CIPA avantaj. Experiență practică recentă în domeniu.",
            "Predai contabilitate conform standardelor moldovenești. Actualizezi cursul cu modificări legislative. Evaluezi.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.PartTime, 10000, 18000, false,
            ["Contabilitate", "1C Contabilitate", "Predare", "Evaluare"]));

        // ────────────────────────────────────────────────────────
        // 11. SĂNĂTATE (7 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("medclinic", "Sănătate", "Medic Internist",
            "Clinica noastră privată caută medic internist pentru consultații și monitorizarea pacienților cronici.",
            "Diplomă medicină, rezidențiat în medicină internă. Permis de practică RM. Experiență min. 2 ani.",
            "Efectuezi consultații și examinări. Stabilești diagnostic și tratament. Monitorizezi pacienții cronici.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 18000, 32000, true,
            ["Îngrijire pacienți", "Diagnostic"]));

        jobs.Add(J("medclinic", "Sănătate", "Asistentă Medicală",
            "Asistentă medicală pentru clinica noastră. Echipă prietenoasă, dotare modernă, condiții bune.",
            "Diplomă de asistentă medicală. Permis de practică activ. Experiență min. 1 an. Empatie și profesionalism.",
            "Asistezi medicii la consultații și proceduri. Administrezi tratamente. Comunici cu pacienții.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 10000, 18000, false,
            ["Îngrijire pacienți", "Farmacologie"]));

        jobs.Add(J("medclinic", "Sănătate", "Farmacist",
            "Farmacist pentru farmacia clinicii noastre. Relație directă cu pacienții și medicii.",
            "Licență în farmacie. Permis de practică RM. Cunoaștere legislației farmaceutice moldovenești.",
            "Eliberezi medicamentele conform prescripțiilor. Consiliezi pacienții. Gestionezi stocurile farmaciei.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 14000, 24000, false,
            ["Farmacologie", "Îngrijire pacienți"]));

        jobs.Add(J("medclinic", "Sănătate", "Medic Stomatolog",
            "Stomatolog pentru cabinet modern, dotat cu echipament de ultimă generație.",
            "Diplomă medicină dentară. Permis de practică activ. Experiență min. 1 an.",
            "Efectuezi consultații și tratamente stomatologice. Realizezi lucrări protetice. Comunici cu pacienții.",
            "Bălți, Moldova", JobType.OnSite, EmploymentType.FullTime, 20000, 40000, true,
            ["Diagnostic", "Îngrijire pacienți"]));

        jobs.Add(J("medclinic", "Sănătate", "Administrator Medical (Recepție)",
            "Administrator medical pentru gestionarea programărilor și relația cu pacienții la recepție.",
            "Studii medicale sau administrative. Cunoaștere EMR avantaj. Comunicare excelentă și răbdare.",
            "Programezi și confirmi consultațiile. Înregistrezi pacienții nou. Gestionezi documentele medicale.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 9000, 15000, false,
            ["EMR", "Comunicare", "Microsoft Office"]));

        jobs.Add(J("medclinic", "Sănătate", "Laborant Medical",
            "Laborant pentru laboratorul clinic al rețelei noastre de clinici. Analize hematologice și biochimice.",
            "Diplomă laborant medical sau chimie clinică. Permis de practică. Atenție la detalii și precizie.",
            "Recoltezi probe biologice. Efectuezi analize de laborator. Introduci rezultatele în sistemul EMR.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 11000, 19000, false,
            ["EMR", "Diagnostic"]));

        jobs.Add(J("medclinic", "Sănătate", "Kinetoterapeut",
            "Kinetoterapeut pentru programele de recuperare medicală ale clinicii noastre.",
            "Licență kinetoterapie. Permis de practică. Experiență min. 1 an. Comunicare și empatie.",
            "Evaluezi starea funcțională a pacienților. Elaborezi programe de recuperare. Aplici tehnici kinetoterapeutice.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 12000, 22000, false,
            ["Îngrijire pacienți", "Diagnostic"]));

        // ────────────────────────────────────────────────────────
        // 12. TRANSPORT & LOGISTICĂ (10 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("logisticmd", "Transport & Logistică", "Dispecer Transport Internațional",
            "Dispecer pentru coordonarea transporturilor TIR Moldova-UE. Echipă de 15 dispeceri, trafic intens.",
            "2+ ani dispecerat transport. Cunoaștere rute europene. Limba rusă și română. Rezistență la stres.",
            "Coordonezi șoferii și vehiculele pe rute internaționale. Gestionezi documentele de transport (CMR).",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 14000, 24000, false,
            ["Logistică", "CMR", "Gestionare flotă"]));

        jobs.Add(J("logisticmd", "Transport & Logistică", "Manager Logistică & Depozit",
            "Manager logistică pentru depozitul nostru de 5000 mp din Chișinău. Echipă de 20 depozitari.",
            "4+ ani logistică și managementul depozitului. Cunoaștere WMS. Leadership. Experiență cu inventarieri.",
            "Coordonezi activitatea depozitului. Optimizezi procesele de recepție, stocare și expediere. Raportezi KPI.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 18000, 32000, false,
            ["Logistică", "SAP WM", "Leadership", "Gestionare flotă"]));

        jobs.Add(J("moldtrans", "Transport & Logistică", "Expeditor Internațional de Mărfuri",
            "Expeditor pentru organizarea transporturilor internaționale rutiere și multimodale.",
            "2+ ani expeditor de mărfuri. Cunoaștere Incoterms, CMR, vamă. Negociere cu transportatori.",
            "Organizezi transporturi internaționale. Coordonezi cu transportatorii și clienții. Gestionezi documentația.",
            "Ungheni, Moldova", JobType.OnSite, EmploymentType.FullTime, 15000, 26000, false,
            ["Logistică", "CMR", "Negociere"]));

        jobs.Add(J("moldtrans", "Transport & Logistică", "Șofer TIR (Rute Internaționale)",
            "Șofer TIR cu permis categoria CE pentru rute Moldova-Romania-UE. Condiții bune, mașini noi.",
            "Permis CE. Carnet tahograf digital. Experiență min. 2 ani rute internaționale. Cunoaștere regulamente.",
            "Efectuezi curse internaționale Moldova-UE. Gestionezi documentele de transport. Asiguri securitatea mărfii.",
            "Ungheni, Moldova", JobType.OnSite, EmploymentType.FullTime, 18000, 30000, false,
            ["CMR", "Logistică"]));

        jobs.Add(J("logisticmd", "Transport & Logistică", "Analist Lanț de Aprovizionare",
            "Analist supply chain pentru optimizarea fluxurilor de marfă și reducerea costurilor logistice.",
            "2+ ani supply chain sau logistică. Excel avansat. Cunoaștere indicatori logistici. Gândire analitică.",
            "Analizezi datele de transport și depozitare. Identifici oportunități de optimizare. Raportezi managementului.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 16000, 28000, false,
            ["Logistică", "Excel avansat", "SAP WM"]));

        jobs.Add(J("moldtrans", "Transport & Logistică", "Coordonator Flotă Auto",
            "Coordonator responsabil de gestionarea flotei de 120 vehicule: mentenanță, licențe, costuri.",
            "3+ ani gestiune flotă auto. Cunoaștere legislației transportului rutier. Organizare și atenție la detalii.",
            "Planifici mentenanța vehiculelor. Gestionezi documentele flotei. Optimizezi costurile de operare.",
            "Ungheni, Moldova", JobType.OnSite, EmploymentType.FullTime, 15000, 26000, false,
            ["Gestionare flotă", "Logistică"]));

        jobs.Add(J("logisticmd", "Transport & Logistică", "Operator Vămuire (Brocker Vamal)",
            "Specialist vămuire pentru efectuarea formalităților vamale de import/export la posturile vamale RM.",
            "2+ ani vămuire. Cunoaștere Codului Vamal RM și TARIC. Autorizație de brocker vamal avantaj.",
            "Pregătești și depui declarațiile vamale. Comunici cu organele vamale. Consiliezi clienții pe teme vamale.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 14000, 24000, false,
            ["CMR", "Logistică"]));

        jobs.Add(J("moldtrans", "Transport & Logistică", "Manager Achiziții Transport",
            "Manager achiziții pentru negocierea și gestionarea contractelor cu transportatori și furnizori.",
            "3+ ani achiziții sau transport. Negociere și contract management. Cunoaștere piața transportului RM.",
            "Negociezi tarife cu transportatorii. Evaluezi și selectezi furnizori. Gestionezi contractele de transport.",
            "Ungheni, Moldova", JobType.OnSite, EmploymentType.FullTime, 16000, 28000, false,
            ["Logistică", "Negociere", "CMR"]));

        jobs.Add(J("logisticmd", "Transport & Logistică", "Controlor Calitate Depozit",
            "Controlor calitate care verifică marfa recepționată și expediată conform standardelor clientului.",
            "1+ ani control calitate sau depozit. Atenție la detalii. Cunoaștere lucru cu WMS avantaj.",
            "Verifici marfa la recepție și expediere. Documentezi discrepanțele. Raportezi managementului depozitului.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 11000, 18000, false,
            ["Logistică", "SAP WM"]));

        jobs.Add(J("moldtrans", "Transport & Logistică", "Junior Logistician",
            "Prima poziție în logistică pentru absolvenți. Vom forma un specialist complet în 6-12 luni.",
            "Studii în logistică, economie sau tehnic. Excel de bază. Motivație și dorință de a învăța.",
            "Asistezi la coordonarea transporturilor. Pregătești documente. Introduci date în sistemul WMS.",
            "Ungheni, Moldova", JobType.OnSite, EmploymentType.FullTime, 9000, 14000, false,
            ["Logistică", "Microsoft Office"]));

        // ────────────────────────────────────────────────────────
        // 13. VÂNZĂRI & RETAIL (10 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("retailmd", "Vânzări & Retail", "Manager Vânzări B2B",
            "Manager vânzări pentru segmentul corporate al rețelei noastre de magazine. Portofoliu 50+ clienți.",
            "4+ ani vânzări B2B. Cunoaștere CRM. Negociere și prezentări la nivel executiv. Permis de conducere.",
            "Gestionezi și extinzi portofoliul de clienți corporate. Negociezi contracte. Atingi targetele lunare.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 18000, 35000, true,
            ["Negociere", "CRM", "Prezentări", "Managementul clienților"]));

        jobs.Add(J("retailmd", "Vânzări & Retail", "Director Magazin",
            "Director pentru unul din magazinele noastre din Chișinău. Echipă de 25 angajați, suprafață 500 mp.",
            "4+ ani retail, din care 2 management magazin. Leadership. Cunoaștere KPI retail (conversion, UPT, ATV).",
            "Coordonezi echipa magazinului. Atingi targetele de vânzări. Asiguri experiența clienților. Raportezi zilnic.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 20000, 36000, false,
            ["Leadership", "Managementul clienților", "Comunicare"]));

        jobs.Add(J("retailmd", "Vânzări & Retail", "Consultant Vânzări (Showroom)",
            "Consultant vânzări pentru showroom-ul nostru de electronice și electrocasnice.",
            "1+ ani vânzări retail. Cunoaștere produse electronice avantaj. Comunicare și orientare spre client.",
            "Consiliezi clienții în alegerea produselor. Efectuezi vânzări și up-sell. Gestionezi reclamațiile.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 9000, 16000, true,
            ["Negociere", "Managementul clienților", "Comunicare"]));

        jobs.Add(J("retailmd", "Vânzări & Retail", "Key Account Manager",
            "KAM pentru gestionarea celor mai importanți 20 clienți ai companiei noastre. Relații pe termen lung.",
            "4+ ani KAM sau vânzări senior. Experiență cu contracte complexe. Orientare spre parteneriat strategic.",
            "Gestionezi relația cu clienții cheie. Identifici oportunități de creștere. Negociezi contractele anuale.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 22000, 40000, false,
            ["Negociere", "CRM", "Managementul clienților", "Prezentări"]));

        jobs.Add(J("retailmd", "Vânzări & Retail", "Specialist E-commerce",
            "Specialist pentru platforma noastră de e-commerce în creștere. Catalog, promoții, UX.",
            "2+ ani e-commerce. Cunoaștere Magento, WooCommerce sau Shopify. Google Analytics. SEO de bază.",
            "Gestionezi catalogul de produse online. Creezi promoții și campanii. Analizezi datele de conversie.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 14000, 25000, false,
            ["SEO", "Google Analytics", "CRM"]));

        jobs.Add(J("retailmd", "Vânzări & Retail", "Merchandiser",
            "Merchandiser care asigură prezența și vizibilitatea produselor în rețeaua noastră de magazine.",
            "1+ ani merchandising sau retail. Cunoaștere principii display comercial. Mobilitate (permis de conducere).",
            "Vizitezi magazinele din portofoliu. Verifici plasarea și stocul produselor. Raportezi situația la teren.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 10000, 18000, false,
            ["Managementul clienților", "Comunicare"]));

        jobs.Add(J("retailmd", "Vânzări & Retail", "Casier / Operator Vânzări",
            "Casier pentru magazinele noastre. Program flexibil, colectiv tânăr, bonusuri de performanță.",
            "Experientă de casier sau vânzări avantaj, nu obligatorie. Onestitate și seriozitate.",
            "Efectuezi operațiunile de casă. Consiliezi clienții. Menții curățenia și ordinea la casa de marcat.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 7500, 12000, false,
            ["Comunicare"]));

        jobs.Add(J("retailmd", "Vânzări & Retail", "Category Manager",
            "Category Manager responsabil de o categorie de produse: achiziții, pricing, promoții, performanță.",
            "3+ ani retail sau FMCG, ideally category management. Analiză de date. Cunoaștere furnizori.",
            "Gestionezi portofoliul categoriei tale. Negociezi cu furnizorii. Stabilești prețurile și promoțiile.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 18000, 32000, false,
            ["Negociere", "Excel avansat", "Managementul clienților"]));

        jobs.Add(J("retailmd", "Vânzări & Retail", "Sales Trainer",
            "Trainer de vânzări care formează și dezvoltă echipele de vânzări din rețeaua noastră.",
            "3+ ani vânzări + 1 an training. Abilități de facilitare și coaching. Energie și entuziasm.",
            "Proiectezi și susții training-uri de vânzări. Însoțești consultanții la teren. Evaluezi progresul.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 16000, 28000, false,
            ["Comunicare", "Prezentări", "Negociere"]));

        jobs.Add(J("retailmd", "Vânzări & Retail", "Reprezentant Vânzări (Teren)",
            "Reprezentant vânzări cu mașina companiei, rută zilnică în Chișinău și împrejurimi.",
            "1+ ani vânzări teren sau reprezintare comercială. Permis B obligatoriu. Comunicare și persuasiune.",
            "Vizitezi clienții existenți și atragi clienți noi. Prezinți produsele și promoțiile. Colectezi comenzile.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 10000, 20000, true,
            ["Negociere", "Managementul clienților", "CRM"]));

        // ────────────────────────────────────────────────────────
        // 14. CONSTRUCȚII & IMOBILIARE (8 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("constructmd", "Construcții & Imobiliare", "Inginer Construcții / Șef Șantier",
            "Șef șantier pentru proiect rezidențial 10 etaje în Chișinău. Echipă de 60 muncitori.",
            "5+ ani în construcții, din care 2+ management șantier. Cunoaștere norme construcții RM. AutoCAD.",
            "Coordonezi activitatea zilnică pe șantier. Asiguri calitatea lucrărilor. Gestionezi subcontractorii.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 20000, 38000, false,
            ["Management șantier", "AutoCAD", "Deviz"]));

        jobs.Add(J("constructmd", "Construcții & Imobiliare", "Arhitect",
            "Arhitect pentru proiectarea blocurilor rezidențiale și spațiilor comerciale. Birou modern.",
            "Licență arhitectură. 3+ ani proiectare. Expert AutoCAD și Revit. Simț estetic și tehnic.",
            "Proiectezi planuri arhitecturale. Colaborezi cu inginerii de structuri și instalații. Obții avizele.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 18000, 35000, false,
            ["AutoCAD", "Revit", "BIM", "Management șantier"]));

        jobs.Add(J("constructmd", "Construcții & Imobiliare", "Devizier (Estimator Costuri)",
            "Devizier care elaborează devizele de lucrări pentru proiecte rezidențiale și comerciale.",
            "3+ ani deviz construcții. Cunoaștere normative RM (NMAM, NTE). Software deviz (DevWin sau similar).",
            "Elaborezi devize și antemăsurători. Analizezi documentele de proiect. Participi la licitații.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 16000, 28000, false,
            ["Deviz", "AutoCAD", "Microsoft Office"]));

        jobs.Add(J("constructmd", "Construcții & Imobiliare", "Agent Imobiliar",
            "Agent imobiliar pentru vânzarea apartamentelor din proiectele proprii ale companiei noastre.",
            "1+ ani imobiliare sau vânzări. Cunoaștere piața imobiliară din Chișinău. Comunicare și persuasiune.",
            "Prezinți și vinzi apartamentele companiei. Calificezi cumpărătorii. Asistești la semnarea contractelor.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 10000, 30000, true,
            ["Negociere", "Comunicare", "CRM"]));

        jobs.Add(J("constructmd", "Construcții & Imobiliare", "Inginer Instalații (MEP)",
            "Inginer MEP pentru proiectarea și supervizarea instalațiilor sanitare, termice și electrice.",
            "3+ ani proiectare MEP. AutoCAD/Revit MEP. Cunoaștere normative tehnice RM.",
            "Proiectezi instalațiile MEP ale clădirilor. Supervizezi execuția pe șantier. Efectuezi recepțiile.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 18000, 32000, false,
            ["AutoCAD", "Revit", "BIM"]));

        jobs.Add(J("constructmd", "Construcții & Imobiliare", "Manager Proiect Construcții",
            "PM pentru coordonarea proiectelor de construcție de la proiectare la recepție finală.",
            "5+ ani construcții, din care 2+ management proiect. PMP avantaj. Bugetare și programare lucrări.",
            "Planifici și monitorizezi proiectul. Gestionezi bugetul și termenele. Coordonezi echipele și subcontractorii.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 25000, 45000, false,
            ["Management șantier", "Managementul proiectelor", "AutoCAD"]));

        jobs.Add(J("constructmd", "Construcții & Imobiliare", "Maistru Construcții",
            "Maistru pentru coordonarea echipelor de muncitori pe șantierele rezidențiale.",
            "5+ ani construcții civile. Capacitate de coordonare echipe. Cunoaștere tehnologii de construcție.",
            "Coordonezi echipele de muncitori. Verifici calitatea lucrărilor. Raportezi șefului de șantier.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 15000, 26000, false,
            ["Management șantier"]));

        jobs.Add(J("constructmd", "Construcții & Imobiliare", "Specialist Achiziții (Materiale de Construcție)",
            "Specialist achiziții pentru aprovizionarea cu materiale a proiectelor noastre de construcție.",
            "2+ ani achiziții, preferabil construcții. Negociere cu furnizori. Cunoaștere materiale de construcție.",
            "Identifici și negociezi cu furnizorii de materiale. Gestionezi contractele de aprovizionare. Urmărești livrările.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 14000, 24000, false,
            ["Negociere", "Microsoft Office"]));

        // ────────────────────────────────────────────────────────
        // 15. INGINERIE & TEHNIC (8 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("constructmd", "Inginerie & Tehnic", "Inginer Electrician (Instalații Industriale)",
            "Inginer electrician pentru proiectarea și punerea în funcțiune a instalațiilor industriale.",
            "3+ ani instalații electrice industriale. AutoCAD Electrical. Cunoaștere norme NORME electrice RM.",
            "Proiectezi instalațiile electrice. Supervizezi execuția. Efectuezi puneri în funcțiune.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 16000, 30000, false,
            ["AutoCAD Electrical", "Mentenanță"]));

        jobs.Add(J("constructmd", "Inginerie & Tehnic", "Programator PLC (Automatizări Industriale)",
            "Programator PLC pentru automatizarea liniilor de producție la clienții noștri industriali.",
            "2+ ani programare PLC (Siemens S7, Allen Bradley sau similar). SCADA. Experientă în mediu industrial.",
            "Programezi și configurezi PLC-urile. Implementezi sisteme SCADA. Asiguri mentenanța și depanarea.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 18000, 35000, false,
            ["PLC", "SCADA", "Mentenanță"]));

        jobs.Add(J("constructmd", "Inginerie & Tehnic", "Tehnician de Service (Echipamente IT)",
            "Tehnician care asigură mentenanța și repararea echipamentelor IT la clienții noștri.",
            "2+ ani tehnic IT. Cunoaștere hardware PC, rețele, imprimante. Permis de conducere.",
            "Intervii la clienți pentru depanare. Instalezi și configurezi echipamente. Documentezi lucrările.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 12000, 20000, false,
            ["Mentenanță", "Comunicare"]));

        jobs.Add(J("constructmd", "Inginerie & Tehnic", "Inginer Mecanic (Utilaje)",
            "Inginer mecanic pentru mentenanța utilajelor de construcție (excavatoare, macarale, betoniere).",
            "3+ ani mentenanță utilaje grele. Cunoaștere hidraulică și pneumatică industrială. Desenare tehnică.",
            "Efectuezi mentenanța preventivă și corectivă. Diagnostichezi avariile. Gestionezi piesele de schimb.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 16000, 28000, false,
            ["Mentenanță", "Desenare tehnică"]));

        jobs.Add(J("constructmd", "Inginerie & Tehnic", "Inginer Calitate (QC/QA)",
            "Inginer calitate pentru implementarea și menținerea sistemului de management al calității ISO 9001.",
            "3+ ani calitate în producție sau construcții. ISO 9001. Audit intern. Gândire analitică.",
            "Implementezi și menții sistemul QMS. Efectuezi audituri interne. Raportezi neconformitățile.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 15000, 28000, false,
            ["Mentenanță", "Desenare tehnică"]));

        jobs.Add(J("constructmd", "Inginerie & Tehnic", "Inginer HSE (Sănătate & Securitate în Muncă)",
            "Responsabil HSE pentru șantierele și operațiunile companiei noastre de construcții.",
            "3+ ani HSE în construcții sau industrie. Cunoaștere legislației SSM din Moldova. Certificare SSM.",
            "Implementezi politicile de securitate în muncă. Efectuezi instruiri și audituri de securitate.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 14000, 26000, false,
            ["Mentenanță", "Comunicare"]));

        jobs.Add(J("constructmd", "Inginerie & Tehnic", "Desenator Tehnic (AutoCAD)",
            "Desenator tehnic pentru elaborarea planurilor de execuție și detaliilor constructive.",
            "2+ ani AutoCAD 2D/3D. Cunoaștere norme desenare tehnică. Atenție la detalii și precizie.",
            "Elaborezi planuri tehnice în AutoCAD. Modifici planurile la solicitarea inginerilor. Menții arhiva de desene.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 12000, 20000, false,
            ["AutoCAD Electrical", "Desenare tehnică", "Microsoft Office"]));

        jobs.Add(J("constructmd", "Inginerie & Tehnic", "Inginer Geodez / Topograf",
            "Topograf pentru ridicări și trasări pe șantierele noastre din Chișinău și regiune.",
            "Licență în geodezie sau domeniu conex. 2+ ani topografie. Cunoaștere stație totală și GPS geodezic.",
            "Efectuezi ridicări topografice. Trasezi axele construcțiilor. Elaborezi planuri topografice.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 15000, 26000, false,
            ["AutoCAD Electrical", "Desenare tehnică"]));

        // ────────────────────────────────────────────────────────
        // 16. ADMINISTRATIV / OFICIU (8 joburi)
        // ────────────────────────────────────────────────────────

        jobs.Add(J("bizmoldova", "Administrativ / Oficiu", "Asistent Manager / Executive Assistant",
            "Asistent pentru directorul general al companiei. Rol variat: agenda, corespondență, organizare.",
            "2+ ani asistent manager sau secretariat. MS Office avansat. Discreție și profesionalism. Engleza B2.",
            "Gestionezi agenda directorului. Organizezi ședințele și deplasările. Redactezi corespondența oficială.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 12000, 22000, false,
            ["Secretariat", "Gestiune documente", "Microsoft Office", "Comunicare"]));

        jobs.Add(J("bizmoldova", "Administrativ / Oficiu", "Operator Date / Introducere Date",
            "Operator pentru procesarea și introducerea datelor în sistemele companiei. Volum mare, precizie necesară.",
            "Operare calculator bună. Excel de bază. Atenție la detalii și viteză de lucru. Serios și punctual.",
            "Introduci și verifici date în sistem. Procesezi documente. Raportezi discrepanțele supervizorului.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 8000, 13000, false,
            ["Microsoft Office", "Gestiune documente"]));

        jobs.Add(J("hrprime", "Administrativ / Oficiu", "Recepționer / Office Manager",
            "Recepționer pentru agenția noastră de recrutare. Fața companiei, primul contact cu vizitatorii.",
            "Prezentare îngrijită. Comunicare excelentă. MS Office. Cunoaștere limbilor română și rusă obligatorii.",
            "Primești vizitatorii și candidații. Gestionezi apelurile telefonice. Asistezi echipa cu sarcini administrative.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 9000, 15000, false,
            ["Recepție", "Comunicare", "Microsoft Office", "Secretariat"]));

        jobs.Add(J("bizmoldova", "Administrativ / Oficiu", "Specialist Achiziții (Procurement)",
            "Specialist achiziții pentru departamentul de procurement al companiei noastre de consultanță.",
            "2+ ani achiziții. Cunoaștere proceduri de achiziție. Negociere cu furnizori. MS Office.",
            "Identifici și evaluezi furnizorii. Negociezi prețurile și condițiile. Gestionezi comenzile și contractele.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 13000, 22000, false,
            ["Negociere", "Gestiune documente", "Microsoft Office", "Coordonare"]));

        jobs.Add(J("hrprime", "Administrativ / Oficiu", "Coordonator Administrativ",
            "Coordonator care gestionează activitățile administrative ale biroului nostru: facilități, furnizori, evenimente.",
            "3+ ani administrare sau office management. Organizare impecabilă. MS Office. Proactiv.",
            "Coordonezi furnizorii de servicii (curățenie, securitate, IT). Organizezi evenimentele interne. Gestionezi bugetul admin.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 12000, 20000, false,
            ["Coordonare", "Gestiune documente", "Microsoft Office"]));

        jobs.Add(J("bizmoldova", "Administrativ / Oficiu", "Specialist Documente & Arhivare",
            "Specialist responsabil de arhivarea și gestiunea documentelor companiei conform cerințelor legale.",
            "2+ ani gestiune documente sau arhivare. Cunoaștere nomenclatorului arhivistic RM. Ordine și sistematizare.",
            "Arhivezi documentele fizice și electronice. Menții nomenclatorul arhivistic. Asiguri accesul la documente.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 9000, 15000, false,
            ["Gestiune documente", "Secretariat", "Microsoft Office"]));

        jobs.Add(J("hrprime", "Administrativ / Oficiu", "Traducător / Interpret (Română-Rusă-Engleză)",
            "Traducător pentru traducerea documentelor oficiale și interpretariat în ședințe de business.",
            "Studii filologie sau limbi străine. Cunoaștere perfectă română, rusă și engleză. Acuratețe și rapiditate.",
            "Traduci documente oficiale, contracte și materiale de marketing. Interpretezi în ședințe și negocieri.",
            "Chișinău, Moldova", JobType.Hybrid, EmploymentType.FullTime, 12000, 22000, false,
            ["Comunicare", "Microsoft Office"]));

        jobs.Add(J("bizmoldova", "Administrativ / Oficiu", "Administrator Sistem (IT Support intern)",
            "Administrator de sistem care asigură funcționarea IT a biroului nostru de 50 de angajați.",
            "2+ ani IT support sau system administration. Windows Server, Active Directory, Office 365. Linux avantaj.",
            "Asiguri funcționarea rețelei și echipamentelor IT. Rezolvi ticketele utilizatorilor. Gestionezi licențele.",
            "Chișinău, Moldova", JobType.OnSite, EmploymentType.FullTime, 14000, 24000, false,
            ["Mentenanță", "Linux", "Microsoft Office"]));

        return jobs;
    }
}